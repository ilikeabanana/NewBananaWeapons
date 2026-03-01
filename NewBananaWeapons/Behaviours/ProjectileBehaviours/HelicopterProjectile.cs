using NewBananaWeapons;
using System.Collections;
using ULTRAKILL.Portal;
using UnityEngine;


public class HelicopterProjectile : MonoBehaviour
{
    float speed = 270;

    void Awake()
    {
        gameObject.AddComponent<SimplePortalTraveler>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.down * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider col)
    {
        if(LayerMaskDefaults.IsMatchingLayer(col.gameObject.layer, LMD.Environment))
        {
            Instantiate(AddressableManager.rubbleBig, transform.position, Quaternion.identity);
        }

        if (col.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier eidd))
        {
            eidd.eid.DeliverDamage(col.gameObject, transform.forward * 10,
                eidd.eid.transform.position, CharlesWeapon.helicopterDamage.Value, false);
        }

        if(col.gameObject.TryGetComponent<NewMovement>(out NewMovement nm))
        {
            nm.GetHurt(99999, false, ignoreInvincibility: true);
        }
    }
}
