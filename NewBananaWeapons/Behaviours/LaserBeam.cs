using BepInEx.Configuration;
using System.Collections;
using UnityEngine;

public class LaserBeam : BaseWeapon
{
    public GameObject chargeParticle;

    float charging = 0.0f;
    InputManager inman;
    CameraController cam;
    LineRenderer line;

    float damageDelay;

    // Configurable values
    private static ConfigEntry<float> chargeTime;
    private static ConfigEntry<float> laserRadius;
    private static ConfigEntry<float> laserRange;
    private static ConfigEntry<float> damage;
    private static ConfigEntry<float> damageTickRate;
    public override void SetupConfigs(string sectionName, ConfigFile Config)
    {
        chargeTime = Config.Bind<float>(sectionName, "Charge Time", 1f,
            "Time required to charge the laser before it fires (in seconds)");

        laserRadius = Config.Bind<float>(sectionName, "Laser Radius", 3f,
            "Radius of the laser beam spherecast");

        laserRange = Config.Bind<float>(sectionName, "Laser Range", 80f,
            "Maximum range of the laser beam");

        damage = Config.Bind<float>(sectionName, "Damage Per Tick", 0.5f,
            "Damage dealt per damage tick");

        damageTickRate = Config.Bind<float>(sectionName, "Damage Tick Rate", 0.1f,
            "Time between damage ticks (in seconds)");

    }

    void Awake()
    {
        inman = InputManager.Instance;
        cam = CameraController.Instance;
        line = gameObject.GetComponent<LineRenderer>();
        line.widthMultiplier = laserRadius.Value;
    }

    // Update is called once per frame
    void Update()
    {
        damageDelay -= Time.deltaTime;

        if (inman.InputSource.Fire1.IsPressed)
        {
            charging += Time.deltaTime;

            if (charging > chargeTime.Value)
            {
                chargeParticle.SetActive(false);
                if (Physics.SphereCast(cam.GetDefaultPos(), laserRadius.Value,
                    cam.transform.forward, out RaycastHit hit, laserRange.Value, LayerMaskDefaults.Get(LMD.EnemiesAndEnvironment)))
                {
                    if (hit.collider.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier eidd))
                    {
                        if (damageDelay <= 0)
                        {
                            eidd.eid.hitter = "beam";
                            eidd.eid.SimpleDamage(damage.Value);
                            damageDelay = damageTickRate.Value;
                        }


                    }

                    line.positionCount = 2;
                    line.SetPosition(0, cam.GetDefaultPos() + Vector3.down);
                    line.SetPosition(1, hit.point);
                }
                else
                {
                    line.positionCount = 2;
                    line.SetPosition(0, cam.GetDefaultPos() + Vector3.down);
                    line.SetPosition(1, cam.GetDefaultPos() + (cam.transform.forward * 10));
                }
            }
            else
            {
                chargeParticle.SetActive(true);
                line.positionCount = 0;
            }
        }
        else
        {
            chargeParticle.SetActive(false);
            line.positionCount = 0;
            charging = 0;
        }
    }
}