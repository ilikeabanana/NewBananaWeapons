using NewBananaWeapons;
using System.Collections;
using UnityEngine;


public class GambleGun : MonoBehaviour
{
    public Transform firePoint;
    Animator anim;
    public GameObject[] slots;

    public GameObject[] projectilesPerFireMode;

    public AudioClip lossClip;
    public AudioClip winClip;

    AudioSource source;

    bool spinning;

    bool riggedMode = false; 

    // 0 = Kitr, 1 = Filth, 2 = Maurice, 3 = Seven, 4 = Coin
    public int riggedResult = 2;


    // Each slot is 72° 
    // Filth = 72
    // Maurice = 144
    // Seven = 216
    // Coin = 288
    // Kitr = 0 or 360

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
    }

    private void OnDisable()
    {
        spinning = false;
        foreach (var slot in slots)
        {
            var t = slot.transform;

            float snappedZ;

            if (riggedMode)
            {
                // Force all slots to the rigged result
                snappedZ = (riggedResult * 72f) % 360f;
            }
            else
            {
                // Normal random snapping
                float z = t.localEulerAngles.z;
                snappedZ = Mathf.Round(z / 72f) * 72f % 360f;
            }

            t.localEulerAngles = new Vector3(
                t.localEulerAngles.x,
                t.localEulerAngles.y,
                snappedZ
            );
        }
        StopAllCoroutines();
    }
    void Update()
    {
        if (InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame && !spinning)
        {
            spinning = true;
            StartCoroutine(Spin());
        }

        if (InputManager.Instance.InputSource.Fire2.WasPerformedThisFrame)
        {
            riggedResult++;
            if(riggedResult == 5)
            {
                riggedResult = 0;
            }
        }

        projectilesPerFireMode[3] = AddressableManager.mauriceBeam;
        projectilesPerFireMode[5] = AddressableManager.normalBeam;
        projectilesPerFireMode[1] = AddressableManager.normalBeam;
        projectilesPerFireMode[0] = AddressableManager.normalBeam;
    }

    IEnumerator Spin()
    {
        float[] speeds = new float[slots.Length];
        for (int i = 0; i < speeds.Length; i++)
        {
            speeds[i] = Random.Range(1.0f, 3.0f);
        }

        float timer = 0;
        float duration = 0.5f;
        while (timer < duration)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].transform.Rotate(new Vector3(0, 0, 500 * speeds[i] * Time.deltaTime));
            }

            timer += Time.deltaTime;
            yield return null;
        }

        foreach (var slot in slots)
        {
            var t = slot.transform;

            float snappedZ;

            if (riggedMode)
            {
                // Force all slots to the rigged result
                snappedZ = (riggedResult * 72f) % 360f;
            }
            else
            {
                // Normal random snapping
                float z = t.localEulerAngles.z;
                snappedZ = Mathf.Round(z / 72f) * 72f % 360f;
            }

            t.localEulerAngles = new Vector3(
                t.localEulerAngles.x,
                t.localEulerAngles.y,
                snappedZ
            );
        }

        spinning = false;
        Fire();
    }
    public void FireSeven()
    {
        GameObject projectile = Instantiate(projectilesPerFireMode[4], firePoint.transform.position, firePoint.transform.rotation);
        Vector3 targetPoint;

        if (Physics.Raycast(CameraController.Instance.transform.position,
                            CameraController.Instance.transform.forward,
                            out RaycastHit hit,
                            1000,
                            LayerMaskDefaults.Get(LMD.EnemiesAndEnvironment)))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = CameraController.Instance.transform.position +
                          CameraController.Instance.transform.forward * 1000f;
        }

        StartCoroutine(ShaderManager.ApplyShaderToGameObject(projectile));

        projectile.transform.LookAt(targetPoint, Vector3.up);
        if (projectile.TryGetComponent<Projectile>(out Projectile proj))
        {
            if (proj.explosionEffect != null)
            {
                StartCoroutine(ShaderManager.ApplyShaderToGameObject(proj.explosionEffect));
                if (proj.explosionEffect.GetComponentInChildren<Explosion>())
                {
                    StartCoroutine(ShaderManager.ApplyShaderToGameObject(proj.explosionEffect.GetComponentInChildren<Explosion>().explosionChunk));
                }
            }
            proj.playerBullet = true;
            proj.friendly = true;
        }
    }

    public GameObject sevenChargeParticle;

    void Fire()
    {
        int fireMode = getFireMode();
        if(fireMode == 4)
        {
            Instantiate(sevenChargeParticle, sevenChargeParticle.transform.parent).SetActive(true);
            return;
        }
        if (anim != null)
            anim.SetTrigger("Fire");
        
        if (fireMode == 0)
            source.PlayOneShot(lossClip);
        else
        {
            StyleHUD.Instance.AddPoints(500, "<color=#00ffffff>JACKPOT!</color>", gameObject);

            source.PlayOneShot(winClip); 
        }
            
        GameObject projectile = Instantiate(projectilesPerFireMode[fireMode], firePoint.transform.position, firePoint.transform.rotation);
        Vector3 targetPoint;

        if (Physics.Raycast(CameraController.Instance.transform.position,
                            CameraController.Instance.transform.forward,
                            out RaycastHit hit,
                            1000,
                            LayerMaskDefaults.Get(LMD.EnemiesAndEnvironment)))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = CameraController.Instance.transform.position +
                          CameraController.Instance.transform.forward * 1000f;
        }

        StartCoroutine(ShaderManager.ApplyShaderToGameObject(projectile));

        projectile.transform.LookAt(targetPoint, Vector3.up);

        if(projectile.TryGetComponent<RevolverBeam>(out RevolverBeam beam))
        {
            switch (fireMode)
            {
                case 5:
                    beam.damage = 1;
                    beam.enemyDamageOverride = 75;
                    break;
                case 0:
                    beam.damage = 0.5f;
                    break;
                case 1:
                    beam.damage = 1;
                    beam.enemyDamageOverride = 15;
                    break;
            }
        }
        if(projectile.TryGetComponent<Projectile>(out Projectile proj))
        {
            if(proj.explosionEffect != null)
            {
                StartCoroutine(ShaderManager.ApplyShaderToGameObject(proj.explosionEffect));
                if (proj.explosionEffect.GetComponentInChildren<Explosion>())
                {
                    StartCoroutine(ShaderManager.ApplyShaderToGameObject(proj.explosionEffect.GetComponentInChildren<Explosion>().explosionChunk));
                }
            }
            proj.playerBullet = true;
            proj.friendly = true;
        }
    }

    int getFireMode()
    {
        // first, check if all the slots are in the same rotation
        int prevRot = Mathf.FloorToInt(slots[0].transform.localEulerAngles.z);
        for (int i = 1; i < slots.Length; i++)
        {
            int currentRot = Mathf.FloorToInt(slots[i].transform.localEulerAngles.z);
            if (currentRot != prevRot) return 0;
        }

        int index = Mathf.RoundToInt(prevRot / 72f) % 5;

        switch (index)
        {
            case 0: return 1; // Kitr
            case 1: return 2; // Filth
            case 2: return 3; // Maurice
            case 3: return 4; // Seven
            case 4: return 5; // Coin
        }

        return 0;
    }
}
