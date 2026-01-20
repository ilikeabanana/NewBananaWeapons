using NewBananaWeapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoaderArm : MonoBehaviour
{
    [Header("Charge Settings")]
    private float maxChargeTime = 2.5f;
    private float minDamageMultiplier = 6f;
    private float maxDamageMultiplier = 27f;
     private float baseDamage = 1.5f;

    [Header("Velocity Scaling")]
    private float velocityDamagePerMPS = 0.15f;
    private float chargedPunchVelocity = 180f;

    [Header("Launch Settings")]
    private float punchForce = 10000f;
    private float hitRadius = 5f;
    private float hitDistance = 1f;

    private float chargeTime = 0f;
    private bool isCharging = false;
    private bool isHoldingCharge = false;
    private bool isPunching = false;
    private float punchTimer = 0f;
    private float punchDuration = 0.2f;

    private List<EnemyIdentifier> alreadyHitEnemies = new List<EnemyIdentifier>();
    private Vector3 punchDirection;

    void Update()
    {
        if (InputManager.Instance.InputSource.Punch.WasPerformedThisFrame && !isCharging && !isHoldingCharge && !isPunching)
        {
            isCharging = true;
            chargeTime = 0f;
        }

        if (InputManager.Instance.InputSource.Punch.IsPressed && isCharging)
        {
            chargeTime += Time.deltaTime;

            if (chargeTime >= maxChargeTime)
            {
                chargeTime = maxChargeTime;
                isCharging = false;
                Instantiate(AddressableManager.blueFlash, CameraController.Instance.transform.position, Quaternion.identity);
                isHoldingCharge = true;
            }
        }

        if (InputManager.Instance.InputSource.Punch.WasCanceledThisFrame && (isCharging || isHoldingCharge))
        {
            isPunching = true;
            punchTimer = punchDuration;
            punchDirection = CameraController.Instance.transform.forward;
            alreadyHitEnemies.Clear();

            float chargePercent = Mathf.Clamp01(chargeTime / maxChargeTime);

            float launchVelocity = chargedPunchVelocity * chargePercent;
            NewMovement.Instance.Launch(punchDirection, launchVelocity, true);
            isCharging = false;
            isHoldingCharge = false;
        }
        if (!isPunching) return;

        punchTimer -= Time.deltaTime;

        RaycastHit[] hits = Physics.SphereCastAll(
            CameraController.Instance.transform.position,
            hitRadius,
            punchDirection,
            hitDistance
        );

        foreach (var hit in hits)
        {
            if (hit.collider.TryGetComponent<EnemyIdentifierIdentifier>(out var enemyHit))
            {
                if (alreadyHitEnemies.Contains(enemyHit.eid))
                    continue;

                float chargePercent = Mathf.Clamp01(chargeTime / maxChargeTime);
                float chargeDamageMultiplier = Mathf.Lerp(minDamageMultiplier, maxDamageMultiplier, chargePercent);

                float currentSpeed = NewMovement.Instance.rb.velocity.magnitude;
                float velocityDamageBonus = (currentSpeed * velocityDamagePerMPS);

                float totalDamageMultiplier = chargeDamageMultiplier + velocityDamageBonus;
                float finalDamage = baseDamage * totalDamageMultiplier;

                enemyHit.eid.hitter = "riskofrain2loaderreference";
                enemyHit.eid.DeliverDamage(
                    hit.collider.gameObject,
                    punchDirection * punchForce * chargePercent,
                    hit.point,
                    finalDamage,
                    false,
                    sourceWeapon: null
                );

                alreadyHitEnemies.Add(enemyHit.eid);
            }
        }

        if (punchTimer <= 0f)
        {
            isPunching = false;
            chargeTime = 0f;
        }
    }


}