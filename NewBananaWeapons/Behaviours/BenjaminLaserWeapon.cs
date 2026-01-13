using NewBananaWeapons;
using System.Collections;
using UnityEngine;

public class BenjaminLaserWeapon : MonoBehaviour
{
    public GameObject chargingLaser;
    public GameObject laser;

    Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
        StartCoroutine(ShaderManager.ApplyShaderToGameObject(laser));
    }
    void Update()
    { 
        if (Banana_WeaponsPlugin.cooldowns.ContainsKey(gameObject))
        {
            chargingLaser.SetActive(false);
            anim.SetBool("Activating", false);
            return;
        }

        chargingLaser.SetActive(true);

        if (InputManager.instance.InputSource.Fire1.WasPerformedThisFrame)
        {
            anim.SetBool("Activating", true);
        }
    }

    public void FireLaser()
    {
        Instantiate(laser, CameraController.Instance.transform.position, 
            CameraController.Instance.transform.rotation).transform.Rotate(90, 0, 0);
        Banana_WeaponsPlugin.cooldowns.Add(gameObject, 25);
    }
}
