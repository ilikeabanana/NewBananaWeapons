using BepInEx.Configuration;
using NewBananaWeapons.Behaviours.Non_WeaponBehaviours;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULTRAKILL.Portal;
using ULTRAKILL.Portal.Geometry;
using UnityEngine;

namespace NewBananaWeapons.Behaviours
{
    public class PortalGun : BaseWeapon
    {
        public override void SetupConfigs(string sectionName, ConfigFile Config)
        {
            base.SetupConfigs(sectionName, Config);
        }


        void Update()
        {
            PortalAttempt();
        }
        GameObject quad2;
        GameObject quad1;
        bool alrSetupPortals;
        void PortalAttempt()
        {
            if (CameraController.Instance == null) return;
            if (!GunControl.Instance.activated) return;
            // Use a local variable to capture the raycast once to avoid 
            // multiple raycasts in one frame (performance/consistency)
            RaycastHit hit = aimHit;

            if (InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame)
            {
                UpdatePortal(ref quad1, "Portal_Entry", hit);
                if (quad2 != null && !alrSetupPortals) SetupPortals();
            }
            else if (InputManager.Instance.InputSource.Fire2.WasPerformedThisFrame)
            {
                UpdatePortal(ref quad2, "Portal_Exit", hit);
                if (quad1 != null && !alrSetupPortals) SetupPortals();
            }
        }

        void UpdatePortal(ref GameObject portalObj, string name, RaycastHit hit)
        {
            // 1. Check if the surface is valid (Flat and large enough)
            Vector3 portalSize = new Vector3(1.25f, 2.5f, 0.1f); // Width, Height, Depth
            if (!IsSurfaceValid(hit, portalSize))
            {
                Debug.Log("Surface invalid: Too small, curved, or near an edge.");
                return;
            }

            // 2. Check if another portal is already here
            if (IsSpaceOccupied(hit.point, portalSize, portalObj))
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

            // Face AWAY from the wall
            portalObj.transform.rotation = Quaternion.LookRotation(-hit.normal);
            // Offset slightly to prevent Z-fighting/clipping
            portalObj.transform.position = hit.point + hit.normal * 0.05f;

            portalObj.GetOrAddComponent<PortalCollisionFixer>().FixCols();
        }

        private bool IsSurfaceValid(RaycastHit centerHit, Vector3 size)
        {
            // We check 4 points around the center hit to ensure they land on the same plane
            Vector3 upDir = Vector3.up;
            // Ensure upDir isn't parallel to the normal
            if (Mathf.Abs(Vector3.Dot(centerHit.normal, Vector3.up)) > 0.9f)
                upDir = Vector3.forward;

            Vector3 right = Vector3.Cross(centerHit.normal, upDir).normalized;
            Vector3 up = Vector3.Cross(right, centerHit.normal).normalized;

            // The 4 corners of the portal to check
            Vector3[] checkPoints = new Vector3[]
            {
        centerHit.point + (right * size.x * 0.5f) + (up * size.y * 0.5f),
        centerHit.point - (right * size.x * 0.5f) + (up * size.y * 0.5f),
        centerHit.point + (right * size.x * 0.5f) - (up * size.y * 0.5f),
        centerHit.point - (right * size.x * 0.5f) - (up * size.y * 0.5f)
            };

            foreach (Vector3 pos in checkPoints)
            {
                // Fire a ray from slightly "outside" the wall back towards the wall
                Ray ray = new Ray(pos + centerHit.normal * 0.5f, -centerHit.normal);
                if (Physics.Raycast(ray, out RaycastHit sideHit, 1f, LayerMaskDefaults.Get(LMD.Environment)))
                {
                    // If the normal is different, the surface is curved or hit a different object
                    if (Vector3.Angle(centerHit.normal, sideHit.normal) > 2.0f) return false;

                    // If the distance is too different, it's floating over a gap
                    if (Mathf.Abs(sideHit.distance - 0.5f) > 0.1f) return false;
                }
                else
                {
                    // Ray missed entirely (the edge of the wall)
                    return false;
                }
            }
            return true;
        }

        private bool IsSpaceOccupied(Vector3 position, Vector3 size, GameObject currentPortal)
        {
            // Look for any existing Portals in the area
            Collider[] colliders = Physics.OverlapBox(position, size * 0.5f);
            foreach (var col in colliders)
            {
                // Ignore if the collider belongs to the portal we are currently moving
                if (currentPortal != null && (col.gameObject == currentPortal || col.transform.IsChildOf(currentPortal.transform)))
                    continue;

                // If we hit something with a Portal component or PortalIdentifier, it's occupied
                if (col.GetComponentInParent<Portal>() != null || col.GetComponentInParent<PortalIdentifier>() != null)
                    return true;
            }
            return false;
        }
        RaycastHit aimHit
        {
            get
            {
                Transform camTrans = CameraController.Instance.transform;
                // Declare it once here
                RaycastHit hit;

                if (Physics.Raycast(camTrans.position, camTrans.forward, out hit, 100, LayerMaskDefaults.Get(LMD.Environment)))
                {
                    return hit;
                }
                else
                {
                    // Just assign to the existing variable, don't re-declare it
                    hit = new RaycastHit();
                    hit.point = camTrans.position + (camTrans.forward * 100); // Fixed: point should be relative to cam position
                    hit.normal = -camTrans.forward;
                    return hit;
                }
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
            portal1.canSeePortalLayer = false;

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
                if (fixer2 == null) return;
                if (fixer2.isOnFloor)
                {
                    Vector3 offset = x.travellerVelocity.normalized * 3;
                    NewMovement.Instance.transform.position += offset;
                }
            });
            portal1.onExitTravel.AddListener((x, y) =>
            {
                if (fixer1 == null) return;
                if (fixer1.isOnFloor)
                {
                    Vector3 offset = x.travellerVelocity.normalized * 3;
                    NewMovement.Instance.transform.position += offset;
                }
            });
        }
    }
}
