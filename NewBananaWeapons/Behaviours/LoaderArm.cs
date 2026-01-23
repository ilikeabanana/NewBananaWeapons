using NewBananaWeapons;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text;
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

    // Store original collision states
    private Dictionary<(int, int), bool> originalCollisionStates = new Dictionary<(int, int), bool>();
    private Dictionary<(int, int), bool> originalPlayerCollisionStates = new Dictionary<(int, int), bool>();

    Animator anim;

    bool isHoldingPunch = false;

    public void ChargeGauntlet()
    {
        isCharging = true;
        isHoldingPunch = true;
        chargeTime = 0f;
    }

    public void Release()
    {
        isHoldingPunch = false;
        isPunching = true;
        punchTimer = punchDuration;
        punchDirection = CameraController.Instance.transform.forward;
        alreadyHitEnemies.Clear();

        float chargePercent = Mathf.Clamp01(chargeTime / maxChargeTime);

        float launchVelocity = chargedPunchVelocity * chargePercent;
        NewMovement.Instance.Launch(punchDirection, launchVelocity, true);
        isCharging = false;
        isHoldingCharge = false;

        int groundCheck = LayerMask.NameToLayer("GroundCheck");
        int ignoreRaycasts = LayerMask.NameToLayer("Ignore Raycast");
        int defaultLayer = LayerMask.NameToLayer("Default");
        int limb = LayerMask.NameToLayer("Limb");
        int bigcorpse = LayerMask.NameToLayer("BigCorpse");
        int etrigger = LayerMask.NameToLayer("EnemyTrigger");
        int gib = LayerMask.NameToLayer("Gib");

        // Save original collision states before modifying
        originalCollisionStates.Clear();
        SaveCollisionState(ignoreRaycasts, limb);
        SaveCollisionState(defaultLayer, limb);
        SaveCollisionState(groundCheck, limb);
        SaveCollisionState(ignoreRaycasts, bigcorpse);
        SaveCollisionState(defaultLayer, bigcorpse);
        SaveCollisionState(groundCheck, bigcorpse);
        SaveCollisionState(ignoreRaycasts, etrigger);
        SaveCollisionState(defaultLayer, etrigger);
        SaveCollisionState(groundCheck, etrigger);
        SaveCollisionState(ignoreRaycasts, gib);
        SaveCollisionState(defaultLayer, gib);
        SaveCollisionState(groundCheck, gib);

        // Save original player collision states
        int playerLayer = MonoSingleton<NewMovement>.Instance.gameObject.layer;
        int environmentLayer = LayerMask.NameToLayer("Environment");
        int outdoorsLayer = LayerMask.NameToLayer("Outdoors");

        originalPlayerCollisionStates.Clear();
        for (int i = 0; i < 32; i++)
        {
            if (i == environmentLayer || i == outdoorsLayer)
                continue;
            SaveCollisionState(playerLayer, i, originalPlayerCollisionStates);
        }
    }

    private void SaveCollisionState(int layer1, int layer2)
    {
        bool currentState = !Physics.GetIgnoreLayerCollision(layer1, layer2);
        originalCollisionStates[(layer1, layer2)] = currentState;
    }

    private void SaveCollisionState(int layer1, int layer2, Dictionary<(int, int), bool> dict)
    {
        bool currentState = !Physics.GetIgnoreLayerCollision(layer1, layer2);
        dict[(layer1, layer2)] = currentState;
    }

    private void DisablePlayerCollisionExceptEnvironment()
    {
        int playerLayer = MonoSingleton<NewMovement>.Instance.gameObject.layer;
        int environmentLayer = LayerMask.NameToLayer("Environment");
        int outdoorsLayer = LayerMask.NameToLayer("Outdoors");
        for (int i = 0; i < 32; i++)
        {
            // Skip environment + outdoors so those still collide
            if (i == environmentLayer || i == outdoorsLayer)
                continue;
            Physics.IgnoreLayerCollision(playerLayer, i, true);
        }
        // Also disable vertical clipping blocker like before
        VerticalClippingBlocker vcb =
            MonoSingleton<NewMovement>.Instance.GetComponent<VerticalClippingBlocker>();
        vcb.enabled = false;
    }

    private void DisableEnemyLayerCollisions()
    {
        int groundCheck = LayerMask.NameToLayer("GroundCheck");
        int ignoreRaycasts = LayerMask.NameToLayer("Ignore Raycast");
        int defaultLayer = LayerMask.NameToLayer("Default");
        int limb = LayerMask.NameToLayer("Limb");
        int bigcorpse = LayerMask.NameToLayer("BigCorpse");
        int etrigger = LayerMask.NameToLayer("EnemyTrigger");
        int gib = LayerMask.NameToLayer("Gib");

        Physics.IgnoreLayerCollision(ignoreRaycasts, limb, true);
        Physics.IgnoreLayerCollision(defaultLayer, limb, true);
        Physics.IgnoreLayerCollision(groundCheck, limb, true);
        Physics.IgnoreLayerCollision(ignoreRaycasts, bigcorpse, true);
        Physics.IgnoreLayerCollision(defaultLayer, bigcorpse, true);
        Physics.IgnoreLayerCollision(groundCheck, bigcorpse, true);
        Physics.IgnoreLayerCollision(ignoreRaycasts, etrigger, true);
        Physics.IgnoreLayerCollision(defaultLayer, etrigger, true);
        Physics.IgnoreLayerCollision(groundCheck, etrigger, true);
        Physics.IgnoreLayerCollision(ignoreRaycasts, gib, true);
        Physics.IgnoreLayerCollision(defaultLayer, gib, true);
        Physics.IgnoreLayerCollision(groundCheck, gib, true);
    }

    private void RestoreCollisionStates()
    {
        foreach (var kvp in originalCollisionStates)
        {
            int layer1 = kvp.Key.Item1;
            int layer2 = kvp.Key.Item2;
            bool shouldCollide = kvp.Value;

            // Set to ignore if it should NOT collide (inverse logic)
            Physics.IgnoreLayerCollision(layer1, layer2, !shouldCollide);
        }

        foreach (var kvp in originalPlayerCollisionStates)
        {
            int layer1 = kvp.Key.Item1;
            int layer2 = kvp.Key.Item2;
            bool shouldCollide = kvp.Value;

            Physics.IgnoreLayerCollision(layer1, layer2, !shouldCollide);
        }
    }

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        anim.SetBool("HoldingPunch", InputManager.Instance.InputSource.Punch.IsPressed);

        if (isCharging && isHoldingPunch)
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

        if (!isPunching) return;

        // Continuously disable collisions every frame while punching
        DisablePlayerCollisionExceptEnvironment();
        DisableEnemyLayerCollisions();

        // Continuously disable gc and wc every frame while punching
        MonoSingleton<NewMovement>.Instance.gc.enabled = false;
        MonoSingleton<NewMovement>.Instance.wc.enabled = false;

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
                TimeController.Instance.HitStop(0.05f);
            }
        }

        if (punchTimer <= 0f)
        {
            isPunching = false;
            chargeTime = 0f;

            // Restore original collision states
            RestoreCollisionStates();

            // Re-enable components
            VerticalClippingBlocker vcb =
               MonoSingleton<NewMovement>.Instance.GetComponent<VerticalClippingBlocker>();
            vcb.enabled = true;
            MonoSingleton<NewMovement>.Instance.gc.enabled = true;
            MonoSingleton<NewMovement>.Instance.wc.enabled = true;
        }
    }
}