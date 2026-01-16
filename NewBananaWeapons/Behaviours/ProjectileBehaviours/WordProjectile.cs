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

    private bool isBouncing;
    private int bounceCount = 0;
    private int maxBounces = 3;
    private bool isSplitting;
    private bool isGrowing;
    private bool isShrinking;
    private bool isSpinning;
    private bool isPhasing;
    private float phaseTimer = 0f;
    private bool isChaining;
    private int chainCount = 0;
    private int maxChains = 2;
    private bool isGravity;
    private Vector3 gravityVelocity = Vector3.zero;
    private bool isReversing;
    private float reverseTimer = 0f;
    private Color originalColor;
    private bool isPulsating;
    private float pulseTimer = 0f;
    private bool isDraining;
    private bool isMultiplying;
    private float multiplyTimer = 0f;
    private int multiplyCount = 0;
    private int maxMultiplies = 2;
    private float lifetime = 0f;
    private float maxLifetime = 10f;

    private void Start()
    {
        StringBuilder fullWord = new StringBuilder();
        foreach (var adj in Adjectives)
        {
            fullWord.Append(adj + " ");
        }
        fullWord.Append(Word);
        GetComponent<TMP_Text>().text = fullWord.ToString();

        originalColor = GetComponent<TMP_Text>().color;

        CheckAdjectives();
        CheckStartWords();
        if (isHoming)
        {
            List<EnemyIdentifier> enemies = EnemyTracker.Instance.GetCurrentEnemies();
            if (enemies.Count > 0)
                target = enemies[Random.Range(0, enemies.Count)];
        }
    }

    void Update()
    {
        lifetime += Time.deltaTime;
        if (lifetime > maxLifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (target != null && !target.dead)
        {
            transform.LookAt(target.transform.position, Vector3.up);
        }
        else if (isHoming)
        {
            List<EnemyIdentifier> enemies = EnemyTracker.Instance.GetCurrentEnemies();
            if (enemies.Count > 0)
                target = enemies[Random.Range(0, enemies.Count)];
        }

        if (isGrowing)
        {
            transform.localScale += Vector3.one * Time.deltaTime * 0.5f;
            projectileDamage += Time.deltaTime * 0.5f;
        }

        if (isShrinking)
        {
            transform.localScale -= Vector3.one * Time.deltaTime * 0.3f;
            projectileSpeed += Time.deltaTime * 10f;
            if (transform.localScale.x <= 0.1f)
                Destroy(gameObject);
        }

        if (isSpinning)
        {
            transform.Rotate(Vector3.up * Time.deltaTime * 360f);
        }

        if (isPhasing)
        {
            phaseTimer += Time.deltaTime;
            if (phaseTimer > 0.3f)
            {
                GetComponent<Collider>().enabled = !GetComponent<Collider>().enabled;
                TMP_Text text = GetComponent<TMP_Text>();
                Color c = text.color;
                c.a = GetComponent<Collider>().enabled ? 1f : 0.4f;
                text.color = c;
                phaseTimer = 0f;
            }
        }

        if (isPulsating)
        {
            pulseTimer += Time.deltaTime * 5f;
            float scale = 1f + Mathf.Sin(pulseTimer) * 0.3f;
            transform.localScale = Vector3.one * scale;
        }

        if (isReversing)
        {
            reverseTimer += Time.deltaTime;
            if (reverseTimer > 1f)
            {
                projectileSpeed *= -1;
                reverseTimer = 0f;
            }
        }

        if (isGravity)
        {
            gravityVelocity.y -= Time.deltaTime * 15f;
            Vector3 forwardVelocity = transform.forward * projectileSpeed;
            transform.position += (forwardVelocity + gravityVelocity) * Time.deltaTime;
        }
        else
        {
            transform.position += transform.forward * projectileSpeed * Time.deltaTime;
        }

        if (isMultiplying && multiplyCount < maxMultiplies)
        {
            multiplyTimer += Time.deltaTime;
            if (multiplyTimer > 1f)
            {
                GameObject clone = Instantiate(gameObject, transform.position + Random.insideUnitSphere * 2f, transform.rotation);
                WordProjectile wp = clone.GetComponent<WordProjectile>();
                wp.Adjectives.Clear();
                wp.isMultiplying = false;
                wp.projectileDamage *= 0.4f;
                wp.maxLifetime = 3f;
                multiplyCount++;
                multiplyTimer = 0f;
            }
        }
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
                case "tiny":
                    transform.localScale *= 0.6f;
                    projectileSpeed *= 1.4f;
                    projectileDamage *= 0.8f;
                    break;
                case "heavy":
                    projectileSpeed *= 0.6f;
                    projectileDamage *= 1.8f;
                    break;
                case "chaotic":
                    projectileDamage *= Random.Range(0.5f, 2.5f);
                    projectileSpeed *= Random.Range(0.7f, 1.5f);
                    break;
                case "vampiric":
                    healsPlayer = true;
                    break;

                case "bouncing":
                case "bouncy":
                    isBouncing = true;
                    break;
                case "splitting":
                    isSplitting = true;
                    break;
                case "growing":
                    isGrowing = true;
                    break;
                case "shrinking":
                    isShrinking = true;
                    break;
                case "spinning":
                    isSpinning = true;
                    projectileDamage *= 1.3f;
                    break;
                case "phasing":
                case "ghostly":
                    isPiercing = true;
                    break;
                case "chaining":
                    isChaining = true;
                    maxChains = 2;
                    break;
                case "freezing":
                case "frozen":
                case "icy":
                    GetComponent<TMP_Text>().color = Color.cyan;
                    break;
                case "burning":
                case "flaming":
                case "fiery":
                    GetComponent<TMP_Text>().color = Color.red;
                    break;
                case "gravity":
                    isGravity = true;
                    break;
                case "reversing":
                    isReversing = true;
                    break;
                case "pulsating":
                case "pulsing":
                    isPulsating = true;
                    break;
                case "draining":
                case "leeching":
                    isDraining = true;
                    healsPlayer = true;
                    projectileDamage *= 0.8f;
                    break;
                case "multiplying":
                case "cloning":
                    isMultiplying = true;
                    projectileDamage *= 0.7f;
                    maxMultiplies = 2;
                    break;
                case "unstable":
                    projectileSpeed *= Random.Range(0.3f, 3f);
                    transform.localScale *= Random.Range(0.5f, 2f);
                    break;
                case "ethereal":
                    GetComponent<TMP_Text>().color = new Color(0.7f, 0.5f, 1f);
                    projectileDamage *= 1.4f;
                    break;
                case "toxic":
                case "poison":
                    GetComponent<TMP_Text>().color = Color.green;
                    projectileDamage *= 1.5f;
                    break;
                case "ancient":
                    projectileDamage *= 2f;
                    projectileSpeed *= 0.7f;
                    transform.localScale *= 1.3f;
                    break;
                case "swift":
                    projectileSpeed *= 2.5f;
                    projectileDamage *= 0.9f;
                    break;
                case "divine":
                    projectileDamage *= 1.8f;
                    GetComponent<TMP_Text>().color = Color.yellow;
                    isHoming = true;
                    break;
                case "cursed":
                    projectileDamage *= 2.2f;
                    GetComponent<TMP_Text>().color = new Color(0.5f, 0f, 0.5f);
                    break;
                case "blessed":
                    healsPlayer = true;
                    projectileDamage *= 1.3f;
                    break;
                case "wild":
                    transform.Rotate(Random.insideUnitSphere * 45f);
                    projectileDamage *= Random.Range(0.8f, 2f);
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
            case "explode":
            case "boom":
                isExplosive = true;
                break;
            case "pierce":
                isPiercing = true;
                break;
            case "seek":
                isHoming = true;
                break;
            case "burst":
                projectileSpeed *= 2.5f;
                projectileDamage *= 0.7f;
                break;
            case "slow":
                projectileSpeed *= 0.4f;
                projectileDamage *= 2f;
                break;
            case "storm":
                projectileSpeed *= 2f;
                isPiercing = true;
                break;
            case "nova":
                isExplosive = true;
                projectileDamage *= 2f;
                transform.localScale *= 1.5f;
                break;
            case "gift":
                healsPlayer = true;
                projectileDamage = 0;
                break;
            case "curse":
                projectileDamage *= 2.5f;
                VarietyMultiplier -= 0.5f;
                break;
            case "wall":
                projectileSpeed *= 0.2f;
                projectileDamage *= 3f;
                transform.localScale *= 3f;
                break;
            case "laser":
                projectileSpeed *= 6f;
                projectileDamage *= 1.5f;
                isPiercing = true;
                break;
            case "mine":
                projectileSpeed = 0;
                isExplosive = true;
                break;

            case "bounce":
                isBouncing = true;
                maxBounces = 5;
                break;
            case "split":
            case "fork":
                isSplitting = true;
                break;
            case "chain":
            case "lightning":
                isChaining = true;
                maxChains = 3;
                projectileDamage *= 1.3f;
                break;
            case "freeze":
            case "frost":
                projectileSpeed *= 0.7f;
                projectileDamage *= 1.2f;
                break;
            case "burn":
            case "flame":
                projectileDamage *= 1.4f;
                break;
            case "meteor":
                isGravity = true;
                isExplosive = true;
                projectileDamage *= 2.5f;
                transform.localScale *= 2f;
                projectileSpeed *= 1.5f;
                break;
            case "vortex":
            case "spiral":
                isSpinning = true;
                isHoming = true;
                projectileDamage *= 1.6f;
                break;
            case "phantom":
            case "ghost":
                isPiercing = true;
                projectileSpeed *= 1.5f;
                break;
            case "multiply":
            case "swarm":
                isMultiplying = true;
                projectileDamage *= 0.6f;
                maxMultiplies = 3;
                break;
            case "pulse":
            case "beat":
                isPulsating = true;
                projectileDamage *= 1.3f;
                break;
            case "drain":
            case "siphon":
                isDraining = true;
                healsPlayer = true;
                projectileDamage *= 1.2f;
                break;
            case "reverse":
            case "return":
                isReversing = true;
                break;
            case "shatter":
                isSplitting = true;
                isExplosive = true;
                projectileDamage *= 1.5f;
                break;
            case "wrath":
            case "rage":
                projectileDamage *= 3f;
                projectileSpeed *= 1.5f;
                GetComponent<TMP_Text>().color = Color.red;
                break;
            case "peace":
                projectileSpeed *= 0.5f;
                healsPlayer = true;
                break;
            case "chaos":
                projectileDamage *= Random.Range(0.5f, 4f);
                projectileSpeed *= Random.Range(0.5f, 3f);
                transform.localScale *= Random.Range(0.5f, 2.5f);
                break;
            case "titan":
                transform.localScale *= 3f;
                projectileDamage *= 3.5f;
                projectileSpeed *= 0.4f;
                break;
            case "void":
                GetComponent<TMP_Text>().color = Color.black;
                projectileDamage *= 2f;
                isPiercing = true;
                break;
            case "star":
            case "sun":
                GetComponent<TMP_Text>().color = Color.yellow;
                isExplosive = true;
                projectileDamage *= 2.2f;
                break;
            case "moon":
                GetComponent<TMP_Text>().color = Color.white;
                isHoming = true;
                projectileDamage *= 1.8f;
                break;
            case "shadow":
                GetComponent<TMP_Text>().color = new Color(0.2f, 0.2f, 0.2f);
                projectileSpeed *= 2f;
                isPiercing = true;
                break;
            case "time":
                projectileSpeed *= 0.3f;
                projectileDamage *= 2.5f;
                break;
            case "space":
                isPiercing = true;
                projectileDamage *= 1.8f;
                break;
            case "reality":
                projectileDamage *= 3.5f;
                transform.localScale *= 2f;
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

        if (isBouncing && ((mask.value & (1 << layer)) != 0) && bounceCount < maxBounces)
        {
            Vector3 reflection = Vector3.Reflect(transform.forward, other.transform.up);
            transform.forward = reflection;
            bounceCount++;
            projectileDamage *= 1.1f;
            return;
        }

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

            if (isSplitting && !isPiercing)
            {
                for (int i = 0; i < 2; i++) 
                {
                    GameObject split = Instantiate(gameObject, transform.position, Quaternion.Euler(Random.insideUnitSphere * 30f));
                    WordProjectile wp = split.GetComponent<WordProjectile>();
                    wp.Adjectives.Clear();
                    wp.isSplitting = false;
                    wp.projectileDamage *= 0.4f;
                    wp.projectileSpeed *= 0.8f;
                    wp.transform.localScale *= 0.7f;
                    wp.maxLifetime = 3f;
                }
            }

            if (isChaining && chainCount < maxChains)
            {
                List<EnemyIdentifier> enemies = EnemyTracker.Instance.GetCurrentEnemies();
                EnemyIdentifier closestEnemy = null;
                float closestDist = 15f;

                foreach (var enemy in enemies)
                {
                    if (enemy != eid && enemy != null && !enemy.dead)
                    {
                        float dist = Vector3.Distance(transform.position, enemy.transform.position);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closestEnemy = enemy;
                        }
                    }
                }

                if (closestEnemy != null)
                {
                    GameObject chain = Instantiate(gameObject, transform.position, Quaternion.identity);
                    WordProjectile wp = chain.GetComponent<WordProjectile>();
                    wp.chainCount = chainCount + 1;
                    wp.isHoming = true;
                    wp.target = closestEnemy;
                    wp.projectileDamage *= 0.6f;
                    wp.maxLifetime = 2f;
                }
            }

            if (healsPlayer)
                NewMovement.Instance.GetHealth(Mathf.FloorToInt(2 * damage), false);

            if (isDraining)
                NewMovement.Instance.GetHealth(Mathf.FloorToInt(3 * damage), false);

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


        if (Word.Length > 6 && Word.Length < 9) totalMult += 0.3f;
        else if (Word.Length >= 9 && Word.Length < 13) totalMult += 0.6f;
        else if (Word.Length >= 13) totalMult += 1f;


        string reversedText = new string(Word.Reverse().ToArray());

        if (reversedText.ToLower() == lower)
            totalMult += 1f;

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

        if (lower == "fire") totalMult += 0.6f;
        if (lower == "ice") totalMult += 0.6f;
        if (lower == "shock") totalMult += 0.6f;

        if (lower.Contains("kill")) totalMult += 1.2f;
        if (lower.Contains("love")) totalMult -= 0.3f;

        if (lower == "ultra") totalMult += 2.5f;
        if (lower == "garbage") totalMult -= 0.6f;

        if (lower == "random") totalMult *= Random.Range(0.5f, 2f);
        if (lower == "boss" && eid.isBoss)
            totalMult += 2f;

        if (lower.Contains("blood"))
            totalMult += 0.6f;

        if (lower == "void")
            totalMult += 1.5f;

        if (lower == "light")
            totalMult += 0.4f;

        if (lower == "dark")
            totalMult += 0.6f;

        if (lower == "chaos")
            totalMult *= Random.Range(0.2f, 3f);

        if (lower == "order")
            totalMult += 1f;

        if (lower == "machine" && eid.enemyClass == EnemyClass.Machine)
            totalMult += 1.5f;

        if (lower == "flesh" && eid.enemyClass != EnemyClass.Machine)
            totalMult += 1.2f;

        if (lower.Contains("die"))
            totalMult += 1f;

        if (lower == "zero")
            totalMult *= 0.1f;

        if (lower == "infinite")
            totalMult += 3f;

        if (lower == "death")
            totalMult += 2f;

        if (lower == "destruction")
            totalMult += 1.5f;

        if (lower == "annihilation")
            totalMult += 3f;

        if (lower == "oblivion")
            totalMult += 2.5f;

        if (lower == "doom")
            totalMult += 1.8f;

        if (lower == "justice")
            totalMult += 1.3f;

        if (lower == "vengeance")
            totalMult += 1.6f;

        if (lower.Contains("fear"))
            totalMult += 0.8f;

        if (lower.Contains("pain"))
            totalMult += 0.9f;

        if (lower == "hope")
            totalMult += 1.1f;

        if (lower.Contains("hate"))
            totalMult += 1.2f;

        totalMult += SentenceMultiplier;
        totalMult += VarietyMultiplier;

        return Mathf.Max(0.1f, totalMult);
    }

}