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
        anim.SetBool("Holding", MonoSingleton<InputManager>.Instance.InputSource.Punch.IsPressed);
    }

}
