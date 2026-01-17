using System.Collections;
using UnityEngine;
using NewBananaWeapons;
using HarmonyLib;


public class PipeProjectile : MonoBehaviour
{
    public GameObject wallHitExplosion;
    public Transform visual;

    [HideInInspector] public float speed = 60;
    [HideInInspector] public float damage = 4.5f;
    [HideInInspector] public int timesParried = 1;

    [HideInInspector] public float timerWhereItHasToReturn = 5f;

    float rotationSpeed = 20;

    [HideInInspector] public bool goingBackToPlayer = false;

    void Awake()
    {
        StartCoroutine(ShaderManager.ApplyShaderToGameObject(wallHitExplosion));
        foreach (var expl in wallHitExplosion.GetComponentsInChildren<Explosion>(true))
        {
            StartCoroutine(ShaderManager.ApplyShaderToGameObject(expl.explosionChunk));
        }
        Calculate();
    }

    public void Calculate()
    {
        speed = 80f - (80 - speed / (float)timesParried);
        damage = 10 - (10 - damage / (float)timesParried);
    }

    private void Update()
    {
        if (!goingBackToPlayer)
            timerWhereItHasToReturn -= Time.deltaTime;
        if(timerWhereItHasToReturn <= 0 && !goingBackToPlayer)
        {
            goingBackToPlayer = true;
        }
        if (goingBackToPlayer)
        {
            transform.LookAt(CameraController.Instance.transform, Vector3.up);
        }
        visual.Rotate(Vector3.one * rotationSpeed * Time.deltaTime);
        transform.position += transform.forward * speed * Time.deltaTime;

        if(Vector3.Distance(transform.position, CameraController.Instance.transform.position) <= 0.25f && goingBackToPlayer)
        {
            StartCoroutine(waitBeforeDestruction());
        }
    }

    IEnumerator waitBeforeDestruction()
    {
        yield return new WaitForSeconds(0.35f);
        if (goingBackToPlayer)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (goingBackToPlayer) return;
        int layer = other.gameObject.layer;
        LayerMask mask = LayerMaskDefaults.Get(LMD.EnemiesAndEnvironment);
        if (((mask.value & (1 << layer)) != 0))
        {
            goingBackToPlayer = true;
            if(other.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier eidd))
            {
                eidd.eid.hitter = "Pipe";
                eidd.eid.SimpleDamage(damage * 2);
            }

            if(((LayerMaskDefaults.Get(LMD.Environment).value & (1 << layer)) != 0))
            {
                GameObject explo = Instantiate(wallHitExplosion, transform.position, Quaternion.identity);
                foreach (var ex in explo.GetComponentsInChildren<Explosion>())
                {
                    ex.playerDamageOverride = (int)damage / 2;
                    ex.enemyDamageMultiplier = damage;
                }
            }
        }
    }
}

[HarmonyPatch(typeof(Punch), nameof(Punch.TryParryProjectile))]
public static class PunchPipe
{
    [HarmonyPrefix]
    public static void Prefix(Punch __instance, Transform target, bool canProjectileBoost = false)
    {
        Banana_WeaponsPlugin.Log.LogInfo(target.gameObject.name + " is being checked");
        if(target.gameObject.TryGetComponent<PipeProjectile>(out PipeProjectile pipe))
        {
            if (pipe.goingBackToPlayer == false) return;
            pipe.goingBackToPlayer = false;
            pipe.timesParried++;
            pipe.transform.forward = CameraController.Instance.transform.forward;
            pipe.Calculate();
            pipe.timerWhereItHasToReturn = 5;

            MonoSingleton<TimeController>.Instance.ParryFlash();
            __instance.anim.Play("Hook", 0, 0.065f);
        }
    }
}