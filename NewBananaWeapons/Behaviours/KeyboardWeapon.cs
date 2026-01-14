using NewBananaWeapons;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using TMPro;

public class KeyboardWeapon : MonoBehaviour
{
    public GameObject projec;
    public TMP_Text writing;
    StringBuilder builder = new StringBuilder();
    void OnDisable()
    {
        EnableMovement();
        builder.Clear();
    }

    void DisableMovement()
    {
        NewMovement.Instance.enabled = false;
        GunControl.Instance.enabled = false;
        FistControl.Instance.enabled = false;

        NewMovement.Instance.movementDirection = Vector3.zero;
        NewMovement.Instance.movementDirection2 = Vector3.zero;
    }

    void EnableMovement()
    {
        NewMovement.Instance.enabled = true;
        GunControl.Instance.enabled = true;
        FistControl.Instance.enabled = true;
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

        // Capital letter
        if (char.IsUpper(sentence[0]))
            StyleHUD.Instance.AddPoints(100, "<color=green>CAPITALIZATION</color>");

        // Ending punctuation
        char last = sentence.Trim().Last();
        if (char.IsPunctuation(last))
            StyleHUD.Instance.AddPoints(200, "<color=#00ffffff>PUNCTUATION</color>");
    }


    public void FireWord(string word)
    {
        GameObject projectile = Instantiate(projec, CameraController.Instance.transform.position, CameraController.Instance.transform.rotation);
        WordProjectile wordProj = projectile.GetComponent<WordProjectile>();
        wordProj.Word = word;
    }

    IEnumerator fireProjectile(string[] words)
    {
        foreach (var word in words)
        {
            HudMessageReceiver.Instance.SendHudMessage(word);
            FireWord(word);
            yield return new WaitForSeconds(0.1f);
        }
    }
}
