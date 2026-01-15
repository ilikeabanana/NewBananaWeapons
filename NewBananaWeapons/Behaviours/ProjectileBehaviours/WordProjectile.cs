using NewBananaWeapons;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;


public class WordProjectile : MonoBehaviour
{
    public string Word;
    public List<string> Adjectives = new List<string>();
    public float projectileSpeed = 50;
    public bool isExplosive;
    public bool healsPlayer;
    public bool isPiercing;
    public bool isHoming;
    public float SentenceMultiplier = 0f;
    public float VarietyMultiplier = 0f;


    float projectileDamage = 2.5f;

    EnemyIdentifier target;

    private void Start()
    {
        StringBuilder fullWord = new StringBuilder();
        foreach (var adj in Adjectives)
        {
            fullWord.Append(adj + " ");
        }
        fullWord.Append(Word);
        GetComponent<TMP_Text>().text = fullWord.ToString();
        
        CheckAdjectives();
        CheckStartWords();
        if (isHoming)
        {
            List<EnemyIdentifier> enemies = EnemyTracker.Instance.GetCurrentEnemies();

            target = enemies[Random.Range(0, enemies.Count)];
        }
    }

    void Update()
    {
        if (target != null)
        {
            transform.LookAt(target.transform.position, Vector3.up);
        }
        transform.position += transform.forward * projectileSpeed * Time.deltaTime;
        
        
    }

    void CheckAdjectives()
    {
        foreach (var adjective in Adjectives)
        {
            switch (adjective.ToLower())
            {
                case "fast":
                    projectileSpeed *= 2;
                    break;
                case "powerful":
                    projectileDamage *= 1.2f;
                    break;
                case "slow":
                    projectileSpeed /= 2;
                    break;
                case "big":
                    transform.localScale *= 1.2f;
                    break;
                case "giant":
                    transform.localScale *= 1.6f;
                    break;
                case "massive":
                    transform.localScale *= 2f;
                    break;
                case "explosive":
                    isExplosive = true;
                    break;
                case "piercing":
                    isPiercing = true;
                    break;
                case "homing":
                    isHoming = true;
                    break;

            }
        }
    }

    void CheckStartWords()
    {
        switch (Word.ToLower())
        {
            case "heal":
            case "life":
            case "mercy":
                NewMovement.Instance.GetHealth(Mathf.FloorToInt(10 * projectileDamage), false);
                Destroy(gameObject);
                break;
        }

        if (Word.Length <= 3)
            projectileSpeed *= 1.3f;
    }

    List<GameObject> hitObjects = new List<GameObject>();

    void OnTriggerEnter(Collider other)
    {
        int layer = other.gameObject.layer;
        LayerMask mask = LayerMaskDefaults.Get(LMD.EnemiesAndEnvironment);

        if (isExplosive && ((mask.value & (1 << layer)) != 0) && !hitObjects.Contains(other.gameObject))
        {
           
            Instantiate(AddressableManager.explosion, transform.position, Quaternion.identity);
            if (!isPiercing)
                Destroy(gameObject);
            return;
        }

        if (other.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier eidd))
        {
            EnemyIdentifier eid = eidd.eid;

            eid.hitter = Word;
            float damage = projectileDamage * ApplyMultipliers(eid);

            eid.DeliverDamage(other.gameObject, Vector3.zero, other.transform.position, damage, false);

            if (!isPiercing)
                Destroy(gameObject);
        }
        hitObjects.Add(other.gameObject);
    }

    float ApplyMultipliers(EnemyIdentifier eid)
    {
        float totalMult = 1f;
        string lower = Word.ToLower();

        if (lower == eid.enemyClass.ToString().ToLower()) totalMult += 1.5f;
        if (lower == "big" && eid.bigEnemy) totalMult += 0.5f;
        if (lower == "small" && !eid.bigEnemy) totalMult += 0.5f;
        if (lower == "tiny" && !eid.bigEnemy) totalMult += 0.5f;
        if (lower == "flesh" && eid.enemyClass != EnemyClass.Machine) totalMult += 0.5f;
        if (lower == "metal" && eid.enemyClass == EnemyClass.Machine) totalMult += 1f;

        // Length bonus
        if (Word.Length > 6 && Word.Length < 9) totalMult += 0.3f;
        else if (Word.Length >= 9 && Word.Length < 13) totalMult += 0.6f;
        else if (Word.Length >= 13) totalMult += 1f;

        // Proper reversed string
        string reversedText = new string(Word.Reverse().ToArray());

        // Palindrome bonus
        if (reversedText.ToLower() == lower)
            totalMult += 1f;

        // Aggressive words
        switch (lower)
        {
            case "weak":
            case "pathetic":
            case "worthless":
            case "fuck":
            case "useless":
                totalMult += 0.7f;
                break;
        }

        if (lower.Contains("shit") || lower.Contains("damn"))
            totalMult += 0.5f;

        if (lower == "god" || lower == "holy" || lower == "divine")
            totalMult += 2f;

        if (lower == "why" || lower == "what" || lower == "how")
            totalMult -= 0.4f;

        if (lower == "banana")
            totalMult += 3f;

        totalMult += SentenceMultiplier;
        totalMult += VarietyMultiplier;



        return Mathf.Max(0.1f, totalMult);
    }

}
