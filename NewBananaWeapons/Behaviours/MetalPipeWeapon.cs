using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MetalPipeWeapon : MonoBehaviour
{
    public GameObject metalPipeProjectile;
    public AudioClip slapClip;
    GameObject pipe;
    AudioSource source;
    Animator anim;

    bool isDamaging = false;
    List<EnemyIdentifier> hitEnemies = new List<EnemyIdentifier>();

    private void Awake()
    {
        anim = GetComponent<Animator>();
        source = GetComponent<AudioSource>();
    }

    private void Update()
    {
        anim.SetBool("HoldingLeftClick", InputManager.Instance.InputSource.Fire1.IsPressed);
        anim.SetBool("RightClick", false);
        anim.SetBool("HoldingPipe", pipe == null);
        if (isDamaging)
        {
            RaycastHit hit;
            if (Physics.Raycast(CameraController.Instance.transform.position,
                CameraController.Instance.transform.forward, out hit, 6, LayerMaskDefaults.Get(LMD.Enemies)))
            {
                if (hit.collider.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier enemyHit))
                {
                    if (hitEnemies.Contains(enemyHit.eid)) return;
                    enemyHit.eid.hitter = "Metal";
                    enemyHit.eid.DeliverDamage(hit.collider.gameObject, CameraController.Instance.transform.forward * 20, enemyHit.transform.position, 1, false);
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
    }

    public void EnableDamage()
    {
        RaycastHit hit;
        if (Physics.Raycast(CameraController.Instance.transform.position,
            CameraController.Instance.transform.forward, out hit, 2.5f, LayerMaskDefaults.Get(LMD.Enemies)))
        {
            if (hit.collider.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier enemyHit))
            {
                enemyHit.eid.hitter = "Metal";
                enemyHit.eid.DeliverDamage(hit.collider.gameObject, CameraController.Instance.transform.forward * 20, enemyHit.transform.position, 1, false);
                source.PlayOneShot(slapClip);
            }
        }
    }
}
