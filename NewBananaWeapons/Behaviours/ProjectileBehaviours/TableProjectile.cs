using NewBananaWeapons;
using System.Collections;
using UnityEngine;


public class TableProjectile : MonoBehaviour
{
    [HideInInspector] public float damage = 5;
    Rigidbody rb;

    public bool parried = false;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Use this for initialization
    void Start()
    {
        rb.velocity = transform.forward * 19 * PlayerTracker.Instance.GetPlayerVelocity().magnitude;
        transform.Rotate(-90, 0, 0);
        float randX = Random.Range(-100, 100);
        float randY = Random.Range(-100, 100);
        float randZ = Random.Range(-100, 100);
        rb.angularVelocity = new Vector3(randX, randY, randZ);
    }

    private void OnTriggerEnter(Collider other)
    { 

        if (other.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier eidd))
        {
            eidd.eid.hitter = "table";
            eidd.eid.DeliverDamage(other.gameObject,
                CameraController.Instance.transform.forward * 20,
                other.gameObject.transform.position,
                damage, false);

            if (parried)
            {
                Instantiate(AddressableManager.explosion, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
                

        }
    }

}
