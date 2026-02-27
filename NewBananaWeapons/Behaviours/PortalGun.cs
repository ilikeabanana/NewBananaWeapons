using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULTRAKILL.Portal;
using ULTRAKILL.Portal.Geometry;
using UnityEngine;
using ULTRAKILL.Portal.Native;


public class PortalGun : BaseWeapon
{
    // Portal half-extents: width = 1.25, height = 2.5
    private static readonly Vector3 PortalSize = new Vector3(1.25f, 2.5f, 0.1f);

    // How many times to try nudging the portal toward the surface center per placement
    private const int SnapIterations = 12;
    // How far to nudge per iteration when a corner is invalid (metres)
    private const float SnapStepSize = 0.12f;
    // Maximum cumulative snapping distance before we give up
    private const float MaxSnapDistance = 2.0f;

    Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public override void SetupConfigs(string sectionName, ConfigFile Config)
    {
        base.SetupConfigs(sectionName, Config);
    }

    // ─────────────────────────────────────────────────────────────────────
    //  Lifecycle
    // ─────────────────────────────────────────────────────────────────────

    void Update()
    {
        PortalAttempt();
    }

    GameObject quad2;
    GameObject quad1;
    bool alrSetupPortals;

    // ─────────────────────────────────────────────────────────────────────
    //  Input handling
    // ─────────────────────────────────────────────────────────────────────

    void PortalAttempt()
    {
        if (CameraController.Instance == null) return;
        if (!GunControl.Instance.activated) return;

        PhysicsCastResult hit = aimHit;

        if (InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame)
        {
            anim.SetTrigger("BlueShot");
            UpdatePortal(ref quad1, "Portal_Entry", hit);
            if (quad2 != null && !alrSetupPortals) SetupPortals();
        }
        else if (InputManager.Instance.InputSource.Fire2.WasPerformedThisFrame)
        {
            anim.SetTrigger("OrangeShot");
            UpdatePortal(ref quad2, "Portal_Exit", hit);
            if (quad1 != null && !alrSetupPortals) SetupPortals();
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  Portal placement
    // ─────────────────────────────────────────────────────────────────────

    void UpdatePortal(ref GameObject portalObj, string name, PhysicsCastResult hit)
    {
        // Compute the correct orientation before we do any snapping so that
        // floor/ceiling portals already carry the player-relative "up".
        Quaternion portalRotation = ComputePortalRotation(hit);

        // Try to nudge the portal centre onto a fully-valid patch of surface.
        if (!TrySnapToSurface(hit, portalRotation, out Vector3 snappedPosition))
        {
            Debug.Log("Surface invalid: could not find a large enough flat patch.");
            return;
        }

        if (IsSpaceOccupied(snappedPosition, PortalSize, portalObj))
        {
            Debug.Log("Surface invalid: Another portal is already here.");
            return;
        }

        if (portalObj == null)
        {
            portalObj = new GameObject(name);
            portalObj.AddComponent<BoxCollider>().isTrigger = true;
            portalObj.transform.localScale = new Vector3(1.25f, 2.5f, 1f);
            portalObj.SetActive(false);
        }

        portalObj.transform.rotation = portalRotation;
        // Slight offset along normal to prevent Z-fighting / clipping
        portalObj.transform.position = snappedPosition + hit.normal * 0.05f;

        portalObj.GetOrAddComponent<PortalCollisionFixer>().FixCols();
    }

    // ─────────────────────────────────────────────────────────────────────
    //  Rotation helpers
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Compute the portal's world rotation.
    /// • Walls  → forward faces away from the wall (standard behaviour).
    /// • Floor/Ceiling → forward still points away from the surface, but the
    ///   portal's "up" axis is aligned with the player's horizontal look
    ///   direction so the portal is always oriented toward the shooter.
    /// </summary>
    Quaternion ComputePortalRotation(PhysicsCastResult hit)
    {
        Vector3 forward = -hit.normal; // portal faces toward the player

        bool isFloorOrCeiling = Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) > 0.9f;

        if (isFloorOrCeiling)
        {
            // Project the camera's forward direction onto the surface plane so the
            // portal's "up" matches where the player is looking horizontally.
            Vector3 camForward = CameraController.Instance.transform.forward;
            Vector3 projectedUp = Vector3.ProjectOnPlane(camForward, hit.normal).normalized;

            // Guard against a perfectly vertical look (rare but possible).
            if (projectedUp.sqrMagnitude < 0.001f)
                projectedUp = Vector3.ProjectOnPlane(CameraController.Instance.transform.right, hit.normal).normalized;

            return Quaternion.LookRotation(forward, projectedUp);
        }
        else
        {
            // For walls use a stable world-up reference.
            return Quaternion.LookRotation(forward);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  Edge snapping
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Starting from the raw hit point, iteratively slide the portal centre
    /// along the surface plane until all four corners land on the same flat
    /// geometry.  Returns false only if no valid position can be found within
    /// <see cref="MaxSnapDistance"/> of the original hit.
    /// </summary>
    bool TrySnapToSurface(PhysicsCastResult hit, Quaternion rotation, out Vector3 result)
    {
        // Derive the portal's local right/up axes from the chosen rotation.
        Vector3 right = rotation * Vector3.right;
        Vector3 up = rotation * Vector3.up;

        Vector3 centre = hit.point;
        Vector3 origin = hit.point; // keep for distance budget check

        for (int iter = 0; iter < SnapIterations; iter++)
        {
            Vector3 nudge = Vector3.zero;
            bool allValid = true;

            foreach (Vector3 cornerOffset in CornerOffsets(right, up))
            {
                Vector3 cornerWorld = centre + cornerOffset;
                CornerStatus status = CheckCorner(cornerWorld, hit.normal);

                if (status != CornerStatus.Valid)
                {
                    // Push the centre away from the failing corner (i.e. inward).
                    nudge -= cornerOffset.normalized * SnapStepSize;
                    allValid = false;
                }
            }

            if (allValid)
            {
                result = centre;
                return true;
            }

            // Move the centre by the aggregate nudge.
            centre += nudge;

            // Bail out if we have wandered too far from where the player aimed.
            if (Vector3.Distance(centre, origin) > MaxSnapDistance)
                break;
        }

        // Do one last full check at the final snapped position.
        bool finalValid = true;
        foreach (Vector3 cornerOffset in CornerOffsets(right, up))
        {
            if (CheckCorner(centre + cornerOffset, hit.normal) != CornerStatus.Valid)
            {
                finalValid = false;
                break;
            }
        }

        result = centre;
        return finalValid;
    }

    /// <summary>Returns the four corner offsets for a portal of <see cref="PortalSize"/>.</summary>
    IEnumerable<Vector3> CornerOffsets(Vector3 right, Vector3 up)
    {
        float hw = PortalSize.x * 0.5f; // half-width
        float hh = PortalSize.y * 0.5f; // half-height
        yield return right * hw + up * hh;
        yield return -right * hw + up * hh;
        yield return right * hw - up * hh;
        yield return -right * hw - up * hh;
    }

    enum CornerStatus { Valid, EdgeOrGap, CurvedOrDifferentSurface }

    /// <summary>
    /// Fires a ray from just in front of the wall back toward it to verify
    /// the corner lands on the same flat surface as the portal centre.
    /// </summary>
    CornerStatus CheckCorner(Vector3 cornerWorld, Vector3 surfaceNormal)
    {
        Ray ray = new Ray(cornerWorld + surfaceNormal * 0.5f, -surfaceNormal);

        if (!Physics.Raycast(ray, out RaycastHit hit, 1f, LayerMaskDefaults.Get(LMD.Environment)))
            return CornerStatus.EdgeOrGap; // ray missed → we're over an edge

        // Surface is curved or belongs to a different object if the normal differs.
        if (Vector3.Angle(surfaceNormal, hit.normal) > 2.0f)
            return CornerStatus.CurvedOrDifferentSurface;

        // The surface has an unexpected depth offset → gap or protruding geometry.
        if (Mathf.Abs(hit.distance - 0.5f) > 0.1f)
            return CornerStatus.CurvedOrDifferentSurface;

        return CornerStatus.Valid;
    }

    // ─────────────────────────────────────────────────────────────────────
    //  Overlap check
    // ─────────────────────────────────────────────────────────────────────

    private bool IsSpaceOccupied(Vector3 position, Vector3 size, GameObject currentPortal)
    {
        Collider[] colliders = Physics.OverlapBox(position, size * 0.5f);
        foreach (var col in colliders)
        {
            if (currentPortal != null &&
                (col.gameObject == currentPortal || col.transform.IsChildOf(currentPortal.transform)))
                continue;

            if (col.GetComponentInParent<Portal>() != null ||
                col.GetComponentInParent<PortalIdentifier>() != null)
                return true;
        }
        return false;
    }

    // ─────────────────────────────────────────────────────────────────────
    //  Aim raycast
    // ─────────────────────────────────────────────────────────────────────

    PhysicsCastResult aimHit
    {
        get
        {
            Transform camTrans = CameraController.Instance.transform;
            PhysicsCastResult hit;

            if (PortalPhysicsV2.Raycast(camTrans.position, camTrans.forward, out hit, 100,
                    LayerMaskDefaults.Get(LMD.Environment)))
            {
                return hit;
            }

            hit = new PhysicsCastResult();
            hit.point = camTrans.position + camTrans.forward * 100;
            hit.normal = -camTrans.forward;
            return hit;
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  Portal initialisation
    // ─────────────────────────────────────────────────────────────────────

    public void SetupPortals()
    {
        alrSetupPortals = true;

        var fixer1 = quad1.GetOrAddComponent<PortalCollisionFixer>();
        var fixer2 = quad2.GetOrAddComponent<PortalCollisionFixer>();
        fixer1.partner = fixer2;
        fixer2.partner = fixer1;

        Portal portal1 = quad1.AddComponent<Portal>();
        portal1.shape = new PlaneShape { width = 3.75f, height = 7.5f };
        portal1.entry = quad2.transform;
        portal1.exit = quad1.transform;
        portal1.supportInfiniteRecursion = true;
        portal1.appearsInRecursions = true;
        portal1.canSeeItself = true;
        portal1.clippingMethod = PortalClippingMethod.Default;
        portal1.maxRecursions = 3;
        portal1.renderSettings = PortalSideFlags.Enter | PortalSideFlags.Exit | PortalSideFlags.None;
        portal1.useFogEnter = true;
        portal1.useFogExit = true;
        portal1.canSeePortalLayer = true;

        StartCoroutine(applyFunnies(portal1, fixer1, fixer2));

        PortalIdentifier portalIdent = quad2.AddComponent<PortalIdentifier>();
        portalIdent.isTraversable = true;

        quad1.SetActive(true);
        quad2.SetActive(true);
    }

    IEnumerator applyFunnies(Portal portal1, PortalCollisionFixer fixer1, PortalCollisionFixer fixer2)
    {
        yield return new WaitForEndOfFrame();

        if (portal1.onEntryTravel == null) portal1.onEntryTravel = new UnityEventPortalTravel();
        if (portal1.onExitTravel == null) portal1.onExitTravel = new UnityEventPortalTravel();

        portal1.onEntryTravel.AddListener((x, y) =>
        {
            if (x.travellerType != PortalTravellerType.PLAYER) return;
            if (fixer2 == null) return;
            if (fixer2.isOnFloor)
                NewMovement.Instance.transform.position += x.travellerVelocity.normalized * 2;
        });

        portal1.onExitTravel.AddListener((x, y) =>
        {
            if (x.travellerType != PortalTravellerType.PLAYER) return;
            if (fixer1 == null) return;
            if (fixer1.isOnFloor)
                NewMovement.Instance.transform.position += x.travellerVelocity.normalized * 2;
        });
    }
}
