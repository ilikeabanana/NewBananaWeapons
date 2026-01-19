using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LoaderArm : MonoBehaviour
{
    float chargeTime;
    bool launching;
    float velocity = 0;
    List<EnemyIdentifier> alreadyHitEnemies = new List<EnemyIdentifier>();
    Vector3 direction;
    LayerMask prevLayerMask;
    // Update is called once per frame
    void Update()
    {
        if (InputManager.Instance.InputSource.Punch.IsPressed && !launching)
        {
            chargeTime += Time.deltaTime * 2;
        }

        if (InputManager.Instance.InputSource.Punch.WasCanceledThisFrame && !launching)
        {
            //NewMovement.Instance.Launch(CameraController.Instance.transform.forward, 32 * chargeTime, true);
            velocity = 32 * chargeTime;
            direction = CameraController.Instance.transform.forward;
            chargeTime = 0;
            launching = true;
            alreadyHitEnemies.Clear();
            
        }

        if (launching)
        {
            NewMovement.Instance.rb.velocity = direction * velocity;

            chargeTime -= Time.deltaTime;
            Collider[] hitCols = Physics.OverlapSphere(transform.position, 10);
            if (hitCols.Length > 0)
            {
                foreach (var col in hitCols)
                {
                    if (col.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier enemyHit))
                    {
                        if (alreadyHitEnemies.Contains(enemyHit.eid)) continue;
                        enemyHit.eid.hitter = "riskofrain2loaderreference";
                        enemyHit.eid.DeliverDamage(col.gameObject, direction * 1000 * chargeTime, enemyHit.transform.position, 15 * chargeTime, false, sourceWeapon: null);
                        alreadyHitEnemies.Add(enemyHit.eid);
                    }

                }
            }
            if (chargeTime <= 0) 
            {
                launching = false;
            }
        }
    }
}
