using NewBananaWeapons;
using System.Collections;
using UnityEngine;

public class CharlesWeapon : BaseWeapon
{
    public GameObject helicopter;


    public static ConfigVar<float> helicopterDamage;

    public override void SetupConfigs(string sectionName)
    {
        helicopterDamage = new ConfigVar<float>(sectionName, "Damage", 70);
        base.SetupConfigs(sectionName);
    }

    public override string GetWeaponDescription()
    {
        return "Click somewhere to spawn charles, charles will spawn 1350m high up in the air and fall down, will pierce through the ground dealing 70 damage to enemies, and instakilling the player.";
    }

    void Start()
    {
        StartCoroutine(ShaderManager.ApplyShaderToGameObject(helicopter));
    }


    // Update is called once per frame
    void Update()
    {
        if (!GunControl.Instance.activated) return;

        Transform CamTrans = CameraController.Instance.transform;
        if (InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame)
        {
            if (Physics.Raycast(CamTrans.position, CamTrans.forward, out RaycastHit hit, 1000, LayerMaskDefaults.Get(LMD.Environment)))
            {
                Instantiate(helicopter, hit.point + new Vector3(0, 1350, 0), helicopter.transform.rotation);
            }
        }
    }
}
