using NewBananaWeapons;
using System.Collections.Generic;
using UnityEngine;

public class TechniqueProjectile : MonoBehaviour
{
    float attractionStrength = 20;
    float attractionRadius = 15;

    [HideInInspector] public bool reversal;

    float damageOnSuction = 15f;
    float damageInterval = 0.25f; // ← delay between damage ticks per enemy

    // Tracks last time each enemy was damaged
    private readonly Dictionary<EnemyIdentifier, float> lastDamageTime
        = new Dictionary<EnemyIdentifier, float>();

    void Update()
    {
        transform.position += transform.forward * 7f * Time.fixedDeltaTime;
    }

    void FixedUpdate()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attractionRadius);

        foreach (var hit in hits)
        {
            if (hit.GetComponent<NewMovement>())
                continue;

            Vector3 dir = reversal
                ? (hit.transform.position - transform.position).normalized
                : (transform.position - hit.transform.position).normalized;

            if (hit.attachedRigidbody)
                hit.attachedRigidbody.AddForce(dir * attractionStrength, ForceMode.Acceleration);

            if (hit.TryGetComponent<EnemyIdentifierIdentifier>(out var eidd))
            {
                var enemy = eidd.eid;

                Banana_WeaponsPlugin.ApplyKnockBack(enemy, dir * attractionStrength);

                // Check damage cooldown
                float lastTime;
                lastDamageTime.TryGetValue(enemy, out lastTime);

                if (Time.time - lastTime >= damageInterval)
                {
                    enemy.DeliverDamage(
                        eidd.gameObject,
                        Vector3.zero,
                        eidd.transform.position,
                        damageOnSuction,
                        true
                    );

                    lastDamageTime[enemy] = Time.time;
                }
            }
        }
    }
}
