using System.Collections;
using UnityEngine;

public class CharlesWeapon : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if (!GunControl.Instance.activated) return;

        Transform CamTrans = CameraController.Instance.transform;
        if (InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame)
        {
            if (Physics.Raycast(CamTrans.position, CamTrans.forward, out RaycastHit hit, 1000, LayerMaskDefaults.Get(LMD.Environment)))
            {
                GameObject heli = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                heli.transform.localScale *= 10;
                heli.transform.position = hit.point + new Vector3(0, 100, 0);
                heli.AddComponent<HelicopterProjectile>();
                heli.AddComponent<Rigidbody>().isKinematic = true;
                heli.GetComponent<Collider>().isTrigger = true;
            }
        }
    }
}
