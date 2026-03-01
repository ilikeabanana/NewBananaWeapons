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
    private static ConfigVar<float> attackRadius;
    private static ConfigVar<float> forceEnemy;
    private static ConfigVar<float> forcePlayer;
    private static ConfigVar<float> damage;
    private static ConfigVar<float> maxCharge;
    private static ConfigVar<float> lightningDamageMultiplier;
    private static ConfigVar<float> cooldownMultiplier;

    public override string GetWeaponDescription()
    {
        return "Left click to swing your oar, launching you forward. Right click to charge a lightning bolt. The longer you charge, the bigger the explosion and damage";
    }

    public override void SetupConfigs(string sectionName)
    {
        attackRadius = new ConfigVar<float>(sectionName, "Attack Radius", 3.5f,
            "Radius of the oar swing attack");

        forceEnemy = new ConfigVar<float>(sectionName, "Enemy Knockback Force", 5000f,
            "Knockback force applied to enemies");

        forcePlayer = new ConfigVar<float>(sectionName, "Player Launch Force", 72f,
            "Forward launch force applied to player on swing");

        damage = new ConfigVar<float>(sectionName, "Swing Damage", 2.5f,
            "Damage dealt by oar swing");

        maxCharge = new ConfigVar<float>(sectionName, "Max Lightning Charge Time", 8.5f,
            "Time to fully charge lightning strike (in seconds)");

        lightningDamageMultiplier = new ConfigVar<float>(sectionName, "Lightning Damage Multiplier", 1.5f,
            "Damage multiplier for lightning strike based on charge");

        cooldownMultiplier = new ConfigVar<float>(sectionName, "Lightning Cooldown Multiplier", 3f,
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