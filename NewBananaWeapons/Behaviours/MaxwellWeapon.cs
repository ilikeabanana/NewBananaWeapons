using NewBananaWeapons;
using System.Collections;
using UnityEngine;


public class MaxwellWeapon : MonoBehaviour
{
    public GameObject MaxwellPrefab;
    public GameObject MaxwellTrans;
    public AudioClip meowSound;
    public Material MaxwellEnraged;
    public Material MaxwellNormal;

    AudioSource source;
    GameObject maxwell;

    int pets;
    Animator anim;
    GameObject rage;
    void Awake()
    {
        anim = GetComponent<Animator>();
        source = GetComponent<AudioSource>();
    }
    // Update is called once per frame
    void Update()
    {
        anim.SetBool("PetMax", false);
        anim.SetBool("ThrowMax", false);
        if (InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame && pets < 5
            && !Banana_WeaponsPlugin.cooldowns.ContainsKey(gameObject))
        {
            anim.SetBool("PetMax", true);
            pets++;
            source.PlayOneShot(meowSound);
            Banana_WeaponsPlugin.cooldowns.Add(gameObject, 1.35f);
            if(pets == 5)
            {
                rage = Instantiate(AddressableManager.rageEffect,
                    MaxwellTrans.transform);
                StartCoroutine(ShaderManager.ApplyShaderToGameObject(MaxwellTrans));
                MaxwellTrans.GetComponent<Renderer>().material = MaxwellEnraged;
                rage.layer = gameObject.layer;
                rage.transform.localScale /= 4;
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
            if(pets == 5)
                maxwell.GetComponent<Renderer>().material = MaxwellEnraged;
            max.pets = pets;
            max.orgPos = hit.point;
            rage.transform.parent = max.transform;
            rage.transform.localPosition = Vector3.zero;
            rage.transform.localScale *= 2;
            rage.layer = maxwell.layer;
            pets = 0;
            MaxwellTrans.GetComponent<Renderer>().material = MaxwellNormal;
            StartCoroutine(ShaderManager.ApplyShaderToGameObject(MaxwellTrans));
            StartCoroutine(ShaderManager.ApplyShaderToGameObject(maxwell));
        }


    }
}
