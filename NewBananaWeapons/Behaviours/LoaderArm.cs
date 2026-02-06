using BepInEx.Configuration;
using NewBananaWeapons;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text;
using UnityEngine;

public class LoaderArm : BaseWeapon
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

    // Store original collision states for ALL layer pairs
    private Dictionary<(int, int), bool> originalAllCollisionStates = new Dictionary<(int, int), bool>();
    private bool hasStoredCollisions = false;

    Animator anim;

    bool isHoldingPunch = false;

    public void ChargeGauntlet()
    {
        isCharging = true;
        isHoldingPunch = true;
        chargeTime = 0f;
    }

    public override void SetupConfigs(string sectionName, ConfigFile Config)
    {
        Config.Bind<float>(sectionName, "Full Charged Punch Velocity", 180);
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
        Banana_WeaponsPlugin.LaunchPlayer(punchDirection, launchVelocity, true);
        isCharging = false;
        isHoldingCharge = false;
        SaveAllCollisionStates();
        hasStoredCollisions = true;
    }

    private void SaveAllCollisionStates()
    {
        originalAllCollisionStates.Clear();
        for (int i = 0; i < 32; i++)
        {
            for (int j = i; j < 32; j++)
            {
                bool isIgnored = Physics.GetIgnoreLayerCollision(i, j);
                originalAllCollisionStates[(i, j)] = isIgnored;
            }
        }
    }

    private void DisablePlayerCollisionExceptEnvironment()
    {
        int playerLayer = MonoSingleton<NewMovement>.Instance.gameObject.layer;
        int environmentLayer = LayerMask.NameToLayer("Environment");
        int outdoorsLayer = LayerMask.NameToLayer("Outdoors");
        for (int i = 0; i < 32; i++)
        {
            if (i == environmentLayer || i == outdoorsLayer)
                continue;
            Physics.IgnoreLayerCollision(playerLayer, i, true);
        }
        VerticalClippingBlocker vcb =
            MonoSingleton<NewMovement>.Instance.GetComponent<VerticalClippingBlocker>();
        if (vcb != null)
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

    private void RestoreAllCollisionStates()
    {
        if (!hasStoredCollisions)
            return;

        // Restore all layer collision states
        foreach (var kvp in originalAllCollisionStates)
        {
            int layer1 = kvp.Key.Item1;
            int layer2 = kvp.Key.Item2;
            bool wasIgnored = kvp.Value;

            Physics.IgnoreLayerCollision(layer1, layer2, wasIgnored);
        }

        hasStoredCollisions = false;
    }

    private void Awake()
    {
        anim = GetComponent<Animator>();
        StartCoroutine(ShaderManager.ApplyShaderToGameObject(GetComponent<Punch>().dustParticle));
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

        if (isPunching)
        {
            DisablePlayerCollisionExceptEnvironment();
            DisableEnemyLayerCollisions();

            if (MonoSingleton<NewMovement>.Instance.gc != null)
                MonoSingleton<NewMovement>.Instance.gc.enabled = false;
            if (MonoSingleton<NewMovement>.Instance.wc != null)
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
                    BloodsplatterManager bsm = MonoSingleton<BloodsplatterManager>.Instance;
                    GoreZone goreZone = enemyHit.eid.GetGoreZone();
                    GoreType got = GoreType.Head;
                    GameObject gore = bsm.GetGore(got, this, false);
                    gore.transform.position = enemyHit.transform.position;
                    if (goreZone != null && goreZone.goreZone != null)
                    {
                        gore.transform.SetParent(goreZone.goreZone, true);
                    }
                    Bloodsplatter component = gore.GetComponent<Bloodsplatter>();
                    if (component != null)
                    {
                        component.GetReady();
                    }
                    TimeController.Instance.HitStop(0.05f);
                    alreadyHitEnemies.Add(enemyHit.eid);
                }
            }

            if (punchTimer <= 0f)
            {
                EndPunch();
            }
        }
    }

    private void EndPunch()
    {
        isPunching = false;
        chargeTime = 0f;

        RestoreAllCollisionStates();

        VerticalClippingBlocker vcb =
           MonoSingleton<NewMovement>.Instance.GetComponent<VerticalClippingBlocker>();
        if (vcb != null)
            vcb.enabled = true;
        if (MonoSingleton<NewMovement>.Instance.gc != null)
            MonoSingleton<NewMovement>.Instance.gc.enabled = true;
        if (MonoSingleton<NewMovement>.Instance.wc != null)
            MonoSingleton<NewMovement>.Instance.wc.enabled = true;

        //MonoSingleton<NewMovement>.Instance.Launch(Vector3.up, 0.1f, true);
    }

    private void OnDisable()
    {
        if (isPunching)
        {
            EndPunch();
        }
    }
     
    private void OnDestroy()
    {
        if (isPunching)
        {
            EndPunch();
        }
    }
}