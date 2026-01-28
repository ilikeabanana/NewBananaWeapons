using System.Collections;
using UnityEngine;


public class SuperBallProjectile : MonoBehaviour
{
    [HideInInspector] public float damage = 50;
    void OnCollisionEnter(Collision hit)
    {
        if (LayerMaskDefaults.IsMatchingLayer(hit.gameObject.layer, LMD.EnemiesAndEnvironment))
        {
            if (hit.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier eidd))
            {
                eidd.eid.DeliverDamage(hit.gameObject, transform.forward * 10,
                    eidd.eid.transform.position, damage, false);
            }
            if (hit.gameObject.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier eid))
            {
                eid.DeliverDamage(hit.gameObject, transform.forward * 10,
                    eid.transform.position, damage, false);
            }

            damage /= 2;
        }

    }
}
