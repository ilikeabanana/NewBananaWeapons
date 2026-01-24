using NewBananaWeapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MaxwellProjectile : MonoBehaviour
{
    public Vector3 orgPos;
    public int pets;

    float timer = 0;
    bool goBackToPlayer = false;
    bool goToThePosition = true;

    void Update()
    {
        HandleTiming();

        if(!goBackToPlayer && !goToThePosition)
        {
            DoAttackThings();
        }
    }

    float damageTickTimer = 0f;

    void DoAttackThings()
    {
        transform.Rotate(0f, 120f * Time.deltaTime, 0f);

        float radius = 25f;
        float inwardStrength = 60f;
        float spinStrength = 90f;
        float liftStrength = 189;
        float damageInterval = 0.25f;

        damageTickTimer -= Time.deltaTime;
        bool doDamageTick = false;
        if (damageTickTimer <= 0f)
        {
            damageTickTimer = damageInterval;
            doDamageTick = true;
        }

        foreach (EnemyIdentifier en in EnemyTracker.Instance.GetCurrentEnemies())
        {
            if (en == null || en.rb == null) continue;

            Vector3 toCenter = transform.position - en.transform.position;
            float dist = toCenter.magnitude;

            if (dist > radius) continue;

            float t = 1f - (dist / radius);

            // inward pull
            Vector3 inward = toCenter.normalized * inwardStrength * t;

            // circular spin
            Vector3 tangent = Vector3.Cross(Vector3.up, toCenter.normalized) * spinStrength * t;

            // slight upward lift
            Vector3 lift = Vector3.up * liftStrength * t;

            Vector3 force = inward + tangent + lift;

            // Smooth continuous force
            Banana_WeaponsPlugin.ApplyKnockBack(en, force);
            //en.rb.AddForce(force, ForceMode.Acceleration);

            // Periodic small damage tick
            if (doDamageTick)
            {
                en.DeliverDamage(
                    en.gameObject,
                    Vector3.zero,
                    en.transform.position,
                    0.2f,    // tiny damage
                    false
                );
            }
        }
        NewMovement nm = NewMovement.Instance;
        float playerRadius = 25f;
        Vector3 toCenterPlayer = transform.position - nm.transform.position;
        float playerDist = toCenterPlayer.magnitude;

        if (playerDist <= playerRadius)
        {
            float t = 1f - (playerDist / playerRadius);

            if (nm.groundProperties && !nm.groundProperties.launchable)
            {
                return;
            }
            nm.jumping = true;
            nm.Invoke("NotJumping", 0.5f);
            nm.jumpCooldown = true;
            nm.Invoke("JumpReady", 0.2f);
            nm.boost = false;
            if (nm.gc.heavyFall)
            {
                nm.fallSpeed = 0f;
                nm.gc.heavyFall = false;
                if (nm.currentFallParticle != null)
                {
                    Object.Destroy(nm.currentFallParticle);
                }
            }

            nm.rb.AddForce(Vector3.up * (liftStrength / 25) * t, ForceMode.VelocityChange);
        }
    }
    void HandleTiming()
    {
        if (!goBackToPlayer && !goToThePosition)
            timer -= Time.deltaTime;
        else
            timer += Time.deltaTime;
        if (timer <= 0 && !goBackToPlayer && !goToThePosition)
        {
            timer = 0;
            goBackToPlayer = true;
        }

        if (timer >= 1 && goToThePosition)
        {
            timer = pets * 1.5f;
            goToThePosition = false;
        }
        else if (timer >= 1 && goBackToPlayer)
        {
            Destroy(gameObject);
        }

        if (goBackToPlayer)
            transform.position = Vector3.Lerp(orgPos, NewMovement.Instance.transform.position, timer);
        if (goToThePosition)
            transform.position = Vector3.Lerp(NewMovement.Instance.transform.position, orgPos, timer);

    }
}
