using System.Collections;
using UnityEngine;
using NewBananaWeapons;
using HarmonyLib;


public class PipeProjectile : MonoBehaviour
{
    public GameObject wallHitExplosion;
    public Transform visual;

    [HideInInspector] public float speed = 60;
    float defaultSpeed = 60;
    float maxSpeed = 80;
    [HideInInspector] public float damage = 2.5f;
    float defaultDamage = 4.5f;
    [HideInInspector] public int timesParried = 1;

    [HideInInspector] public float timerWhereItHasToReturn = 5f;

    float rotationSpeed = 20;

    [HideInInspector] public bool goingBackToPlayer = false;
    [HideInInspector] public GameObject sourceWeapon = null;

    void Awake()
    {
        StartCoroutine(ShaderManager.ApplyShaderToGameObject(wallHitExplosion));
        foreach (var expl in wallHitExplosion.GetComponentsInChildren<Explosion>(true))
        {
            StartCoroutine(ShaderManager.ApplyShaderToGameObject(expl.explosionChunk));
        }
        Calculate();
    }

    public float maxDamage = 10f;
    public float rampK = 0.35f;

    public void Calculate()
    {
        damage = maxDamage - (maxDamage - defaultDamage) / (1f + timesParried * rampK);
        speed = maxSpeed - (maxSpeed - defaultSpeed) / (1f + timesParried * rampK);
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

        if(Vector3.Distance(transform.position, CameraController.Instance.transform.position) <= 0.5f && goingBackToPlayer)
        {
            StartCoroutine(waitBeforeDestruction());
        }
    }

    IEnumerator waitBeforeDestruction()
    {
        yield return new WaitForSeconds(0.15f);
        Destroy(gameObject);
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
                if (timesParried == 1)
                    eidd.eid.hitter = "pipe";
                else
                    eidd.eid.hitter = "repipe";
                eidd.eid.DeliverDamage(other.gameObject, 
                    CameraController.Instance.transform.forward * 20,
                    other.gameObject.transform.position,
                    damage * 1.2f, false, sourceWeapon: sourceWeapon);
            }

            if(((LayerMaskDefaults.Get(LMD.Environment).value & (1 << layer)) != 0))
            {
                GameObject explo = Instantiate(wallHitExplosion, transform.position, Quaternion.identity);
                foreach (var ex in explo.GetComponentsInChildren<Explosion>())
                {
                    ex.playerDamageOverride = Mathf.RoundToInt(damage / 1.5f);
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
            pipe.StopAllCoroutines();
            MonoSingleton<TimeController>.Instance.ParryFlash();
            __instance.anim.Play("Hook", 0, 0.065f);
        }
    }
}