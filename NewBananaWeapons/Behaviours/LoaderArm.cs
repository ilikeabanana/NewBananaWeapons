using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LoaderArm : MonoBehaviour
{
    float charge;          // how strong the punch is
    float launchTimer;     // how long the launch lasts

    bool launching;
    float velocity = 0;
    List<EnemyIdentifier> alreadyHitEnemies = new List<EnemyIdentifier>();
    Vector3 direction;
    LayerMask prevLayerMask;
    // Update is called once per frame
    void Update()
    {
        if (InputManager.Instance.InputSource.Punch.IsPressed && !launching)
            charge += Time.deltaTime;


        if (InputManager.Instance.InputSource.Punch.WasCanceledThisFrame && !launching)
        {
            //NewMovement.Instance.Launch(CameraController.Instance.transform.forward, 32 * chargeTime, true);
            direction = CameraController.Instance.transform.forward;
            charge = Mathf.Clamp(charge, 0f, 5f); // optional cap
            velocity = 48f * charge;
            launchTimer = charge / 2;  // duration scales with charge
            launching = true;
            charge = 0;
            alreadyHitEnemies.Clear();
            
        }

        if (launching)
        {
            NewMovement.Instance.rb.velocity = direction * velocity;

            launchTimer -= Time.deltaTime;
            if (launchTimer <= 0f)
            {
                launching = false;
            }

            RaycastHit[] hits = Physics.SphereCastAll(
                CameraController.Instance.transform.position,
                5f,
                direction,
                1
            );

            foreach (var hit in hits)
            {
                if (hit.collider.TryGetComponent<EnemyIdentifierIdentifier>(out var enemyHit))
                {
                    if (alreadyHitEnemies.Contains(enemyHit.eid))
                        continue;

                    enemyHit.eid.hitter = "riskofrain2loaderreference";
                    enemyHit.eid.DeliverDamage(
                        hit.collider.gameObject,
                        direction * 1000f * charge,
                        hit.point,
                        15 * (charge + 1),
                        false,
                        sourceWeapon: null
                    );

                    alreadyHitEnemies.Add(enemyHit.eid);
                }
            }

            if (launchTimer <= 0) 
            {
                launching = false;
            }
        }
    }
}
