using System.Collections;
using UnityEngine;

namespace NewBananaWeapons.Behaviours.ProjectileBehaviours
{
    public class WordProjectile : MonoBehaviour
    {
        public string Word;
        public float projectileSpeed = 10;

        void Update()
        {
            transform.position += transform.forward * projectileSpeed * Time.deltaTime;
        }

        void OnTriggerEnter(Collider other)
        {
            if(other.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier eidd))
            {
                EnemyIdentifier eid = eidd.eid;

                eid.hitter = Word;
                float damage = 2.5f * ApplyMultipliers(eid);
                eid.DeliverDamage(other.gameObject, Vector3.zero, other.transform.position, damage, false);
                Destroy(gameObject);
            }
        }

        float ApplyMultipliers(EnemyIdentifier eid)
        {
            if (Word.ToLower() == eid.enemyClass.ToString().ToLower()) return 2.5f;

            return 1f;
        }
        
    }
}