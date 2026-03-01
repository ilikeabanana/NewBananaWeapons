using NewBananaWeapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULTRAKILL.Portal;
using UnityEngine;


public class PortalBeam : MonoBehaviour
{
    LineRenderer lr;
    Vector3 shotHitPoint;
    Vector3? lastForward;
    Vector3 alternateStartPoint;

    public Color shootColor;

    float t = 1;

    void Awake()
    {
        Destroy(gameObject, 1);
        lr = GetComponent<LineRenderer>();
        Shoot();
    }

    void Update()
    {
        t -= Time.deltaTime;
        lr.widthMultiplier = t;
    }

    private void UpdateForward(PortalTraversalV2[] portalTraversals, PhysicsCastResult latestHit)
    {
        this.UpdateForward(portalTraversals, latestHit.point);
    }

    // Token: 0x060019EB RID: 6635 RVA: 0x000EB768 File Offset: 0x000E9968
    private void UpdateForward(PortalTraversalV2[] portalTraversals, Vector3 hitPos)
    {
        if (portalTraversals.Length == 0)
        {
            return;
        }
        PortalTraversalV2 portalTraversalV = portalTraversals[portalTraversals.Length - 1];
        this.lastForward = new Vector3?(hitPos - portalTraversalV.exitPoint);
        this.lastForward = new Vector3?(this.lastForward.Value.normalized);
    }

    private void Shoot()
    {

        Vector3 position = base.transform.position;
        Vector3 forward = base.transform.forward;
        float num2 = PortalGun.range.Value;
        LayerMask mask = LayerMaskDefaults.Get(LMD.Environment);
        PhysicsCastResult physicsCastResult;
        PortalTraversalV2[] array;
        Vector3 vector;
        bool flag2 = PortalPhysicsV2.Raycast(position, forward, num2, mask, out physicsCastResult, out array, out vector, QueryTriggerInteraction.UseGlobal);
        bool flag3 = false;
        Vector3 a = position;
        Vector3 vector2 = forward;
        float num3 = 0f;
        for (int i = 0; i < array.Length; i++)
        {
            PortalTraversalV2 portalTraversalV = array[i];
            Vector3 entrancePoint = portalTraversalV.entrancePoint;
            num3 += Vector3.Distance(a, entrancePoint);
            PortalHandle portalHandle = portalTraversalV.portalHandle;
            if (!portalTraversalV.portalObject.GetTravelFlags(portalHandle.side).HasAllFlags(PortalTravellerFlags.PlayerProjectile))
            {
                this.shotHitPoint = entrancePoint;
                Array.Resize<PortalTraversalV2>(ref array, i);
                flag3 = true;
                break;
            }
            a = portalTraversalV.exitPoint;
            vector2 = portalTraversalV.exitDirection;
        }
        if (flag3)
        {
            flag2 = false;
            num2 = num3 - 0.01f;
            this.lastForward = new Vector3?(vector2.normalized);
        }
        else if (flag2)
        {
            num2 = physicsCastResult.distance;
            this.shotHitPoint = physicsCastResult.point;
            this.UpdateForward(array, physicsCastResult);
        }
        else
        {
            Vector3 a2 = position;
            Vector3 a3 = forward;
            if (array.Length != 0)
            {
                PortalTraversalV2[] array2 = array;
                PortalTraversalV2 portalTraversalV2 = array2[array2.Length - 1];
                a2 = portalTraversalV2.exitPoint;
                a3 = portalTraversalV2.exitDirection;
            }
            this.shotHitPoint = a2 + a3 * num2;
        }
        Vector3 vector3 = position;
        bool flag5 = false;
        Vector3 vector4 = PortalUtils.GetTravelMatrix(array).inverse.MultiplyPoint3x4(this.shotHitPoint);
        Vector3 vector5 = vector4 - vector3;
        if (this.alternateStartPoint != Vector3.zero)
        {
            PhysicsCastResult physicsCastResult3;
            Vector3 vector6;
            PortalTraversalV2[] array4;
            PortalPhysicsV2.ProjectThroughPortals(base.transform.position, this.alternateStartPoint - base.transform.position, default(LayerMask), out physicsCastResult3, out vector6, out array4);
            if (array4.Length != 0)
            {
                PortalTraversalV2 portalTraversalV3 = array4[0];
                PortalHandle portalHandle2 = portalTraversalV3.portalHandle;
                if (!portalTraversalV3.portalObject.GetTravelFlags(portalHandle2.side).HasAllFlags(PortalTravellerFlags.PlayerProjectile))
                {
                    flag5 = true;
                }
                else if (array4.AllHasFlag(PortalTravellerFlags.PlayerProjectile))
                {
                    vector3 = vector6;
                    vector5 = PortalUtils.GetTravelMatrix(array4).MultiplyPoint3x4(vector4) - vector3;
                }
                else
                {
                    vector3 = this.alternateStartPoint;
                    vector5 = vector4 - vector3;
                }
            }
            else
            {
                vector3 = this.alternateStartPoint;
                vector5 = vector4 - vector3;
            }
        }
        else
        {
            vector5 = vector4 - vector3;
        }
        if (!flag5)
        {
            PhysicsCastResult physicsCastResult3;
            PortalTraversalV2[] array5;
            PortalPhysicsV2.Raycast(vector3, vector5.normalized, vector5.magnitude - 0.01f, default(LayerMask), out physicsCastResult3, out array5, out vector, QueryTriggerInteraction.UseGlobal);
            this.lr.SetPosition(0, vector3);
            this.lr.SetPosition(1, this.shotHitPoint);
            if (array5 != null && array5.Length > 0)
            {
                this.GenerateLineRendererSegments(this.lr, array5);
            }
            //Transform child = base.transform.GetChild(0);
            //child.gameObject.SetActive(false);
        }
    }

}
