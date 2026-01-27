using System.Collections;
using ULTRAKILL.Cheats;
using UnityEngine;


public class SwordWeapon : MonoBehaviour
{
    float timeBetweenSlashes = 3;
    float cd;
    Animator anim;

    bool activeFrames = false;

    // Use this for initialization
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!GunControl.Instance.activated) return;
        if (NoWeaponCooldown.NoCooldown)
        {
            cd = 0;
        }
        cd -= Time.deltaTime;
        if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed && cd <= 0)
        {
            anim.SetBool("DoingTheThing", true);
            cd = timeBetweenSlashes;
        }
        else
        {
            anim.SetBool("DoingTheThing", false);
        }
        if (activeFrames)
        {
            // size = 31
            Transform cam = CameraController.Instance.transform;
            Vector3 origin = cam.position;
            Vector3 dir = cam.forward;

            bool hitEnemy = false;
            RaycastHit hit;

            if (!Physics.Raycast(origin, dir, out hit, 40, LayerMaskDefaults.Get(LMD.Enemies), QueryTriggerInteraction.Collide))
            {
                Physics.BoxCast(origin, Vector3.one * 30, dir, out hit, cam.rotation, 4f,
                    LayerMaskDefaults.Get(LMD.Enemies), QueryTriggerInteraction.Collide);
            }

            if (hit.collider != null)
            {
                if (hit.collider.TryGetComponent(out EnemyIdentifierIdentifier eidd))
                {
                    DealSwordDamage(eidd, hit.point, dir);
                    hitEnemy = true;
                }
            }

            if (!hitEnemy)
            {
                Collider[] closeHits = Physics.OverlapSphere(origin + dir * 1.5f, 1f,
                    LayerMaskDefaults.Get(LMD.Enemies), QueryTriggerInteraction.Collide);

                foreach (Collider c in closeHits)
                {
                    if (c.TryGetComponent(out EnemyIdentifierIdentifier eidd))
                    {
                        DealSwordDamage(eidd, c.transform.position, dir);
                        hitEnemy = true;
                        break;
                    }
                }
            }

            if (hitEnemy)
            {
                MonoSingleton<TimeController>.Instance.HitStop(0.05f);
                CameraController.Instance.CameraShake(0.2f);
            }
        }
    }
    void OnDisable()
    {
        DeactivateFrames();
    }

    public void ActivateFrames()
    {
        activeFrames = true;
    }
    void DealSwordDamage(EnemyIdentifierIdentifier eidd, Vector3 hitPoint, Vector3 direction)
    {
        EnemyIdentifier eid = eidd.eid;
        eid.DeliverDamage(
            eidd.gameObject,
            direction * 100f,
            hitPoint,
            25,
            false
        );
    }

    public void DeactivateFrames()
    {
        activeFrames = false;
    }
}
