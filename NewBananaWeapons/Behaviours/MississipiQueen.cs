using System.Collections;
using UnityEngine;


public class MississipiQueen : MonoBehaviour
{
    Animator anim;
    bool doingSequence = false;
    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame && !doingSequence)
        {
            TimeController.Instance.timeScale = 0;
            TimeController.instance.RestoreTime();
        }
    }

    public void EndSequence()
    {

    }

    public void FireBullet()
    {

    }
}
