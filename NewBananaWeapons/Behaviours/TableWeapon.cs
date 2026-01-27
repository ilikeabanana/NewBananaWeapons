using NewBananaWeapons;
using System.Collections;
using UnityEngine;


public class TableWeapon : MonoBehaviour
{
    public Transform table;
    public GameObject tableProjectile;

    Vector3 origiScaleTable = new Vector3(64.32401f, 64.32401f, 64.32401f);
    float tableCooldown = 1;
    Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        StartCoroutine(ShaderManager.ApplyShaderToGameObject(tableProjectile));
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetBool("LeftClick", InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame);

        if(tableCooldown < 1)
        {
            tableCooldown += Time.deltaTime;
            table.localScale = Vector3.Lerp(table.localScale, origiScaleTable, tableCooldown);
        }
        else
        {
            table.localScale = origiScaleTable;
        }
    }

    public void KickTable()
    {
        Instantiate(tableProjectile, CameraController.Instance.transform.position, CameraController.Instance.transform.rotation);
        table.localScale = Vector3.zero;
        tableCooldown = 0;
    }
}
