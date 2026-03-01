using NewBananaWeapons;
using System.Collections;
using ULTRAKILL.Portal;
using UnityEngine;


public class Rocket : MonoBehaviour
{
    public GameObject explosion;
    float speed = 9f;

    void Start()
    {
        gameObject.AddComponent<SimplePortalTraveler>();
        StartCoroutine(ShaderManager.ApplyShaderToGameObject(explosion));

        foreach (var exp in explosion.GetComponentsInChildren<Explosion>())
        {
            StartCoroutine(ShaderManager.ApplyShaderToGameObject(exp.explosionChunk));
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider hit)
    {
        if(LayerMaskDefaults.IsMatchingLayer(hit.gameObject.layer, LMD.EnemiesAndEnvironment))
        {
            Instantiate(explosion, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
        
    }
}
