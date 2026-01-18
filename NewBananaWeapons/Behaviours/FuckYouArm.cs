using System.Collections;
using UnityEngine;

public class FuckYouArm : MonoBehaviour
{
    Animator anim;

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
