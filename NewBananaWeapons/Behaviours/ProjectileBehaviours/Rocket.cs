using NewBananaWeapons;
using System.Collections;
using UnityEngine;


public class Rocket : MonoBehaviour
{
    float speed = 9f;
    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider hit)
    {
        if(LayerMaskDefaults.IsMatchingLayer(hit.gameObject.layer, LMD.EnemiesAndEnvironment))
        {
            Instantiate(AddressableManager.bigExplosion, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
        
    }
}
