using System.Collections;
using UnityEngine;

public class FuckYouArm : BaseWeapon
{
    Animator anim;

    public override string GetWeaponDescription()
    {
        return "Express dissatisfaction...";
    }

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void Update()
    {
        if (!GunControl.Instance.activated) return;
        anim.SetBool("Holding", MonoSingleton<InputManager>.Instance.InputSource.Punch.IsPressed);
    }

}
