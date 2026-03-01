using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NewBananaWeapons;

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

    public bool isGhosting => selfGhosting || partnerForced;

    public bool isOnFloor
    {
        get
        {
            if (placedOnWall == null) return false;
            if (placedOnWall.tag == "Floor") return true;
            return false;
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
    }

    void Update()
    {
        if (wallColliders.Count == 0) return;

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
    }

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

            if (isOnFloor)
            {
                targetWalls[i].layer = ignore ? 0 : wallLayers[i];

                if (NewMovement.Instance.gc)
                {
                    NewMovement.Instance.gc.enabled = !ignore;

                    // FIX: only wipe ground state when STARTING to ghost (ignore=true).
                    // Wiping on re-enable (ignore=false) causes a 1-frame gap where
                    // gc is enabled but onGround=false -> walking-on-air bug.
                    if (ignore)
                    {
                        foreach (var ggc in NewMovement.Instance.gc.instances)
                        {
                            ggc.onGround = false;
                            ggc.touchingGround = false;
                        }
                    }
                }

                if (NewMovement.Instance.wc) NewMovement.Instance.wc.enabled = !ignore;
                var vcb = NewMovement.Instance.GetComponent<VerticalClippingBlocker>();
                if (vcb) vcb.enabled = !ignore;
            }

            Banana_WeaponsPlugin.Log.LogInfo($"Wall {targetWalls[i].name} is now {status} for Player.");
        }
    }

    private bool IsPlayerWithinPortalBounds(Collider col)
    {
        // FIX: for floor portals, check the bottom of the player's bounds instead of
        // the center. The center exits the portal rectangle before the feet clear the
        // hole, causing the floor layer to restore while the player is still inside -> fall through.
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
        // Restore player collision
        if (playerCollider != null)
        {
            for (int i = 0; i < wallColliders.Count; i++)
            {
                if (wallColliders[i] != null)
                    Physics.IgnoreCollision(playerCollider, wallColliders[i], false);
            }

            // Restore gc/wc/vcb in case portal is destroyed while still ghosting
            if (isOnFloor && NewMovement.Instance != null)
            {
                if (NewMovement.Instance.gc) NewMovement.Instance.gc.enabled = true;
                if (NewMovement.Instance.wc) NewMovement.Instance.wc.enabled = true;
                var vcb = NewMovement.Instance.GetComponent<VerticalClippingBlocker>();
                if (vcb) vcb.enabled = true;

                // Restore layers
                for (int i = 0; i < targetWalls.Count; i++)
                {
                    if (targetWalls[i] != null)
                        targetWalls[i].layer = wallLayers[i];
                }
            }
        }

        // Restore physics object collisions
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
}