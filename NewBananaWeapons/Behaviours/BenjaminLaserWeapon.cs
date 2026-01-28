using NewBananaWeapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BenjaminLaserWeapon : MonoBehaviour
{
    public GameObject chargingLaser;
    public GameObject laser;

    Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
        StartCoroutine(ShaderManager.ApplyShaderToGameObject(laser));
    }
    void Update()
    {
        if (!GunControl.Instance.activated) return;
        if (Banana_WeaponsPlugin.cooldowns.ContainsKey(gameObject))
        {
            chargingLaser.SetActive(false);
            anim.SetBool("Activating", false);
            return;
        }

        chargingLaser.SetActive(true);

        if (InputManager.instance.InputSource.Fire1.WasPerformedThisFrame)
        {
            anim.SetBool("Activating", true);
        }
    }

    public void FireLaser()
    {
        Transform camTrans = CameraController.Instance.transform;
        Instantiate(laser, CameraController.Instance.transform.position, 
            CameraController.Instance.transform.rotation).transform.Rotate(90, 0, 0);
        Banana_WeaponsPlugin.cooldowns.Add(gameObject, 120);

        RaycastHit[] justFuckingHitEverythingTBH = Physics.SphereCastAll(
            camTrans.position, 1317.042f, camTrans.forward, 100000, LayerMaskDefaults.Get(LMD.Enemies));
        List<EnemyIdentifier> alreadyHitEnemies = new List<EnemyIdentifier>();
        foreach (var hit in justFuckingHitEverythingTBH)
        {
            
            if(hit.collider.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier eidd))
            {
                Vector3 toEnemy = (eidd.transform.position - camTrans.position).normalized;
                if (Vector3.Dot(camTrans.forward, toEnemy) <= 0f)
                    continue;


                if (eidd.eid.enemyType == EnemyType.Centaur && SceneHelper.CurrentScene == "Level 7-4")
                {
                    foreach (var eid in EnemyTracker.Instance.GetCurrentEnemies())
                    {
                        eid.InstaKill();
                    }
                    break;
                }
                if (alreadyHitEnemies.Contains(eidd.eid)) continue;

                eidd.eid.hitter = "BenjaminBeam";
                eidd.eid.DeliverDamage(hit.collider.gameObject, camTrans.forward * int.MaxValue, hit.point,
                    125, true);
                alreadyHitEnemies.Add(eidd.eid);

            }
        }
    }
}
