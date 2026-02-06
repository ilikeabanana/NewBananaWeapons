using BepInEx.Configuration;
using NewBananaWeapons;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

public class KeyboardWeapon : BaseWeapon
{
    public GameObject projec;
    public TMP_Text writing;
    StringBuilder builder = new StringBuilder();

    // Configurable values
    private ConfigEntry<float> slowMotionMultiplier;
    private ConfigEntry<int> capitalizationPoints;
    private ConfigEntry<int> punctuationPoints;
    private ConfigEntry<int> fullSentencePoints;
    private ConfigEntry<int> multiSentencePointsPerSentence;
    private ConfigEntry<float> wordFireDelay;
    private ConfigEntry<float> varietyBonusMultiplier;
    private ConfigEntry<float> sentenceMultiplierScale;

    public override void SetupConfigs(string sectionName, ConfigFile Config)
    {
        slowMotionMultiplier = Config.Bind<float>(sectionName, "Slow Motion Multiplier", 0.25f,
            "Time scale when typing (0.25 = 25% speed)");

        capitalizationPoints = Config.Bind<int>(sectionName, "Capitalization Points", 100,
            "Style points for capitalizing first letter");

        punctuationPoints = Config.Bind<int>(sectionName, "Punctuation Points", 200,
            "Style points for proper punctuation");

        fullSentencePoints = Config.Bind<int>(sectionName, "Full Sentence Points", 300,
            "Bonus points for both capitalization and punctuation");

        multiSentencePointsPerSentence = Config.Bind<int>(sectionName, "Multi Sentence Points", 150,
            "Points per sentence when typing multiple sentences");

        wordFireDelay = Config.Bind<float>(sectionName, "Word Fire Delay", 0.1f,
            "Delay between firing each word (in seconds)");

        varietyBonusMultiplier = Config.Bind<float>(sectionName, "Variety Bonus Multiplier", 0.5f,
            "Multiplier for word variety bonus");

        sentenceMultiplierScale = Config.Bind<float>(sectionName, "Sentence Multiplier Scale", 0.1f,
            "How much longer sentences increase damage (0.1 = 10% per word)");
    }

    static readonly HashSet<string> adjectiveDictionary = new HashSet<string>()
    {
        "fast", "powerful", "slow", "big", "giant", "massive", "explosive", "piercing", "homing",
        "tiny", "heavy", "chaotic", "vampiric", "bouncing", "bouncy", "splitting", "growing",
        "shrinking", "spinning", "phasing", "ghostly", "chaining", "freezing", "frozen", "icy",
        "burning", "flaming", "fiery", "gravity", "reversing", "pulsating", "pulsing", "draining",
        "leeching", "multiplying", "cloning", "unstable", "ethereal", "toxic", "poison", "ancient",
        "swift", "divine", "cursed", "blessed", "wild"
    };

    void OnDisable()
    {
        EnableMovement();
        builder.Clear();
    }

    void DisableMovement()
    {
        NewMovement.Instance.enabled = false;
        GunControl.Instance.enabled = false;
        PlayerUtilities.Instance.NoFist();
        NewMovement.Instance.movementDirection = Vector3.zero;
        NewMovement.Instance.movementDirection2 = Vector3.zero;
        TimeController.Instance.timeScaleModifier = slowMotionMultiplier.Value;
        TimeController.instance.RestoreTime();
    }

    void EnableMovement()
    {
        NewMovement.Instance.enabled = true;
        GunControl.Instance.enabled = true;
        PlayerUtilities.Instance.YesFist();
        TimeController.Instance.timeScaleModifier = 1;
        TimeController.instance.RestoreTime();
    }
    bool Typing = false;

    void Update()
    {
        if (!GunControl.Instance.activated) return;
        if (InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame)
        {
            DisableMovement();
            Typing = true;
        }
        if (!Typing) return;

        string input = Input.inputString;

        if (!string.IsNullOrEmpty(input))
        {
            foreach (char c in input)
            {
                if (c == '\b')
                {
                    if (builder.Length > 0)
                        builder.Length--;
                }
                else if (c == '\n' || c == '\r')
                {
                    HudMessageReceiver.Instance.SendHudMessage(builder.ToString());
                    Banana_WeaponsPlugin.Log.LogInfo(builder.ToString());
                    CheckSpelling(builder.ToString());
                    builder.Replace(".", "");
                    builder.Replace(",", "");
                    builder.Replace("?", "");
                    builder.Replace("!", "");
                    builder.Replace(";", "");
                    string[] batchstring = builder.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    StartCoroutine(fireProjectile(batchstring));
                    builder.Clear();
                    EnableMovement();
                    Typing = false;
                }
                else
                {
                    builder.Append(c);
                }
            }
        }
        writing.text = builder.ToString() + "<color=#808080>Type to enter text, and hit Enter to fire...";
    }

    void CheckSpelling(string sentence)
    {
        if (string.IsNullOrWhiteSpace(sentence))
            return;

        bool capital = char.IsUpper(sentence.TrimStart()[0]);
        bool punctuation = char.IsPunctuation(sentence.Trim().Last());

        if (capital)
            StyleHUD.Instance.AddPoints(capitalizationPoints.Value, "<color=green>CAPITALIZATION</color>", gameObject);

        if (punctuation)
            StyleHUD.Instance.AddPoints(punctuationPoints.Value, "<color=#00ffffff>PUNCTUATION</color>", gameObject);

        if (capital && punctuation)
            StyleHUD.Instance.AddPoints(fullSentencePoints.Value, "<color=yellow>FULL SENTENCE</color>", gameObject);

        int sentenceCount = sentence.Count(c => c == '.' || c == '!' || c == '?');
        if (sentenceCount > 1)
            StyleHUD.Instance.AddPoints(sentenceCount * multiSentencePointsPerSentence.Value, "<color=orange>MULTI SENTENCE</color>", gameObject);
    }

    public void FireWord(string word, List<string> adjectives, float varietyBonus, float sentenceMultiplier)
    {
        GameObject projectile = Instantiate(projec,
        CameraController.Instance.transform.position,
        CameraController.Instance.transform.rotation);

        WordProjectile wordProj = projectile.GetComponent<WordProjectile>();
        wordProj.Word = word;
        wordProj.Adjectives.AddRange(adjectives);
        wordProj.VarietyMultiplier = varietyBonus * varietyBonusMultiplier.Value;
        wordProj.SentenceMultiplier = sentenceMultiplier;
    }

    bool IsAdjective(string word)
    {
        return adjectiveDictionary.Contains(word.ToLower());
    }

    IEnumerator fireProjectile(string[] words)
    {
        HashSet<string> pendingAdjectives = new HashSet<string>();
        float sentenceMultiplier = Mathf.Clamp(words.Length * sentenceMultiplierScale.Value, 0f, 1f);
        int uniqueWords = words.Distinct().Count();
        float varietyBonus = (float)uniqueWords / words.Length;

        foreach (var raw in words)
        {
            string word = raw.ToLowerInvariant();

            if (IsAdjective(word))
            {
                pendingAdjectives.Add(word);
                continue;
            }

            FireWord(word, pendingAdjectives.ToList(), varietyBonus, sentenceMultiplier);
            pendingAdjectives.Clear();

            yield return new WaitForSeconds(wordFireDelay.Value);
        }

        if (pendingAdjectives.Count > 0)
        {
            foreach (var adj in pendingAdjectives)
            {
                FireWord(adj, new List<string>(), varietyBonus, sentenceMultiplier);
                yield return new WaitForSeconds(wordFireDelay.Value);
            }
        }
    }
}