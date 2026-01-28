using NewBananaWeapons;
using System.Collections;
using UnityEngine;


public class Paintball : MonoBehaviour
{

    float speed = 150f;
    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider hit)
    {
        if (LayerMaskDefaults.IsMatchingLayer(hit.gameObject.layer, LMD.EnemiesAndEnvironment))
        {
            if(hit.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier eidd))
            {
                eidd.eid.DeliverDamage(hit.gameObject, transform.forward * 10,
                    eidd.eid.transform.position, 12, false);
            }

            for (int i = 0; i < Random.Range(3, 15); i++)
            {
                SpawnStud();
            }
            Destroy(gameObject);
        }

    }

    public void SpawnStud()
    {
        GameObject studObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Vector3 randomSize = Random.insideUnitCircle;
        studObject.transform.localScale = new Vector3(randomSize.x * 2, 1, randomSize.z * 2);
        studObject.GetComponent<Renderer>().material = GetComponent<Renderer>().material;
        Rigidbody rb = studObject.AddComponent<Rigidbody>();
        rb.AddForce(Random.insideUnitSphere * Random.Range(0, 25), ForceMode.Impulse);
        studObject.transform.position = transform.position;
        studObject.AddComponent<RemoveOnTime>().time = Random.Range(0, 3);
    }
}
