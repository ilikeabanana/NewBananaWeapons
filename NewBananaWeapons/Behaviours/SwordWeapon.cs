using System.Collections;
using ULTRAKILL.Cheats;
using UnityEngine;


public class SwordWeapon : MonoBehaviour
{
    float timeBetweenSlashes = 3;
    float cd;
    Animator anim;
    // Use this for initialization
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!GunControl.Instance.activated) return;
        if (NoWeaponCooldown.NoCooldown)
        {
            cd = 0;
        }
        cd -= Time.deltaTime;
        if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.IsPressed && cd <= 0)
        {
            anim.SetBool("DoingTheThing", true);
            cd = timeBetweenSlashes;
        }
        else
        {
            anim.SetBool("DoingTheThing", false);
        }
    }
}
