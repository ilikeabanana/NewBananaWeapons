using NewBananaWeapons;
using System.Collections.Generic;
using ULTRAKILL.Portal;
using UnityEngine;

public class TechniqueProjectile : MonoBehaviour
{
    public enum TechniqueType
    {
        Blue,
        Red,
        Purple
    }
    void Awake()
    {
        gameObject.AddComponent<SimplePortalTraveler>();
    }

    public TechniqueType technique;

    float attractionStrength = 65;
    float attractionRadius = 15;

    float damageOnSuction = 15f;
    float damageInterval = 0.25f;
    float speed = 7f;

    // Collapse settings
    float lifetime = 20f;
    float collapseRadius = 35f;
    float collapseForce = 250f;
    float collapseDamage = 50f;

    float spawnTime;
    bool collapsed;

    private readonly Dictionary<EnemyIdentifier, float> lastDamageTime
        = new Dictionary<EnemyIdentifier, float>();

    void Start()
    {
        spawnTime = Time.time;

        // Purple = stronger by default
        if (technique == TechniqueType.Purple)
        {
            speed *= 25;
            attractionStrength *= 1.5f;
            damageOnSuction *= 7f;
            collapseForce *= 2f;
            collapseDamage *= 2f;
        }
    }

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;

        if (!collapsed && Time.time - spawnTime >= lifetime)
            Collapse();
    }

    void FixedUpdate()
    {
        if (collapsed) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, attractionRadius);

        foreach (var hit in hits)
        {
            if (hit.GetComponent<NewMovement>()) continue;
            if (hit.attachedRigidbody && hit.attachedRigidbody.GetComponent<NewMovement>()) continue;

            Vector3 toTarget = (hit.transform.position - transform.position).normalized;
            Vector3 toCenter = -toTarget;

            // Blue = pull, Red = push, Purple = both
            Vector3 forceDir = Vector3.zero;

            switch (technique)
            {
                case TechniqueType.Blue:
                    forceDir = toCenter;
                    break;

                case TechniqueType.Red:
                    forceDir = toTarget;
                    break;

                case TechniqueType.Purple:
                    forceDir = toCenter + toTarget; // tearing force
                    break;
            }


            if (hit.attachedRigidbody)
                hit.attachedRigidbody.AddForce(forceDir * attractionStrength, ForceMode.Impulse);

            if (hit.TryGetComponent<EnemyIdentifierIdentifier>(out var eidd))
            {
                var enemy = eidd.eid;

                if (technique == TechniqueType.Purple)
                {
                    if (enemy.dead) continue;
                    StyleHUD.Instance.AddPoints(125, "PURPLED");
                    enemy.InstaKill();
                    Destroy(enemy.gameObject);
                    Instantiate(AddressableManager.blackholekaboom, transform.position, transform.rotation);
                    continue;
                }


                Banana_WeaponsPlugin.ApplyKnockBack(enemy, forceDir * attractionStrength);

                
                lastDamageTime.TryGetValue(enemy, out float lastTime);

                if (Time.time - lastTime >= damageInterval && !enemy.dead)
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

    void Collapse()
    {
        collapsed = true;

        Collider[] hits = Physics.OverlapSphere(transform.position, collapseRadius);

        foreach (var hit in hits)
        {
            if (hit.GetComponent<NewMovement>()) continue;
            if (hit.attachedRigidbody && hit.attachedRigidbody.GetComponent<NewMovement>()) continue;

            Vector3 dir = (hit.transform.position - transform.position).normalized;

            if (hit.attachedRigidbody)
                hit.attachedRigidbody.AddForce(dir * collapseForce, ForceMode.Impulse);

            if (hit.TryGetComponent<EnemyIdentifierIdentifier>(out var eidd))
            {
                var enemy = eidd.eid;

                if (!enemy.dead)
                {
                    Banana_WeaponsPlugin.ApplyKnockBack(enemy, dir * collapseForce);

                    enemy.DeliverDamage(
                        eidd.gameObject,
                        Vector3.zero,
                        eidd.transform.position,
                        collapseDamage,
                        true
                    );
                }
            }
        }

        Destroy(gameObject);
    }
}
