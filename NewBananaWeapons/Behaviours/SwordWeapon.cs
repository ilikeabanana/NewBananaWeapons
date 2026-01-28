using System.Collections;
using System.Collections.Generic;
using ULTRAKILL.Cheats;
using UnityEngine;


public class SwordWeapon : MonoBehaviour
{
    float timeBetweenSlashes = 3;
    float cd;
    Animator anim;

    bool activeFrames = false;
    List<EnemyIdentifier> alreadyHitEnemies = new List<EnemyIdentifier>();

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
            Transform cam = CameraController.Instance.transform;
            Vector3 origin = cam.position;
            Vector3 dir = cam.forward;

            bool hitAnyEnemy = false;

            RaycastHit[] hits = Physics.BoxCastAll(
                origin,
                Vector3.one * 30f,   // <-- halfExtents
                dir,
                cam.rotation,
                4f,
                LayerMaskDefaults.Get(LMD.Enemies),
                QueryTriggerInteraction.Collide
            );


            if (hits.Length == 0)
            {
                Collider[] closeHits = Physics.OverlapSphere(
                    origin + dir * 1.5f,
                    40f,
                    LayerMaskDefaults.Get(LMD.Enemies),
                    QueryTriggerInteraction.Collide
                );

                foreach (Collider c in closeHits)
                {
                    if (c.TryGetComponent(out EnemyIdentifierIdentifier eidd))
                    {
                        Vector3 toEnemy = (eidd.transform.position - origin).normalized;
                        if (Vector3.Dot(dir, toEnemy) <= 0f) continue;
                        DealSwordDamage(eidd, c.transform.position, dir);
                        hitAnyEnemy = true;
                    }
                }
            }
            else
            {
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.TryGetComponent(out EnemyIdentifierIdentifier eidd))
                    {
                        Vector3 toEnemy = (eidd.transform.position - origin).normalized;
                        if (Vector3.Dot(dir, toEnemy) <= 0f) continue;
                        DealSwordDamage(eidd, hit.point, dir);
                        hitAnyEnemy = true;
                    }
                }
            }

            if (hitAnyEnemy)
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
        alreadyHitEnemies.Clear();
        activeFrames = true;
    }
    void DealSwordDamage(EnemyIdentifierIdentifier eidd, Vector3 hitPoint, Vector3 direction)
    {
        EnemyIdentifier eid = eidd.eid;

        if (alreadyHitEnemies.Contains(eid)) return;
        eid.DeliverDamage(
            eidd.gameObject,
            direction * 100f,
            hitPoint,
            25,
            false
        );
        alreadyHitEnemies.Add(eid);
    }

    public void DeactivateFrames()
    {
        activeFrames = false;
        alreadyHitEnemies.Clear();
    }
}
