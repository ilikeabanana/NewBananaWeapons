using System.Collections;
using UnityEngine;
using NewBananaWeapons;


public class PipeProjectile : MonoBehaviour
{
    public GameObject wallHitExplosion;
    public Transform visual;

    float speed = 20;

    float rotationSpeed = 20;

    bool goingBackToPlayer = false;

    void Awake()
    {
        StartCoroutine(ShaderManager.ApplyShaderToGameObject(wallHitExplosion));
        foreach (var expl in wallHitExplosion.GetComponentsInChildren<Explosion>(true))
        {
            StartCoroutine(ShaderManager.ApplyShaderToGameObject(expl.explosionChunk));
        }
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
            if(((LayerMaskDefaults.Get(LMD.Environment).value & (1 << layer)) != 0))
            {
                Instantiate(wallHitExplosion, transform.position, Quaternion.identity);
            }
        }
    }
}
