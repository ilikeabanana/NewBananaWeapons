using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MetalPipeWeapon : MonoBehaviour
{
    public GameObject metalPipeProjectile;
    public AudioClip slapClip;
    AudioSource source;
    Animator anim;

    bool isDamaging = false;
    List<EnemyIdentifier> hitEnemies = new List<EnemyIdentifier>();

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        anim.SetBool("HoldingLeftClick", InputManager.Instance.InputSource.Fire1.IsPressed);
        anim.SetBool("RightClick", false);
        if (isDamaging)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, 2.5f);
            if (hits.Length > 0)
            {
                foreach (var hit in hits)
                {
                    if (hit.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier enemyHit))
                    {
                        if (hitEnemies.Contains(enemyHit.eid)) continue;
                        enemyHit.eid.hitter = "oar";
                        enemyHit.eid.DeliverDamage(hit.gameObject, CameraController.Instance.transform.forward * 20, enemyHit.transform.position, 1, false);
                        hitEnemies.Add(enemyHit.eid);
                        source.PlayOneShot(slapClip);
                    }
                }
            }
        }

        if (InputManager.Instance.InputSource.Fire2.WasPerformedThisFrame)
        {
            anim.SetBool("RightClick", true);
            anim.SetBool("HoldingPipe", false);
            Instantiate(metalPipeProjectile, CameraController.Instance.transform.position, CameraController.Instance.transform.rotation);
        
        }
    }
     
    public void EnableDamage()
    {
        hitEnemies.Clear();
        isDamaging = true;
    }

    public void DisableDamage()
    {
        hitEnemies.Clear();
    }
}
