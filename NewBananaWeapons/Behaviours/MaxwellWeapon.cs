using NewBananaWeapons;
using System.Collections;
using UnityEngine;


public class MaxwellWeapon : MonoBehaviour
{
    public GameObject MaxwellPrefab;
    public GameObject MaxwellTrans;
    public Material MaxwellEnraged;
    public Material MaxwellNormal;
    GameObject maxwell;

    int pets;
    Animator anim;
    GameObject rage;
    void Awake()
    {
        anim = GetComponent<Animator>();
        StartCoroutine(ShaderManager.ApplyShaderToGameObject(MaxwellPrefab));
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
                rage = Instantiate(AddressableManager.rageEffect,
                    transform);
                rage.layer = gameObject.layer;
                // make maxwell ENRAGED
            }
        }

        if (InputManager.Instance.InputSource.Fire2.WasPerformedThisFrame && maxwell == null && pets > 0)
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
            maxwell = Instantiate(MaxwellPrefab, hit.point, MaxwellPrefab.transform.rotation);
            maxwell.transform.localScale *= 3;
            MaxwellProjectile max = maxwell.GetComponent<MaxwellProjectile>();
            max.pets = pets;
            max.orgPos = hit.point;
            rage.transform.parent = max.transform;
            rage.transform.localPosition = Vector3.zero;
            rage.transform.localScale *= 2;
            rage.layer = maxwell.layer;
            pets = 0;
        }


    }
}
