using NewBananaWeapons;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

public class KeyboardWeapon : MonoBehaviour
{
    public GameObject projec;
    public TMP_Text writing;
    StringBuilder builder = new StringBuilder();

    static readonly HashSet<string> adjectiveDictionary = new HashSet<string>()
    {
        "fast",
        "powerful",
        "slow",
        "big",
        "giant",
        "massive",
        "explosive",
        "piercing",
        "homing",
        "tiny",
        "heavy",
        "chaotic",
        "vampiric",
        "bouncing",
        "bouncy",
        "splitting",
        "growing",
        "shrinking",
        "spinning",
        "phasing",
        "ghostly",
        "chaining",
        "freezing",
        "frozen",
        "icy",
        "burning",
        "flaming",
        "fiery",
        "gravity",
        "reversing",
        "pulsating",
        "pulsing",
        "draining",
        "leeching",
        "multiplying",
        "cloning",
        "unstable",
        "ethereal",
        "toxic",
        "poison",
        "ancient",
        "swift",
        "divine",
        "cursed",
        "blessed",
        "wild"
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
        TimeController.Instance.timeScaleModifier = 0.25f;
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
            StyleHUD.Instance.AddPoints(100, "<color=green>CAPITALIZATION</color>", gameObject);

        if (punctuation)
            StyleHUD.Instance.AddPoints(200, "<color=#00ffffff>PUNCTUATION</color>", gameObject);

        if (capital && punctuation)
            StyleHUD.Instance.AddPoints(300, "<color=yellow>FULL SENTENCE</color>", gameObject);

        int sentenceCount = sentence.Count(c => c == '.' || c == '!' || c == '?');
        if (sentenceCount > 1)
            StyleHUD.Instance.AddPoints(sentenceCount * 150, "<color=orange>MULTI SENTENCE</color>", gameObject);

    }


    public void FireWord(string word, List<string> adjectives, float varietyBonus, float sentenceMultiplier)
    {
        GameObject projectile = Instantiate(projec,
        CameraController.Instance.transform.position,
        CameraController.Instance.transform.rotation);

        WordProjectile wordProj = projectile.GetComponent<WordProjectile>();
        wordProj.Word = word;
        wordProj.Adjectives.AddRange(adjectives);
        wordProj.VarietyMultiplier = varietyBonus * 0.5f;
        wordProj.SentenceMultiplier = sentenceMultiplier;

    }
    bool IsAdjective(string word)
    {
        return adjectiveDictionary.Contains(word.ToLower());
    }

    IEnumerator fireProjectile(string[] words)
    {
        HashSet<string> pendingAdjectives = new HashSet<string>();
        float sentenceMultiplier = Mathf.Clamp(words.Length * 0.1f, 0f, 1f);
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


            yield return new WaitForSeconds(0.1f);
        }

        if (pendingAdjectives.Count > 0)
        {
            foreach (var adj in pendingAdjectives)
            {
                FireWord(adj, new List<string>(), varietyBonus, sentenceMultiplier);
                yield return new WaitForSeconds(0.1f);
            }
        }

    }

}
