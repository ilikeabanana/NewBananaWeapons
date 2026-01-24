using System.Collections;
using UnityEngine;


public class MaxwellWeapon : MonoBehaviour
{
    GameObject maxwell;

    int pets;

    // Update is called once per frame
    void Update()
    {
        Transform camTrans = CameraController.Instance.transform;
        if (InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame && pets < 5)
        {
            pets++;
            if(pets == 5)
            {
                // make maxwell ENRAGED
            }
        }

        if (InputManager.Instance.InputSource.Fire2.WasPerformedThisFrame && maxwell == null)
        {
            RaycastHit hit;
            if(Physics.Raycast(camTrans.position, camTrans.forward, out hit, 45, 
                LayerMaskDefaults.Get(LMD.Environment)))
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(cube.GetComponent<Collider>());
                MaxwellProjectile max = cube.AddComponent<MaxwellProjectile>();
                max.pets = pets;
                max.orgPos = hit.point;
            }

            
        }
    }
}
