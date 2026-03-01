using NewBananaWeapons;
using System.Collections;
using UnityEngine;

public class TheForceWeapon : BaseWeapon
{
    Animator anim;
    EnemyIdentifier currentTarget;
    Vector3 lastCamForward;
    Vector3 accumulatedThrow;
    Vector3 offset = Vector3.zero;

    GameObject manipulationEffectCurrent = null;

    // Configurable values
    private static ConfigVar<float> distance;
    private static ConfigVar<float> cooldownOnCrush;
    private static ConfigVar<float> throwForceMultiplier;
    private static ConfigVar<float> targetingRange;
    private static ConfigVar<float> bigEnemyDamage;
    private static ConfigVar<int> implodeStylePoints;
    private static ConfigVar<bool> allowImplosionOnBigEnemies;

    float cooldown = 0;

    public override string GetWeaponDescription()
    {
        return "Left click to use the force, big enemies wont be picked up (can be changed), right click to instakill non big enemies. Deal 5 damage to big enemies";
    }

    public override void SetupConfigs(string sectionName)
    {
        distance = new ConfigVar<float>(sectionName, "Hold Distance", 25f,
            "Distance at which enemies are held from the camera");

        cooldownOnCrush = new ConfigVar<float>(sectionName, "Crush Cooldown", 1.5f,
            "Cooldown after crushing an enemy (in seconds)");

        throwForceMultiplier = new ConfigVar<float>(sectionName, "Throw Force Multiplier", 40f,
            "Multiplier for throw force when releasing enemies");

        targetingRange = new ConfigVar<float>(sectionName, "Targeting Range", 25f,
            "Maximum range for grabbing enemies with the Force");

        bigEnemyDamage = new ConfigVar<float>(sectionName, "Big Enemy Crush Damage", 5f,
            "Damage dealt to big enemies when crushing (they can't be imploded)");

        implodeStylePoints = new ConfigVar<int>(sectionName, "Implode Style Points", 100,
            "Style points awarded for imploding an enemy");

        allowImplosionOnBigEnemies = new ConfigVar<bool>(sectionName, "Allow Implosion on Big enemies", false,
            "Allows you to pickup and implode big enemies.");
    }

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (!GunControl.Instance.activated) return;
        if (currentTarget != null && currentTarget.dead)
            currentTarget = null;

        if (anim != null)
        {
            if (anim.GetBool("Crush"))
            {
                anim.SetBool("Crush", false);
            }
            if (cooldown <= 0)
                anim.SetBool("Holding", InputManager.Instance.InputSource.Fire1.IsPressed);
            else
                anim.SetBool("Holding", false);
        }

        if (cooldown > 0)
        {
            cooldown -= Time.deltaTime;
        }

        Transform camTransform = CameraController.Instance.transform;
        if (InputManager.Instance.InputSource.Fire1.IsPressed && cooldown <= 0)
        {
            if (currentTarget != null)
            {
                if (!currentTarget.bigEnemy || allowImplosionOnBigEnemies.Value)
                {
                    Vector3 targetPosition;

                    if (Physics.Raycast(camTransform.position, camTransform.forward, out RaycastHit hit,
                        distance.Value, LayerMaskDefaults.Get(LMD.Environment)))
                    {
                        targetPosition = hit.point;
                    }
                    else
                    {
                        targetPosition = camTransform.position +
                              camTransform.forward * distance.Value;
                    }
                    currentTarget.transform.position = targetPosition + offset;
                    Vector3 camDelta = camTransform.forward - lastCamForward;

                    // Scale controls how strong the throw is
                    accumulatedThrow += camDelta * throwForceMultiplier.Value;

                    lastCamForward = camTransform.forward;

                }

            }
            else
            {
                if (Physics.Raycast(camTransform.position, camTransform.forward, out RaycastHit hit,
                    targetingRange.Value, LayerMaskDefaults.Get(LMD.Enemies)))
                {
                    if (hit.collider.
                        gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier eidd))
                    {
                        EnemyIdentifier eid = eidd.eid;
                        currentTarget = eid;
                        lastCamForward = camTransform.forward;
                        accumulatedThrow = Vector3.zero;
                        distance.Value = Vector3.Distance(camTransform.position, currentTarget.transform.position);
                        Vector3 target = camTransform.position +
                              camTransform.forward * distance.Value;
                        offset = currentTarget.transform.position - target;
                        if (manipulationEffectCurrent != null)
                        {
                            Destroy(manipulationEffectCurrent);
                            manipulationEffectCurrent = null;
                        }
                        manipulationEffectCurrent = Instantiate(AddressableManager.manipulationEffect, currentTarget.transform);
                    }
                }
            }
        }
        else
        {
            if (currentTarget != null && (!currentTarget.bigEnemy || allowImplosionOnBigEnemies.Value))
            {
                currentTarget.rb.velocity = Vector3.zero;
                currentTarget.rb.useGravity = true;
                currentTarget.rb.AddForce(accumulatedThrow, ForceMode.VelocityChange);
            }
            currentTarget = null;
            if (manipulationEffectCurrent != null)
            {
                Destroy(manipulationEffectCurrent);
                manipulationEffectCurrent = null;
            }

        }

        if (InputManager.Instance.InputSource.Fire2.WasPerformedThisFrame)
        {
            if (currentTarget != null)
            {
                if (anim != null)
                    anim.SetBool("Crush", true);
                if (currentTarget.bigEnemy)
                {
                    currentTarget.hitter = "implosion";
                    currentTarget.SimpleDamage(bigEnemyDamage.Value);
                }
                else
                {
                    StyleHUD.Instance.AddPoints(implodeStylePoints.Value, "IMPLODED", gameObject);
                    currentTarget.Explode(false);
                }
                if (manipulationEffectCurrent != null)
                {
                    Destroy(manipulationEffectCurrent);
                    manipulationEffectCurrent = null;
                }
                currentTarget = null;
                cooldown = cooldownOnCrush.Value;

            }
        }
    }
}