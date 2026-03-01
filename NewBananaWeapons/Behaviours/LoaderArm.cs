using NewBananaWeapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoaderArm : BaseWeapon
{
    // ================= CONFIG ENTRIES =================
    private static ConfigVar<float> cfgMaxChargeTime;
    private static ConfigVar<float> cfgMinDamageMultiplier;
    private static ConfigVar<float> cfgMaxDamageMultiplier;
    private static ConfigVar<float> cfgBaseDamage;

    private static ConfigVar<float> cfgVelocityDamagePerMPS;
    private static ConfigVar<float> cfgChargedPunchVelocity;

    private static ConfigVar<float> cfgPunchForce;
    private static ConfigVar<float> cfgHitRadius;
    private static ConfigVar<float> cfgHitDistance;
    private static ConfigVar<float> cfgPunchDuration;

    // ================= RUNTIME VALUES =================
    private static float maxChargeTime;
    private static float minDamageMultiplier;
    private static float maxDamageMultiplier;
    private static float baseDamage;

    private static float velocityDamagePerMPS;
    private static float chargedPunchVelocity;

    private static float punchForce;
    private static float hitRadius;
    private static float hitDistance;
    private static float punchDuration;

    private float chargeTime;
    private bool isCharging;
    private bool isHoldingCharge;
    private bool isPunching;

    private float punchTimer;
    private Vector3 punchDirection;

    private Animator anim;

    private readonly List<EnemyIdentifier> alreadyHitEnemies = new List<EnemyIdentifier>();
    private readonly Dictionary<(int, int), bool> originalAllCollisionStates = new Dictionary<(int, int), bool>();
    private bool hasStoredCollisions;

    // ================= CONFIG SETUP =================
    public override void SetupConfigs(string sectionName)
    {
        cfgMaxChargeTime = new ConfigVar<float>(sectionName, "Max Charge Time", 2.5f);
        cfgMinDamageMultiplier = new ConfigVar<float>(sectionName, "Min Damage Multiplier", 6f);
        cfgMaxDamageMultiplier = new ConfigVar<float>(sectionName, "Max Damage Multiplier", 27f);
        cfgBaseDamage = new ConfigVar<float>(sectionName, "Base Damage", 1.5f);

        cfgVelocityDamagePerMPS = new ConfigVar<float>(sectionName, "Velocity Damage Per MPS", 0.15f);
        cfgChargedPunchVelocity = new ConfigVar<float>(sectionName, "Charged Punch Velocity", 180f);

        cfgPunchForce = new ConfigVar<float>(sectionName, "Punch Force", 10000f);
        cfgHitRadius = new ConfigVar<float>(sectionName, "Hit Radius", 5f);
        cfgHitDistance = new ConfigVar<float>(sectionName, "Hit Distance", 1f);
        cfgPunchDuration = new ConfigVar<float>(sectionName, "Punch Duration", 0.2f);

        ApplyConfigValues();
    }

    public override string GetWeaponDescription()
    {
        return "Press punch to just punch, holding punch allows you to charge up a piercing punch, dealing damage based on charge time and velocity";
    }

    private void ApplyConfigValues()
    {
        maxChargeTime = cfgMaxChargeTime.Value;
        minDamageMultiplier = cfgMinDamageMultiplier.Value;
        maxDamageMultiplier = cfgMaxDamageMultiplier.Value;
        baseDamage = cfgBaseDamage.Value;

        velocityDamagePerMPS = cfgVelocityDamagePerMPS.Value;
        chargedPunchVelocity = cfgChargedPunchVelocity.Value;

        punchForce = cfgPunchForce.Value;
        hitRadius = cfgHitRadius.Value;
        hitDistance = cfgHitDistance.Value;
        punchDuration = cfgPunchDuration.Value;
    }

    // ================= LIFECYCLE =================
    private void Awake()
    {
        anim = GetComponent<Animator>();
        StartCoroutine(
            ShaderManager.ApplyShaderToGameObject(GetComponent<Punch>().dustParticle)
        );
    }

    private void Update()
    {
        anim.SetBool("HoldingPunch", InputManager.Instance.InputSource.Punch.IsPressed);
        ApplyConfigValues();
        HandleCharging();
        HandlePunching();
    }

    // ================= INPUT =================
    public void ChargeGauntlet()
    {
        isCharging = true;
        chargeTime = 0f;
    }

    public void Release()
    {
        if (!isCharging && !isHoldingCharge)
            return;

        isCharging = false;
        isHoldingCharge = false;

        isPunching = true;
        punchTimer = punchDuration;
        punchDirection = CameraController.Instance.transform.forward;

        alreadyHitEnemies.Clear();

        float chargePercent = Mathf.Clamp01(chargeTime / maxChargeTime);
        Banana_WeaponsPlugin.LaunchPlayer(
            punchDirection,
            chargedPunchVelocity * chargePercent,
            true
        );

        SaveAllCollisionStates();
        hasStoredCollisions = true;
    }

    // ================= CHARGING =================
    private void HandleCharging()
    {
        if (!isCharging)
            return;

        chargeTime += Time.deltaTime;

        if (chargeTime >= maxChargeTime)
        {
            chargeTime = maxChargeTime;
            isCharging = false;
            isHoldingCharge = true;

            Instantiate(
                AddressableManager.blueFlash,
                CameraController.Instance.transform.position,
                Quaternion.identity
            );
        }
    }

    // ================= PUNCHING =================
    private void HandlePunching()
    {
        if (!isPunching)
            return;

        DisablePlayerCollisionExceptEnvironment();
        DisableEnemyLayerCollisions();

        if (NewMovement.Instance.gc) NewMovement.Instance.gc.enabled = false;
        if (NewMovement.Instance.wc) NewMovement.Instance.wc.enabled = false;

        punchTimer -= Time.deltaTime;

        var hits = Physics.SphereCastAll(
            CameraController.Instance.transform.position,
            hitRadius,
            punchDirection,
            hitDistance
        );

        foreach (var hit in hits)
        {
            if (!hit.collider.TryGetComponent<EnemyIdentifierIdentifier>(out var enemy))
                continue;

            if (alreadyHitEnemies.Contains(enemy.eid))
                continue;

            DealDamage(enemy, hit);
        }

        if (punchTimer <= 0f)
            EndPunch();
    }

    private void DealDamage(EnemyIdentifierIdentifier enemy, RaycastHit hit)
    {
        float chargePercent = Mathf.Clamp01(chargeTime / maxChargeTime);
        float chargeMultiplier = Mathf.Lerp(
            minDamageMultiplier,
            maxDamageMultiplier,
            chargePercent
        );

        float velocityBonus = NewMovement.Instance.rb.velocity.magnitude * velocityDamagePerMPS;
        float finalDamage = baseDamage * (chargeMultiplier + velocityBonus);

        enemy.eid.hitter = "riskofrain2loaderreference";
        enemy.eid.DeliverDamage(
            hit.collider.gameObject,
            punchDirection * punchForce * chargePercent,
            hit.point,
            finalDamage,
            false
        );

        TimeController.Instance.HitStop(0.05f);
        alreadyHitEnemies.Add(enemy.eid);
    }

    // ================= COLLISIONS =================
    private void SaveAllCollisionStates()
    {
        originalAllCollisionStates.Clear();

        for (int i = 0; i < 32; i++)
            for (int j = i; j < 32; j++)
                originalAllCollisionStates[(i, j)] =
                    Physics.GetIgnoreLayerCollision(i, j);
    }

    private void RestoreAllCollisionStates()
    {
        if (!hasStoredCollisions)
            return;

        foreach (var kvp in originalAllCollisionStates)
            Physics.IgnoreLayerCollision(kvp.Key.Item1, kvp.Key.Item2, kvp.Value);

        hasStoredCollisions = false;
    }

    private void DisablePlayerCollisionExceptEnvironment()
    {
        int player = NewMovement.Instance.gameObject.layer;
        int env = LayerMask.NameToLayer("Environment");
        int outdoors = LayerMask.NameToLayer("Outdoors");

        for (int i = 0; i < 32; i++)
            if (i != env && i != outdoors)
                Physics.IgnoreLayerCollision(player, i, true);

        var vcb = NewMovement.Instance.GetComponent<VerticalClippingBlocker>();
        if (vcb) vcb.enabled = false;
    }

    private void DisableEnemyLayerCollisions()
    {
        int[] layers =
        {
            LayerMask.NameToLayer("GroundCheck"),
            LayerMask.NameToLayer("Ignore Raycast"),
            LayerMask.NameToLayer("Default")
        };

        int[] enemyLayers =
        {
            LayerMask.NameToLayer("Limb"),
            LayerMask.NameToLayer("BigCorpse"),
            LayerMask.NameToLayer("EnemyTrigger"),
            LayerMask.NameToLayer("Gib")
        };

        foreach (var a in layers)
            foreach (var b in enemyLayers)
                Physics.IgnoreLayerCollision(a, b, true);
    }

    // ================= CLEANUP =================
    private void EndPunch()
    {
        isPunching = false;
        chargeTime = 0f;

        RestoreAllCollisionStates();

        var nm = NewMovement.Instance;
        if (nm.gc) nm.gc.enabled = true;
        if (nm.wc) nm.wc.enabled = true;

        var vcb = nm.GetComponent<VerticalClippingBlocker>();
        if (vcb) vcb.enabled = true;
    }

    private void OnDisable() => EndPunch();
    private void OnDestroy() => EndPunch();
}