using JetBrains.Annotations;
using NewBananaWeapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OarWeapon : MonoBehaviour
{
    [SerializeField] AudioClip swingOne;
    [SerializeField] AudioClip swingTwo;

    AudioSource source;
    bool damageActive = false;
    float attackRadius = 2.5f;
    float force = 5000;
    float damage = 5;
    Animator anim;

    float charge = 0;
    float maxCharge = 7.5f;

    EnemyIdentifier targetedEID;
    GameObject lightningBoltWindUp = null;

    List<EnemyIdentifier> hitEnemies = new List<EnemyIdentifier>();
    // Use this for initialization
    void Awake()
    {
        anim = GetComponent<Animator>();
        source = GetComponent<AudioSource>();
    }

    public void AttackForward()
    {
        float force = 48;
        NewMovement.Instance.Launch(CameraController.Instance.transform.forward, force, true);
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetBool("Holding", MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed);
        anim.SetBool("Rightclick", MonoSingleton<InputManager>.Instance.InputSource.Fire2.IsPressed);
        if (damageActive)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, attackRadius);
            if(hits.Length > 0)
            {
                foreach (var hit in hits)
                {
                    if(hit.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier enemyHit))
                    {
                        if (hitEnemies.Contains(enemyHit.eid)) continue;
                        enemyHit.eid.hitter = "oar";
                        enemyHit.eid.DeliverDamage(hit.gameObject, CameraController.Instance.transform.forward * force * 20, enemyHit.transform.position, damage, false);
                        hitEnemies.Add(enemyHit.eid);
                    }
                }
            }
        }

        if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame)
        {
            if(targetedEID == null)
            {
                // get a random enemy for now
                List<EnemyIdentifier> identifiers = EnemyTracker.Instance.GetCurrentEnemies();

                targetedEID = identifiers[Random.Range(0, identifiers.Count)];
            }
        } else if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasCanceledThisFrame)
        {
            if(lightningBoltWindUp != null && targetedEID != null)
            {
                StopCharge();
            }
        }


        
        if(targetedEID != null)
        {
            if (targetedEID.dead)
            {
                targetedEID = null;
                Destroy(lightningBoltWindUp);
                charge = 0;
                return;
            }
            charge += Time.deltaTime;
            if(charge >= maxCharge)
            {
                charge = maxCharge;
                anim.SetBool("Rightclick", false);
                StopCharge();
                return;
            }
            if(lightningBoltWindUp == null)
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
        explosion.GetComponent<LightningStrikeExplosive>().damageMultiplier = (charge*2) / maxCharge;
        Destroy(lightningBoltWindUp);
        targetedEID = null;
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
