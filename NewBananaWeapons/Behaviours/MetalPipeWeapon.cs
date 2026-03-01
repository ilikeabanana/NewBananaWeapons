using NewBananaWeapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MetalPipeWeapon : BaseWeapon
{
    public GameObject metalPipeProjectile;
    public AudioClip slapClip;
    GameObject pipe;
    AudioSource source;
    Animator anim;

    bool isDamaging = false;
    List<EnemyIdentifier> hitEnemies = new List<EnemyIdentifier>();

    // Configurable values
    private static ConfigVar<float> slapRange;
    private static ConfigVar<float> slapDamage;
    private static ConfigVar<float> slapForce;

    public static ConfigVar<float> pipeProjectileMaxDamage;
    public static ConfigVar<float> pipeProjectileDefaultDamage;

    public override string GetWeaponDescription()
    {
        return "Left click to slap, right click to throw pipe. (Can be parried)";
    }

    public override void SetupConfigs(string sectionName)
    {
        pipeProjectileMaxDamage = new ConfigVar<float>(sectionName, "Max Damage Projectile", 10);
        pipeProjectileDefaultDamage = new ConfigVar<float>(sectionName, "Default Damage", 4.5f);

        slapRange = new ConfigVar<float>(sectionName, "Slap Range", 35f,
            "Range of the metal pipe slap attack");

        slapDamage = new ConfigVar<float>(sectionName, "Slap Damage", 3f,
            "Damage dealt by charged slap attack");

        slapForce = new ConfigVar<float>(sectionName, "Slap Force", 20f,
            "Knockback force applied by slap");
    }

    private void Awake()
    {
        anim = GetComponent<Animator>();
        source = GetComponent<AudioSource>();
        StartCoroutine(ShaderManager.ApplyShaderToGameObject(metalPipeProjectile));
    }

    private void Update()
    {
        if (!GunControl.Instance.activated) return;
        anim.SetBool("HoldingLeftClick", InputManager.Instance.InputSource.Fire1.IsPressed);
        anim.SetBool("RightClick", false);
        anim.SetBool("HoldingPipe", pipe == null);
        if (isDamaging)
        {
            RaycastHit hit;
            if (Physics.Raycast(CameraController.Instance.transform.position,
                CameraController.Instance.transform.forward, out hit, slapRange.Value, LayerMaskDefaults.Get(LMD.Enemies)))
            {
                if (hit.collider.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier enemyHit))
                {
                    if (hitEnemies.Contains(enemyHit.eid)) return;
                    enemyHit.eid.hitter = "Metal";
                    enemyHit.eid.DeliverDamage(hit.collider.gameObject, CameraController.Instance.transform.forward * slapForce.Value, enemyHit.transform.position, slapDamage.Value, false);
                    hitEnemies.Add(enemyHit.eid);
                    source.PlayOneShot(slapClip);
                }
            }
        }

        if (InputManager.Instance.InputSource.Fire2.WasPerformedThisFrame && pipe == null)
        {
            anim.SetBool("RightClick", true);
        }
    }

    public void ThrowPipe()
    {
        pipe = Instantiate(metalPipeProjectile, CameraController.Instance.transform.position, CameraController.Instance.transform.rotation);
        pipe.GetComponent<PipeProjectile>().sourceWeapon = gameObject;
    }

    public void EnableDamage()
    {
        RaycastHit hit;
        if (Physics.Raycast(CameraController.Instance.transform.position,
            CameraController.Instance.transform.forward, out hit, slapRange.Value, LayerMaskDefaults.Get(LMD.Enemies)))
        {
            if (hit.collider.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier enemyHit))
            {
                enemyHit.eid.hitter = "Metal";
                enemyHit.eid.DeliverDamage(hit.collider.gameObject, CameraController.Instance.transform.forward * slapForce.Value, enemyHit.transform.position, slapDamage.Value, false, sourceWeapon: gameObject);
                source.PlayOneShot(slapClip);
            }
        }
    }
}