using BepInEx.Configuration;
using JetBrains.Annotations;
using NewBananaWeapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OarWeapon : BaseWeapon
{
    [SerializeField] AudioClip swingOne;
    [SerializeField] AudioClip swingTwo;

    AudioSource source;
    bool damageActive = false;
    Animator anim;

    float charge = 0;
    float cooldown = 0.0f;

    EnemyIdentifier targetedEID;
    GameObject lightningBoltWindUp = null;

    List<EnemyIdentifier> hitEnemies = new List<EnemyIdentifier>();

    // Configurable values
    private ConfigEntry<float> attackRadius;
    private ConfigEntry<float> forceEnemy;
    private ConfigEntry<float> forcePlayer;
    private ConfigEntry<float> damage;
    private ConfigEntry<float> maxCharge;
    private ConfigEntry<float> lightningDamageMultiplier;
    private ConfigEntry<float> cooldownMultiplier;

    public override void SetupConfigs(string sectionName, ConfigFile Config)
    {
        attackRadius = Config.Bind<float>(sectionName, "Attack Radius", 3.5f,
            "Radius of the oar swing attack");

        forceEnemy = Config.Bind<float>(sectionName, "Enemy Knockback Force", 5000f,
            "Knockback force applied to enemies");

        forcePlayer = Config.Bind<float>(sectionName, "Player Launch Force", 72f,
            "Forward launch force applied to player on swing");

        damage = Config.Bind<float>(sectionName, "Swing Damage", 2.5f,
            "Damage dealt by oar swing");

        maxCharge = Config.Bind<float>(sectionName, "Max Lightning Charge Time", 8.5f,
            "Time to fully charge lightning strike (in seconds)");

        lightningDamageMultiplier = Config.Bind<float>(sectionName, "Lightning Damage Multiplier", 1.5f,
            "Damage multiplier for lightning strike based on charge");

        cooldownMultiplier = Config.Bind<float>(sectionName, "Lightning Cooldown Multiplier", 3f,
            "Cooldown after lightning strike = charge ratio * this value");
    }

    // Use this for initialization
    void Awake()
    {
        anim = GetComponent<Animator>();
        source = GetComponent<AudioSource>();
    }

    public void AttackForward()
    {
        NewMovement.Instance.Launch(CameraController.Instance.transform.forward, forcePlayer.Value, true);
    }

    // Update is called once per frame
    void Update()
    {
        if (!GunControl.Instance.activated) return;
        anim.SetBool("Holding", MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed);
        if (cooldown <= 0)
            anim.SetBool("Rightclick", MonoSingleton<InputManager>.Instance.InputSource.Fire2.IsPressed);
        if (damageActive)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, attackRadius.Value);
            if (hits.Length > 0)
            {
                foreach (var hit in hits)
                {
                    if (hit.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier enemyHit))
                    {
                        if (hitEnemies.Contains(enemyHit.eid)) continue;
                        enemyHit.eid.hitter = "oar";
                        enemyHit.eid.DeliverDamage(hit.gameObject, CameraController.Instance.transform.forward * forceEnemy.Value * 20, enemyHit.transform.position, damage.Value, false);
                        hitEnemies.Add(enemyHit.eid);
                    }
                }
            }
        }

        if (cooldown > 0)
        {
            cooldown -= Time.deltaTime;
        }

        if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame && cooldown <= 0)
        {
            if (targetedEID == null)
            {
                // get a random enemy for now
                List<EnemyIdentifier> identifiers = EnemyTracker.Instance.GetCurrentEnemies();

                targetedEID = identifiers[Random.Range(0, identifiers.Count)];
            }
        }
        else if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasCanceledThisFrame)
        {
            if (lightningBoltWindUp != null && targetedEID != null)
            {
                StopCharge();
            }
        }



        if (targetedEID != null)
        {
            if (targetedEID.dead)
            {
                targetedEID = null;
                Destroy(lightningBoltWindUp);
                charge = 0;
                return;
            }
            charge += Time.deltaTime;
            if (charge >= maxCharge.Value)
            {
                charge = maxCharge.Value;
                anim.SetBool("Rightclick", false);
                StopCharge();
                return;
            }
            if (lightningBoltWindUp == null)
            {
                lightningBoltWindUp = Instantiate(AddressableManager.lightningBoltWindup, targetedEID.transform.position, Quaternion.identity);
            }

            Follow[] components = lightningBoltWindUp.GetComponents<Follow>();
            for (int i = 0; i < components.Length; i++)
            {
                components[i].target = targetedEID.transform;
            }
        }

    }
    void StopCharge()
    {
        GameObject explosion = Instantiate(AddressableManager.lightningBolt, lightningBoltWindUp.transform.position, Quaternion.identity);
        explosion.GetComponent<LightningStrikeExplosive>().damageMultiplier = (charge * lightningDamageMultiplier.Value) / maxCharge.Value;
        Destroy(lightningBoltWindUp);
        targetedEID = null;
        cooldown = (charge / maxCharge.Value) * cooldownMultiplier.Value;
        charge = 0;

    }
    public void activateDamage()
    {
        damageActive = true;
        hitEnemies.Clear();
    }
    public void deactivateDamage()
    {
        damageActive = false;
    }

    public void PlaySwing1Audio()
    {
        source.PlayOneShot(swingOne);
    }

    public void PlaySwing2Audio()
    {
        source.PlayOneShot(swingTwo);
    }
    void OnEnable()
    {
        deactivateDamage();
    }
}