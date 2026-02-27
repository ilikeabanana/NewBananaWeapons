using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace NewBananaWeapons.Behaviours.Non_WeaponBehaviours
{
    public class PortalCollisionFixer : MonoBehaviour
    {
        private List<GameObject> targetWalls = new List<GameObject>();
        private List<Collider> wallColliders = new List<Collider>();
        private List<int> wallLayers = new List<int>();
        private Collider playerCollider;
        public float ghostRadius = 3f;
        public float reenableDelay = 0.05f;
        public PortalCollisionFixer partner;
        private bool selfGhosting = false;
        private bool partnerForced = false;
        private Coroutine reenableRoutine;
        public bool isGhosting => selfGhosting || partnerForced;


        public bool isOnFloor
        {
            get
            {
                if (targetWalls == null) return false;
                if (targetWalls.Count == 0) return false;
                foreach (var wall in targetWalls)
                {
                    if (wall.tag == "Floor") return true;
                }
                return false;
            }
        }

        public void FixCols()
        {
            Vector3 inflatedSize = (transform.localScale / 2) + Vector3.one * 0.3f; // inflate uniformly

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
            if (wallColliders.Count == 0 || playerCollider == null) return;

            float distance = Vector3.Distance(
                transform.position,
                playerCollider.bounds.ClosestPoint(transform.position));

            bool shouldSelfGhost = distance < ghostRadius;
            if (shouldSelfGhost == selfGhosting) return;

            selfGhosting = shouldSelfGhost;
            HandleCollisionChange();

            if (partner != null)
                partner.SetPartnerForced(selfGhosting);
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
                targetWalls[i].layer = ignore ? 0 : wallLayers[i];
                if (NewMovement.Instance.gc) NewMovement.Instance.gc.enabled = !ignore;
                if (NewMovement.Instance.wc) NewMovement.Instance.wc.enabled = !ignore;
                var vcb = NewMovement.Instance.GetComponent<VerticalClippingBlocker>();
                if (vcb) vcb.enabled = !ignore;

                Banana_WeaponsPlugin.Log.LogInfo($"Wall {targetWalls[i].name} is now {status} for Player.");
            }
        }

        void OnDestroy()
        {
            if (playerCollider == null) return;
            for (int i = 0; i < wallColliders.Count; i++)
            {
                if (wallColliders[i] != null)
                    Physics.IgnoreCollision(playerCollider, wallColliders[i], false);
            }
        }
    }
}