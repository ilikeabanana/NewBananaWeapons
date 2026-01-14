using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;


public class WordProjectile : MonoBehaviour
{
    public string Word;
    public float projectileSpeed = 10;

    private void Start()
    {
        GetComponent<TMP_Text>().text = Word;
    }

    void Update()
    {
        transform.position += transform.forward * projectileSpeed * Time.deltaTime;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier eidd))
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
        float totalMult = 1;

        if (Word.ToLower() == eid.enemyClass.ToString().ToLower()) totalMult += 1.5f;
        if (Word.ToLower() == "big" && eid.bigEnemy) totalMult += 0.5f;
        if (Word.ToLower() == "small" && !eid.bigEnemy) totalMult += 0.5f;
        if (Word.ToLower() == "tiny" && !eid.bigEnemy) totalMult += 0.5f;
        if (Word.ToLower() == "flesh" && eid.enemyClass != EnemyClass.Machine) totalMult += 0.5f;
        if (Word.ToLower() == "metal" && eid.enemyClass == EnemyClass.Machine) totalMult += 1;

        if (Word.Length > 6 && Word.Length < 9) totalMult += 0.3f;
        else if (Word.Length > 9 && Word.Length < 13) totalMult += 0.6f;
        else totalMult += 1f;

        string reversedText = Word.Reverse().ToString();

        if (reversedText.ToLower() == reversedText.ToLower()) totalMult += 1;

        switch (Word.ToLower())
        {
            case "weak":
            case "pathetic": 
            case "worthless": 
            case "fuck": 
            case "useless": 
                totalMult += 0.7f; 
                break;
        }

        return totalMult;
    }

}
