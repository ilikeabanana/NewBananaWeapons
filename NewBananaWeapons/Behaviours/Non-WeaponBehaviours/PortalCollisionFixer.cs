using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NewBananaWeapons;
using HarmonyLib;

public class PortalCollisionFixer : MonoBehaviour
{
    private List<GameObject> targetWalls = new List<GameObject>();
    private List<Collider> wallColliders = new List<Collider>();
    public GameObject placedOnWall;
    private List<int> wallLayers = new List<int>();
    private Collider playerCollider;
    public float ghostRadius = 2.5f;
    public float reenableDelay = 0.05f;
    public PortalCollisionFixer partner;
    private bool selfGhosting = false;
    private bool partnerForced = false;
    private Coroutine reenableRoutine;

    // Tracks physics objects currently ghosting through this portal
    private List<Collider> ghostedPhysicsColliders = new List<Collider>();

    // -------------------------------------------------------------------------
    // Visualization settings
    // -------------------------------------------------------------------------
    public bool DEBUG = false;

    // Face rectangle shape tweaks (local space, portal face is nominally ±1.2)
    private const float VisHalfWidth = 1.35f;
    private const float VisHalfTop = 1.0f;
    private const float VisHalfBottom = 1.7f;

    private const float LineWidthRect = 0.07f; 
    private const float LineWidthDepth = 0.04f;

    private LineRenderer lrRect;
    private LineRenderer lrDepthBox;

    private static readonly Color ColIdle = new Color(0.2f, 0.8f, 1f, 0.6f);
    private static readonly Color ColGhost = new Color(0.1f, 1f, 0.3f, 0.9f);
    private static readonly Color ColFloor = new Color(1f, 0.7f, 0.1f, 0.8f);
    // -------------------------------------------------------------------------

    public bool isGhosting => selfGhosting || partnerForced;

    public static bool isInPortal;

    public bool isOnFloor
    {
        get
        {
            return Mathf.Abs(Vector3.Dot(transform.forward, Vector3.up)) > 0.9f;
        }
    }

    public void FixCols()
    {
        Vector3 inflatedSize = (transform.localScale / 2) + Vector3.one * 0.3f;

        Collider[] walls = Physics.OverlapBox(
            transform.position,
            inflatedSize,
            transform.rotation,
            LayerMaskDefaults.Get(LMD.Environment));

        targetWalls.Clear();
        wallColliders.Clear();
        wallLayers.Clear();

        if (walls.Length > 0)
        {
            if (NewMovement.Instance != null)
                playerCollider = NewMovement.Instance.GetComponent<Collider>();

            foreach (Collider wall in walls)
            {
                targetWalls.Add(wall.gameObject);
                wallColliders.Add(wall);
                wallLayers.Add(wall.gameObject.layer);
                Banana_WeaponsPlugin.Log.LogInfo($"Portal linked to wall: {wall.gameObject.name}");
            }
        }

        if (NewMovement.Instance != null)
        {
            playerCollider = NewMovement.Instance.GetComponent<Collider>();
            foreach (Collider col in GetComponents<Collider>())
                Physics.IgnoreCollision(playerCollider, col, true);
            foreach (Collider col in GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(playerCollider, col, true);
        }

        SetupVisuals();
    }

    void Update()
    {
        if (wallColliders.Count == 0) return;
        isInPortal = isGhosting;
        // --- Player ghosting ---
        if (playerCollider != null)
        {
            bool shouldSelfGhost = IsPlayerWithinPortalBounds(playerCollider);
            if (shouldSelfGhost != selfGhosting)
            {
                selfGhosting = shouldSelfGhost;
                HandleCollisionChange();

                if (partner != null)
                    partner.SetPartnerForced(selfGhosting);
            }
        }

        // --- Physics object ghosting ---
        UpdatePhysicsObjectGhosting();

        // --- Visuals ---
        if (DEBUG) UpdateVisuals();
    }

    // -------------------------------------------------------------------------
    // Visualization
    // -------------------------------------------------------------------------

    private void SetupVisuals()
    {
        if (lrRect != null) Destroy(lrRect.gameObject);
        if (lrDepthBox != null) Destroy(lrDepthBox.gameObject);

        if (!DEBUG) return;

        lrRect = CreateLineRenderer("PortalBounds_Rect", LineWidthRect, 5);
        lrDepthBox = CreateLineRenderer("PortalBounds_Depth", LineWidthDepth, 16);

        UpdateVisuals();
    }

    private LineRenderer CreateLineRenderer(string goName, float width, int pointCount)
    {
        GameObject go = new GameObject(goName);
        go.transform.SetParent(transform, false);
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = false;
        lr.positionCount = pointCount;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        return lr;
    }

    private void UpdateVisuals()
    {
        if (lrRect == null || lrDepthBox == null) return;

        // Build axes manually so width/height scale with the portal but depth doesn't.
        Vector3 origin = transform.position;
        Vector3 right = transform.right * transform.lossyScale.x;
        Vector3 up = transform.up * transform.lossyScale.y;
        Vector3 forward = transform.forward; // unit vector — we control depth ourselves

        // --- Face rectangle ---
        Vector3 TL = origin + right * (-VisHalfWidth) + up * VisHalfTop;
        Vector3 TR = origin + right * VisHalfWidth + up * VisHalfTop;
        Vector3 BR = origin + right * VisHalfWidth + up * -VisHalfBottom;
        Vector3 BL = origin + right * (-VisHalfWidth) + up * -VisHalfBottom;

        lrRect.SetPositions(new Vector3[] { TL, TR, BR, BL, TL });

        // --- Depth envelope ---
        // IsPlayerWithinPortalBounds checks localPos.z < ghostRadius.
        // InverseTransformPoint divides by lossyScale.z, so the actual world-space
        // depth is ghostRadius * lossyScale.z. Use exactly that so visual == actual.
        float d = ghostRadius * transform.lossyScale.z;

        Vector3 ftl = TL + forward * d;
        Vector3 ftr = TR + forward * d;
        Vector3 fbr = BR + forward * d;
        Vector3 fbl = BL + forward * d;
        Vector3 btl = TL + forward * -d;
        Vector3 btr = TR + forward * -d;
        Vector3 bbr = BR + forward * -d;
        Vector3 bbl = BL + forward * -d;

        lrDepthBox.SetPositions(new Vector3[]
        {
            ftl, ftr, fbr, fbl, ftl,   // front face  (5)
            btl, btr, bbr, bbl, btl,   // back  face  (5)
            ftl, btl,                  // edge TL     (2)
            ftr, btr,                  // edge TR     (2)
            fbr, bbr                   // edge BR     (2)
        });

        // --- Colour ---
        Color c = isGhosting ? ColGhost : (isOnFloor ? ColFloor : ColIdle);

        lrRect.startColor = c;
        lrRect.endColor = c;
        lrDepthBox.startColor = new Color(c.r, c.g, c.b, c.a * 0.4f);
        lrDepthBox.endColor = new Color(c.r, c.g, c.b, c.a * 0.4f);
    }

    // -------------------------------------------------------------------------

    private void UpdatePhysicsObjectGhosting()
    {
        Vector3 halfExtents = new Vector3(
            1.1f * transform.lossyScale.x,
            1.1f * transform.lossyScale.y,
            ghostRadius * PortalGun.sizeMult.Value
        );
        Collider[] nearby = Physics.OverlapBox(transform.position, halfExtents, transform.rotation);
        var newGhosted = new List<Collider>();

        foreach (Collider col in nearby)
        {
            if (col == playerCollider) continue;
            if (col.GetComponent<Rigidbody>() == null && col.GetComponentInParent<Rigidbody>() == null) continue;
            if (col.transform.IsChildOf(transform)) continue;
            if (NewMovement.Instance != null && col.transform.IsChildOf(NewMovement.Instance.transform)) continue;

            newGhosted.Add(col);

            if (!ghostedPhysicsColliders.Contains(col))
            {
                foreach (Collider wall in wallColliders)
                {
                    if (wall != null)
                        Physics.IgnoreCollision(col, wall, true);
                }
                Banana_WeaponsPlugin.Log.LogInfo($"Physics object {col.gameObject.name} is now GHOST through portal wall.");
            }
        }

        foreach (Collider col in ghostedPhysicsColliders)
        {
            if (col == null) continue;
            if (!newGhosted.Contains(col))
            {
                foreach (Collider wall in wallColliders)
                {
                    if (wall != null)
                        Physics.IgnoreCollision(col, wall, false);
                }
                Banana_WeaponsPlugin.Log.LogInfo($"Physics object {col.gameObject.name} is now SOLID against portal wall.");
            }
        }

        ghostedPhysicsColliders = newGhosted;
    }

    public void SetPartnerForced(bool forced)
    {
        if (partnerForced == forced) return;
        partnerForced = forced;

        HandleCollisionChange();
    }

    private void HandleCollisionChange()
    {
        if (isGhosting)
        {
            if (reenableRoutine != null) StopCoroutine(reenableRoutine);
            ApplyCollision(true);
        }
        else
        {
            if (reenableRoutine != null) StopCoroutine(reenableRoutine);
            reenableRoutine = StartCoroutine(WaitToReenable());
        }
    }

    private IEnumerator WaitToReenable()
    {
        yield return new WaitForSeconds(reenableDelay);
        ApplyCollision(false);
        reenableRoutine = null;
    }

    void ApplyCollision(bool ignore)
    {
        if (wallColliders.Count == 0 || playerCollider == null) return;

        string status = ignore ? "GHOST" : "SOLID";

        for (int i = 0; i < wallColliders.Count; i++)
        {
            if (wallColliders[i] == null) continue;
            Physics.IgnoreCollision(playerCollider, wallColliders[i], ignore);

            //NewMovement.Instance.GetComponent<Collider>().isTrigger = !ignore;

            if (isOnFloor)
            {
                targetWalls[i].layer = ignore ? 0 : wallLayers[i];
                /*
                if (NewMovement.Instance.gc)
                {
                    NewMovement.Instance.gc.enabled = !ignore;

                    if (ignore)
                    {
                        foreach (var ggc in NewMovement.Instance.gc.instances)
                        {
                            ggc.onGround = false;
                            ggc.touchingGround = false;
                        }
                    }
                }*/

                if (NewMovement.Instance.wc) NewMovement.Instance.wc.enabled = !ignore;
                var vcb = NewMovement.Instance.GetComponent<VerticalClippingBlocker>();
                if (vcb) vcb.enabled = !ignore;
            }

            Banana_WeaponsPlugin.Log.LogInfo($"Wall {targetWalls[i].name} is now {status} for Player.");
        }
    }

    private bool IsPlayerWithinPortalBounds(Collider col)
    {
        Vector3 checkPoint = isOnFloor
            ? new Vector3(col.bounds.center.x, col.bounds.min.y, col.bounds.center.z)
            : col.bounds.center;

        Vector3 localPos = transform.InverseTransformPoint(checkPoint);

        const float expand = 0.2f;
        return Mathf.Abs(localPos.x) < 1.2f + expand
            && Mathf.Abs(localPos.y) < 1.2f + expand
            && Mathf.Abs(localPos.z) < ghostRadius;
    }

    void OnDestroy()
    {
        if (playerCollider != null)
        {
            for (int i = 0; i < wallColliders.Count; i++)
            {
                if (wallColliders[i] != null)
                    Physics.IgnoreCollision(playerCollider, wallColliders[i], false);
            }

            if (isOnFloor && NewMovement.Instance != null)
            {
                if (NewMovement.Instance.gc) NewMovement.Instance.gc.enabled = true;
                if (NewMovement.Instance.wc) NewMovement.Instance.wc.enabled = true;
                var vcb = NewMovement.Instance.GetComponent<VerticalClippingBlocker>();
                if (vcb) vcb.enabled = true;

                for (int i = 0; i < targetWalls.Count; i++)
                {
                    if (targetWalls[i] != null)
                        targetWalls[i].layer = wallLayers[i];
                }
            }
        }

        foreach (Collider col in ghostedPhysicsColliders)
        {
            if (col == null) continue;
            foreach (Collider wall in wallColliders)
            {
                if (wall != null)
                    Physics.IgnoreCollision(col, wall, false);
            }
        }
        ghostedPhysicsColliders.Clear();
    }

    [HarmonyPatch(typeof(GroundCheckGroup), nameof(GroundCheckGroup.onGround), MethodType.Getter)]
    public class DontBeGroundedInPortales
    {
        public static bool Prefix(ref bool __result)
        {
            if (isInPortal)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}