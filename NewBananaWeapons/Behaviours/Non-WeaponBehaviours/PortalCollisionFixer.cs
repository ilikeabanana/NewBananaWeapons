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
            float distance = Vector3.Distance(
                transform.position,
                playerCollider.bounds.ClosestPoint(transform.position));

            bool shouldSelfGhost = distance < ghostRadius;
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
        // Find all rigidbodies within ghostRadius
        Collider[] nearby = Physics.OverlapSphere(transform.position, ghostRadius);
        var newGhosted = new List<Collider>();

        foreach (Collider col in nearby)
        {
            // Skip the player, portal's own colliders, and non-rigidbody objects
            if (col == playerCollider) continue;
            if (col.GetComponent<Rigidbody>() == null && col.GetComponentInParent<Rigidbody>() == null) continue;
            if (col.transform.IsChildOf(transform)) continue;

            newGhosted.Add(col);

            // If not already ghosted, apply ignore
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

        // Re-enable collision for objects that left the radius
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
                    if (ignore)
                    {
                        foreach (var ggc in NewMovement.Instance.gc.instances)
                        {
                            ggc.onGround = false;
                            ggc.touchingGround = false;

                        }
                    }


                    NewMovement.Instance.gc.enabled = !ignore;
                }
                if (NewMovement.Instance.wc) NewMovement.Instance.wc.enabled = !ignore;
                var vcb = NewMovement.Instance.GetComponent<VerticalClippingBlocker>();
                if (vcb) vcb.enabled = !ignore;
            }

            Banana_WeaponsPlugin.Log.LogInfo($"Wall {targetWalls[i].name} is now {status} for Player.");
        }
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
