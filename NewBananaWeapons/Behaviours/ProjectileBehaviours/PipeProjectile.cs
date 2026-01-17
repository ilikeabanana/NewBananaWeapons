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
        speed = 80f - (20f / (float)timesParried);
        damage = 10 - (5.5f / (float)timesParried);
    }

    private void Update()
    {
        if (goingBackToPlayer)
        {
            transform.LookAt(NewMovement.Instance.transform, Vector3.up);
        }
        visual.Rotate(Vector3.one * rotationSpeed * Time.deltaTime);
        transform.position += transform.forward * speed * Time.deltaTime;

        if(Vector3.Distance(transform.position, NewMovement.Instance.transform.position) <= 0.1f)
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
                eidd.eid.SimpleDamage(damage);
            }

            if(((LayerMaskDefaults.Get(LMD.Environment).value & (1 << layer)) != 0))
            {
                Instantiate(wallHitExplosion, transform.position, Quaternion.identity);
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
            pipe.goingBackToPlayer = false;
            pipe.timesParried++;
            pipe.Calculate();

            MonoSingleton<TimeController>.Instance.ParryFlash();
            __instance.anim.Play("Hook", 0, 0.065f);
        }
    }
}