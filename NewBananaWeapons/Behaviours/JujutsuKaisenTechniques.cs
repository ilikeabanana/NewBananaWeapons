using NewBananaWeapons;
using System.Collections;
using UnityEngine;

public class JujutsuKaisenTechniques : MonoBehaviour
{
    bool chargingBlue;
    bool chargingRed;
    bool chargingPurple;

    InputManager inman;
    AudioSource source;

    void Awake()
    {
        inman = InputManager.Instance;
        source = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!GunControl.Instance.activated) return;
        if (Banana_WeaponsPlugin.cooldowns.ContainsKey(gameObject)) return;

        if(inman.InputSource.Fire1.WasCanceledThisFrame || inman.InputSource.Fire2.WasCanceledThisFrame)
        {
            if (chargingBlue)
            {
                GameObject blueProj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                blueProj.AddComponent<TechniqueProjectile>();
                blueProj.AddComponent<TechniqueProjectile>().technique = TechniqueProjectile.TechniqueType.Blue;
                blueProj.transform.position = CameraController.Instance.transform.position;
                blueProj.transform.forward = CameraController.Instance.transform.forward;
                source.PlayOneShot(AddressableManager.blackholeLaunch);
                blueProj.GetComponent<Renderer>().material = new Material(AddressableManager.unlit);
                blueProj.GetComponent<Renderer>().material.color = Color.blue;
                Banana_WeaponsPlugin.cooldowns.Add(gameObject, 3.5f);
            }
            else if (chargingRed)
            {
                GameObject blueProj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                blueProj.AddComponent<TechniqueProjectile>().technique = TechniqueProjectile.TechniqueType.Red;
                blueProj.transform.position = CameraController.Instance.transform.position;
                blueProj.transform.forward = CameraController.Instance.transform.forward;
                blueProj.GetComponent<Renderer>().material = new Material(AddressableManager.unlit);
                blueProj.GetComponent<Renderer>().material.color = Color.red;
                source.PlayOneShot(AddressableManager.blackholeLaunch);
                Banana_WeaponsPlugin.cooldowns.Add(gameObject, 3.5f);
            }
            else if (chargingPurple)
            {
                GameObject blueProj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                blueProj.AddComponent<TechniqueProjectile>();
                blueProj.AddComponent<TechniqueProjectile>().technique = TechniqueProjectile.TechniqueType.Purple;
                blueProj.transform.position = CameraController.Instance.transform.position;
                blueProj.transform.forward = CameraController.Instance.transform.forward;
                source.PlayOneShot(AddressableManager.blackholeLaunch);
                blueProj.GetComponent<Renderer>().material = new Material(AddressableManager.unlit);
                blueProj.GetComponent<Renderer>().material.color = new Color(120, 0, 120);
                blueProj.transform.localScale *= 10;
                Banana_WeaponsPlugin.cooldowns.Add(gameObject, 4.5f);
            }
            chargingBlue = false;
            chargingRed = false;
            chargingPurple = false;
        }

        if (inman.InputSource.Fire1.IsPressed && !chargingPurple)
        {
            chargingBlue = true;
        }
        else
        {
            chargingBlue = false;
        }
        if (inman.InputSource.Fire2.IsPressed && !chargingPurple)
        {
            chargingRed = true;
        }
        else
        {
            chargingRed = false;
        }

        chargingPurple = (chargingBlue && chargingRed) || chargingPurple;
    }
}
