using NewBananaWeapons;
using System.Collections;
using UnityEngine;

public class NukeWeapon : MonoBehaviour
{
    public GameObject explosion;

    // Update is called once per frame
    void Update()
    {
        if (InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame)
        {
            GameObject biem = Instantiate(AddressableManager.explosion, transform.position, Quaternion.identity);
            foreach (var explosiono in biem.GetComponentsInChildren<Explosion>())
            {
                explosiono.maxSize *= 1000;
                explosiono.damage *= 100000;
                explosiono.speed *= 25;
            }
        }
    }

    public void Explode()
    {

    }
}
