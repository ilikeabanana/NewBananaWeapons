using NewBananaWeapons;
using System.Collections;
using UnityEngine;

public class CharlesWeapon : MonoBehaviour
{
    public GameObject helicopter;

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
