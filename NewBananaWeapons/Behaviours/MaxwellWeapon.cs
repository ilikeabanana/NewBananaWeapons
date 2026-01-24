using System.Collections;
using UnityEngine;


public class MaxwellWeapon : MonoBehaviour
{
    public GameObject MaxwellPrefab;
    GameObject maxwell;

    int pets;
    Animator anim;
    void Awake()
    {
        anim = GetComponent<Animator>(); 
    }
    // Update is called once per frame
    void Update()
    {
        anim.SetBool("PetMax", false);
        anim.SetBool("ThrowMax", false);
        if (InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame && pets < 5)
        {
            anim.SetBool("PetMax", true);
            pets++;
            if(pets == 5)
            {
                // make maxwell ENRAGED
            }
        }

        if (InputManager.Instance.InputSource.Fire2.WasPerformedThisFrame && maxwell == null)
        {
            anim.SetBool("ThrowMax", true);
        } 
    }

    public void ThrowMax()
    { 
        Transform camTrans = CameraController.Instance.transform;
        RaycastHit hit;
        if (Physics.Raycast(camTrans.position, camTrans.forward, out hit, 45,
            LayerMaskDefaults.Get(LMD.Environment)))
        {
            maxwell = Instantiate(MaxwellPrefab, hit.point, Quaternion.identity);
            MaxwellProjectile max = maxwell.GetComponent<MaxwellProjectile>();
            max.pets = pets;
            max.orgPos = hit.point;
        }


    }
}
