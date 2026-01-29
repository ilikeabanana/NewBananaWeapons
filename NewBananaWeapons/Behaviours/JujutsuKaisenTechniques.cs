using NewBananaWeapons;
using System.Collections;
using UnityEngine;

public class JujutsuKaisenTechniques : MonoBehaviour
{
    bool chargingBlue;
    bool chargingRed;
    bool chargingPurple;

    InputManager inman;

    void Awake()
    {
        inman = InputManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if(inman.InputSource.Fire1.WasCanceledThisFrame || inman.InputSource.Fire2.WasCanceledThisFrame)
        {
            if (chargingBlue)
            {
                GameObject blueProj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                blueProj.AddComponent<TechniqueProjectile>();
                blueProj.transform.position = CameraController.Instance.transform.position;
                blueProj.transform.forward = CameraController.Instance.transform.forward;
            }
            else if (chargingRed)
            {
                GameObject blueProj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                blueProj.AddComponent<TechniqueProjectile>().reversal = true;
                blueProj.transform.position = CameraController.Instance.transform.position;
                blueProj.transform.forward = CameraController.Instance.transform.forward;
            }
            else if (chargingPurple)
            {
                GameObject beam = Instantiate(AddressableManager.normalBeam,
                    CameraController.Instance.transform.position,
                    CameraController.Instance.transform.rotation);
                if (beam.TryGetComponent<RevolverBeam>(out RevolverBeam baem))
                {
                    baem.hitAmount = 9999;
                    baem.damage = 14;
                    baem.enemyDamageOverride = 900;
                }
                if (beam.TryGetComponent<LineRenderer>(out LineRenderer lr))
                {
                    lr.startColor = new Color(128, 0, 128);
                }
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
