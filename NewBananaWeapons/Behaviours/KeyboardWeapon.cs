using NewBananaWeapons;
using NewBananaWeapons.Behaviours.ProjectileBehaviours;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;

public class KeyboardWeapon : MonoBehaviour
{
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
        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Cube);
        projectile.transform.position = CameraController.Instance.transform.position;
        projectile.transform.forward = CameraController.Instance.transform.forward;
        WordProjectile wordProj = projectile.AddComponent<WordProjectile>();
        wordProj.Word = word;
        projectile.AddComponent<Rigidbody>().isKinematic = true;
        projectile.GetComponent<Collider>().isTrigger = true;
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
