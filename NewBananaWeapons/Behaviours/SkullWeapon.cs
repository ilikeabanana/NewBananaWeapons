using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SkullWeapon : MonoBehaviour
{
    public GameObject skullProjectile;
    public GameObject skeletonThatAppearsWhenRightClicked;
    public GameObject eyesEmoji;

    Animator anim;

    // Use this for initialization
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void Throw()
    {
        GameObject proj = Instantiate(skullProjectile, CameraController.Instance.transform.position, CameraController.Instance.transform.rotation);
    
        if(proj.TryGetComponent<Projectile>(out Projectile projectile))
        {
            projectile.playerBullet = true;
            projectile.friendly = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!GunControl.Instance.activated) return;
        anim.SetBool("Holding", MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed);

        if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame)
        {
            Holyshitaskeleton();
        }
    }

    public void Holyshitaskeleton()
    {
        int skullsAmount = 0;
        foreach (var skull in FindObjectsByType<Projectile>(FindObjectsSortMode.None))
        {
            if (!skull.name.Contains("ActualProjectule")) continue;


            GameObject runn = Instantiate(skeletonThatAppearsWhenRightClicked, skull.transform.position, Quaternion.identity);
            runn.transform.LookAt(NewMovement.Instance.transform);

            if (runn.TryGetComponent<MoveTowards>(out MoveTowards move))
            {
                move.ChangeTarget(-runn.transform.right * 1000);
            }
            Destroy(skull.gameObject);
            skullsAmount++;
        }
        if (skullsAmount <= 0) return;
        List<EnemyIdentifier> currentEnemies = EnemyTracker.Instance.GetCurrentEnemies();

        foreach (var enemy in currentEnemies)
        {
            bool ignore = enemy.ignorePlayer;
            bool attack = enemy.attackEnemies;
            enemy.attackEnemies = false;
            enemy.ignorePlayer = true;
             
            Transform weakpoint = enemy.transform;
            if (enemy.weakPoint != null)
                weakpoint = enemy.weakPoint.transform;

            GameObject instEyes = Instantiate(eyesEmoji, weakpoint);
            instEyes.transform.localPosition = Vector3.zero;
            SetGlobalScale(instEyes.transform, Vector3.one);

            instEyes.GetComponent<SpriteRenderer>().rendererPriority = 99999;
            instEyes.GetComponent<SpriteRenderer>().material.renderQueue = 40000;

            StartCoroutine(delayedReset(enemy, attack, ignore, instEyes));
        }

        
    }
    public static void SetGlobalScale(Transform theTransform, Vector3 globalScale)
    {
        theTransform.localScale = Vector3.one;
        theTransform.localScale = new Vector3(globalScale.x / theTransform.lossyScale.x, globalScale.y / theTransform.lossyScale.y, globalScale.z / theTransform.lossyScale.z);
    }

    IEnumerator delayedReset(EnemyIdentifier enemy, bool attack, bool ignore, GameObject eyes)
    {
        yield return new WaitForSeconds(3);
        enemy.ignorePlayer = ignore;
        enemy.attackEnemies = attack;
        Destroy(eyes);
    }
}
