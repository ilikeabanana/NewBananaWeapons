using NewBananaWeapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ULTRAKILL.Cheats;
using ULTRAKILL.Portal;
using ULTRAKILL.Portal.Geometry;
using ULTRAKILL.Portal.Native;

public class PortalGun : BaseWeapon
{
    private static readonly Vector3 PortalSize = new Vector3(1.25f, 2.5f, 0.1f);
    private const int SnapIterations = 12;
    private const float SnapStepSize = 0.12f;
    private const float MaxSnapDistance = 2.0f;

    public AudioClip orangeSound;
    public AudioClip blueSound;
    public GameObject blueTube;
    public GameObject orangeTube;
    Animator anim;
    AudioSource source;

    static ConfigVar<bool> changeGravity;
    public static ConfigVar<float> aspectRatio;
    public static ConfigVar<float> sizeMult;
    public static ConfigVar<float> range;
    public static ConfigVar<bool> DEBUG;
    static ConfigVar<Color> portal1Color;
    static ConfigVar<Color> portal2Color;

    public void FireBlue()
    {
        source.pitch = Random.Range(0.95f, 1.15f);
        orangeTube.SetActive(false);
        blueTube.SetActive(true);
        source.PlayOneShot(blueSound);
    }

    public void FireOrange()
    {
        source.pitch = Random.Range(0.95f, 1.15f);
        orangeTube.SetActive(true);
        blueTube.SetActive(false);
        source.PlayOneShot(orangeSound);
    }

    void Awake()
    {
        anim = GetComponent<Animator>();
        source = GetComponent<AudioSource>();
    }

    public override void SetupConfigs(string sectionName)
    {
        changeGravity = new ConfigVar<bool>(sectionName, "Gravity Changing Portals", false, "");
        sizeMult = new ConfigVar<float>(sectionName, "Size Mult", 1.25f, "");
        range = new ConfigVar<float>(sectionName, "Range", 100, "");
        aspectRatio = new ConfigVar<float>(sectionName, "Aspect Ratio", 2f, "");
        portal1Color = new ConfigVar<Color>(sectionName, "Portal 1 Color", new Color(0.153f, 0.655f, 0.847f), "");
        portal2Color = new ConfigVar<Color>(sectionName, "Portal 2 Color", new Color(1.0f, 0.604f, 0.0f), "");
        DEBUG = new ConfigVar<bool>(sectionName, "DEBUG", false, "This is to show the bounds of the portals");
    }
    void UpdatePortalProperties()
    {
        if (quad1 != null)
        {
            quad1.transform.localScale = new Vector3(
                1.25f * sizeMult.Value,
                1.25f * sizeMult.Value * aspectRatio.Value,
                1f * sizeMult.Value);

            Portal p = quad1.GetComponent<Portal>();
            if (p != null)
                p.shape = new PlaneShape { width = 3.75f * sizeMult.Value, height = 3.75f * sizeMult.Value * aspectRatio.Value };
        }

        if (quad2 != null)
        {
            quad2.transform.localScale = new Vector3(
                1.25f * sizeMult.Value,
                1.25f * sizeMult.Value * aspectRatio.Value,
                1f * sizeMult.Value);

            Portal p = quad2.GetComponent<Portal>();
            if (p != null)
                p.shape = new PlaneShape { width = 3.75f * sizeMult.Value, height = 3.75f * sizeMult.Value * aspectRatio.Value };
        }

        if (quad1Outline != null)
        {
            quad1Outline.transform.localScale = new Vector3(
                3.75f * sizeMult.Value,
                (3.75f * sizeMult.Value * aspectRatio.Value) - 0.5f,
                0.01f) * 1.2f;

            quad1Outline.GetComponent<Renderer>().material.color = portal1Color.Value;
        }

        if (quad2Outline != null)
        {
            quad2Outline.transform.localScale = new Vector3(
                3.75f * sizeMult.Value,
                (3.75f * sizeMult.Value * aspectRatio.Value) - 0.5f,
                0.01f) * 1.2f;

            quad2Outline.GetComponent<Renderer>().material.color = portal2Color.Value;
        }
    }


    void Update()
    {
        PortalAttempt();
        UpdatePortalProperties();
    }

    GameObject quad2;
    GameObject quad2Outline;
    GameObject quad1;
    GameObject quad1Outline;
    bool alrSetupPortals;

    void PortalAttempt()
    {
        if (CameraController.Instance == null || !GunControl.Instance.activated) return;
        PhysicsCastResult hit = aimHit;

        if (InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame)
        {
            FireBlue();
            anim.SetTrigger("Shoot");
            UpdatePortal(ref quad1, ref quad1Outline, "Portal_Entry", hit, portal1Color.Value);
            if (quad2 != null && !alrSetupPortals) SetupPortals();
        }
        else if (InputManager.Instance.InputSource.Fire2.WasPerformedThisFrame)
        {
            FireOrange();
            anim.SetTrigger("Shoot");
            UpdatePortal(ref quad2, ref quad2Outline, "Portal_Exit", hit, portal2Color.Value);
            if (quad1 != null && !alrSetupPortals) SetupPortals();
        }
    }

    void UpdatePortal(ref GameObject portalObj, ref GameObject outlineObj, string name, PhysicsCastResult hit, Color portalColor)
    {
        GameObject lineObject = new GameObject();
        lineObject.transform.position = CameraController.Instance.transform.position;
        lineObject.transform.forward = CameraController.Instance.transform.forward;
        LineRenderer lr = lineObject.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.material.shader = AddressableManager.unlit;
        lr.material.color = portalColor;
        lineObject.AddComponent<PortalBeam>().shootColor = portalColor;

        Quaternion portalRotation = ComputePortalRotation(hit);
        if (!TrySnapToSurface(hit, portalRotation, out Vector3 snappedPosition)) return;
        if (IsSpaceOccupied(snappedPosition, PortalSize, portalObj)) return;

        if (portalObj == null)
        {
            portalObj = new GameObject(name);
            portalObj.AddComponent<BoxCollider>().isTrigger = true;
            portalObj.transform.localScale = new Vector3(1.25f * sizeMult.Value, 1.25f * sizeMult.Value * aspectRatio.Value, 1f * sizeMult.Value);
            portalObj.SetActive(false);
        }

        if (outlineObj == null)
        {
            outlineObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            outlineObj.GetComponent<Renderer>().material.shader = AddressableManager.unlit;
            outlineObj.GetComponent<Renderer>().material.color = portalColor;
            Destroy(outlineObj.GetComponent<Collider>());
            outlineObj.transform.localScale = new Vector3(3.75f * sizeMult.Value, (3.75f * sizeMult.Value * aspectRatio.Value) - 0.5f, 0.01f) * 1.2f;
        }

        portalObj.transform.rotation = portalRotation;
        outlineObj.transform.rotation = portalRotation;
        outlineObj.transform.position = snappedPosition + hit.normal * 0.025f;
        portalObj.transform.position = snappedPosition + hit.normal * 0.05f;

        PortalCollisionFixer fixer = portalObj.GetOrAddComponent<PortalCollisionFixer>();
        fixer.placedOnWall = hit.collider.gameObject;
        fixer.FixCols();
    }

    Quaternion ComputePortalRotation(PhysicsCastResult hit)
    {
        Vector3 forward = -hit.normal;
        if (Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) > 0.9f)
        {
            Vector3 camForward = CameraController.Instance.transform.forward;
            Vector3 projectedUp = Vector3.ProjectOnPlane(camForward, hit.normal).normalized;
            if (projectedUp.sqrMagnitude < 0.001f)
                projectedUp = Vector3.ProjectOnPlane(CameraController.Instance.transform.right, hit.normal).normalized;
            return Quaternion.LookRotation(forward, projectedUp);
        }
        return Quaternion.LookRotation(forward);
    }

    bool TrySnapToSurface(PhysicsCastResult hit, Quaternion rotation, out Vector3 result)
    {
        Vector3 right = rotation * Vector3.right;
        Vector3 up = rotation * Vector3.up;
        Vector3 centre = hit.point;
        Vector3 origin = hit.point;

        for (int iter = 0; iter < SnapIterations; iter++)
        {
            Vector3 nudge = Vector3.zero;
            bool allValid = true;
            foreach (Vector3 cornerOffset in CornerOffsets(right, up))
            {
                if (CheckCorner(centre + cornerOffset, hit.normal) != CornerStatus.Valid)
                {
                    nudge -= cornerOffset.normalized * SnapStepSize;
                    allValid = false;
                }
            }
            if (allValid) { result = centre; return true; }
            centre += nudge;
            if (Vector3.Distance(centre, origin) > MaxSnapDistance) break;
        }
        result = centre;
        return false;
    }

    IEnumerable<Vector3> CornerOffsets(Vector3 right, Vector3 up)
    {
        float hw = PortalSize.x * 0.5f;
        float hh = PortalSize.y * 0.5f;
        yield return right * hw + up * hh;
        yield return -right * hw + up * hh;
        yield return right * hw - up * hh;
        yield return -right * hw - up * hh;
    }

    enum CornerStatus { Valid, EdgeOrGap, CurvedOrDifferentSurface }
    CornerStatus CheckCorner(Vector3 cornerWorld, Vector3 surfaceNormal)
    {
        Ray ray = new Ray(cornerWorld + surfaceNormal * 0.5f, -surfaceNormal);
        if (!Physics.Raycast(ray, out RaycastHit hit, 1f, LayerMaskDefaults.Get(LMD.Environment))) return CornerStatus.EdgeOrGap;
        if (Vector3.Angle(surfaceNormal, hit.normal) > 2.0f) return CornerStatus.CurvedOrDifferentSurface;
        if (Mathf.Abs(hit.distance - 0.5f) > 0.1f) return CornerStatus.CurvedOrDifferentSurface;
        return CornerStatus.Valid;
    }

    private bool IsSpaceOccupied(Vector3 position, Vector3 size, GameObject currentPortal)
    {
        Collider[] colliders = Physics.OverlapBox(position, size * 0.5f);
        foreach (var col in colliders)
        {
            if (currentPortal != null && (col.gameObject == currentPortal || col.transform.IsChildOf(currentPortal.transform))) continue;
            if (col.GetComponentInParent<Portal>() != null || col.GetComponentInParent<PortalIdentifier>() != null) return true;
        }
        return false;
    }

    PhysicsCastResult aimHit
    {
        get
        {
            Transform camTrans = CameraController.Instance.transform;
            PhysicsCastResult hit;
            if (PortalPhysicsV2.Raycast(camTrans.position, camTrans.forward, out hit, range.Value, LayerMaskDefaults.Get(LMD.Environment))) return hit;
            hit = new PhysicsCastResult { point = camTrans.position + camTrans.forward * 100, normal = -camTrans.forward };
            return hit;
        }
    }

    public void SetupPortals()
    {
        alrSetupPortals = true;
        var fixer1 = quad1.GetOrAddComponent<PortalCollisionFixer>();
        var fixer2 = quad2.GetOrAddComponent<PortalCollisionFixer>();
        fixer1.partner = fixer2;
        fixer2.partner = fixer1;

        Portal portal1 = quad1.AddComponent<Portal>();
        portal1.shape = new PlaneShape { width = 3.75f * sizeMult.Value, height = 3.75f * sizeMult.Value * aspectRatio.Value };
        portal1.entry = quad2.transform;
        portal1.exit = quad1.transform;
        portal1.supportInfiniteRecursion = true;
        portal1.maxRecursions = 3;
        portal1.usePerceivedGravityOnEnter = changeGravity.Value;
        portal1.usePerceivedGravityOnExit = changeGravity.Value;

        StartCoroutine(applyFunnies(portal1, fixer1, fixer2));
        quad2.AddComponent<PortalIdentifier>().isTraversable = true;
        quad1.SetActive(true);
        quad2.SetActive(true);
    }

    IEnumerator applyFunnies(Portal portal1, PortalCollisionFixer fixer1, PortalCollisionFixer fixer2)
    {
        yield return new WaitForEndOfFrame();
        if (portal1.onEntryTravel == null) portal1.onEntryTravel = new UnityEventPortalTravel();
        if (portal1.onExitTravel == null) portal1.onExitTravel = new UnityEventPortalTravel();
        portal1.onEntryTravel.AddListener((x, y) => { if (x.travellerType == PortalTravellerType.PLAYER && fixer2.isOnFloor) NewMovement.Instance.transform.position += x.travellerVelocity.normalized * 2; });
        portal1.onExitTravel.AddListener((x, y) => { if (x.travellerType == PortalTravellerType.PLAYER && fixer1.isOnFloor) NewMovement.Instance.transform.position += x.travellerVelocity.normalized * 2; });
    }
}