using NewBananaWeapons;
using System.Collections.Generic;
using UnityEngine;

public class RobloxWeapon : MonoBehaviour
{
    public GameObject bomb;
    public GameObject superBall;
    public GameObject pellet;
    public GameObject rocket;

    public Animator weaponAnimator;

    int curWeapon = 0;

    public List<GameObject> weaponObjects = new List<GameObject>()
    {
        null, null, null, null, null, null, null, null, null
    };

    void Start()
    {
        UpdateWeaponActive();
         
        StartCoroutine(ShaderManager.ApplyShaderToGameObject(bomb));
        StartCoroutine(ShaderManager.ApplyShaderToGameObject(superBall));
        StartCoroutine(ShaderManager.ApplyShaderToGameObject(pellet));
        StartCoroutine(ShaderManager.ApplyShaderToGameObject(rocket));
    }

    void Update()
    {
        if (!GunControl.Instance.activated)
            return;

        // Speed change
        NewMovement.Instance.walkSpeed = (curWeapon == 8) ? 1500 : 750;

        // Fire
        if (InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame)
        {
            FireCurrentWeapon();
        }

        // Switch weapon
        if (InputManager.Instance.InputSource.Fire2.WasPerformedThisFrame)
        {
            curWeapon = (curWeapon + 1) % weaponObjects.Count;
            UpdateWeaponActive();
        }
    }

    void UpdateWeaponActive()
    {
        for (int i = 0; i < weaponObjects.Count; i++)
        {
            if (weaponObjects[i] != null)
                weaponObjects[i].SetActive(i == curWeapon);
        }

        if (weaponAnimator != null)
            weaponAnimator.SetTrigger("SwitchWeapon");
    }

    void FireCurrentWeapon()
    {
        PlayFireSound();

        switch (curWeapon)
        {
            case 0: SwordAttack(); break;
            case 1: BombAttack(); break;
            case 2: RocketAttack(); break;
            case 3: PaintballGun(); break;
            case 4: SlingshotAttack(); break;
            case 5: SuperballAttack(); break;
            case 6: TrowelBuild(); break;
            case 7: Hamburger(); break;
        }
    }

    // ----------------------------------------------------
    // Utility: Fire point resolving + sound
    // ----------------------------------------------------

    Transform GetFirePoint()
    {
        GameObject w = weaponObjects[curWeapon];
        if (w != null && w.TryGetComponent(out RobloxWeaponSounds s) && s.firePoint != null)
            return s.firePoint;

        return CameraController.Instance.transform;
    }

    void PlayFireSound()
    {
        GameObject w = weaponObjects[curWeapon];
        if (w != null && w.TryGetComponent(out RobloxWeaponSounds s) && s.fireSound != null)
        {
            w.GetComponent<AudioSource>().PlayOneShot(s.fireSound);
        }
    }

    // ----------------------------------------------------
    // Attacks
    // ----------------------------------------------------

    void Hamburger()
    {
        NewMovement.Instance.GetHealth(25, false);
    }

    void SlingshotAttack()
    {
        Transform fp = GetFirePoint();

        GameObject proj = Instantiate(pellet, fp.position, fp.rotation);
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb == null) rb = proj.AddComponent<Rigidbody>();

        rb.velocity = fp.forward * 25f;
        proj.AddComponent<SuperBallProjectile>().damage = 15;

        Destroy(proj, 9f);
    }

    void SuperballAttack()
    {
        Transform fp = GetFirePoint();

        GameObject proj = Instantiate(superBall, fp.position, fp.rotation);
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb == null) rb = proj.AddComponent<Rigidbody>();

        rb.mass = 0.8f;
        rb.velocity = fp.forward * 18f;
        rb.angularVelocity = Random.insideUnitSphere * 10f;

        Destroy(proj, 10f);
    }

    void RocketAttack()
    {
        Transform fp = GetFirePoint();

        GameObject proj = Instantiate(rocket, fp.position, fp.rotation);
        proj.AddComponent<Rocket>();
    }

    void BombAttack()
    {
        Transform fp = GetFirePoint();

        GameObject proj = Instantiate(bomb, fp.position, fp.rotation);
        proj.AddComponent<Bomb>();
    }

    void PaintballGun()
    {
        GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rock.transform.position = CameraController.Instance.transform.position;
        rock.transform.forward = CameraController.Instance.transform.forward;
        rock.transform.localScale *= 1.3f;
        Rigidbody rb = rock.AddComponent<Rigidbody>();
        rock.GetOrAddComponent<Collider>().isTrigger = true;
        rb.isKinematic = true;
        rock.AddComponent<Paintball>();
        rock.GetComponent<MeshRenderer>().material = new Material(AddressableManager.unlit);

        Vector3 randColor = Random.insideUnitCircle;
        rock.GetComponent<MeshRenderer>().material.color = new Color(randColor.x, randColor.y, randColor.z);
    }

    // ----------------------------------------------------
    // Sword unchanged
    // ----------------------------------------------------

    void SwordAttack()
    {
        Transform cam = CameraController.Instance.transform;
        Vector3 origin = cam.position;
        Vector3 dir = cam.forward;

        bool hitEnemy = false;
        RaycastHit hit;

        if (!Physics.Raycast(origin, dir, out hit, 4f, LayerMaskDefaults.Get(LMD.Enemies), QueryTriggerInteraction.Collide))
        {
            Physics.BoxCast(origin, Vector3.one * 0.3f, dir, out hit, cam.rotation, 4f,
                LayerMaskDefaults.Get(LMD.Enemies), QueryTriggerInteraction.Collide);
        }

        if (hit.collider != null && hit.collider.TryGetComponent(out EnemyIdentifierIdentifier eidd))
        {
            DealSwordDamage(eidd, hit.point, dir);
            hitEnemy = true;
        }

        if (!hitEnemy)
        {
            Collider[] closeHits = Physics.OverlapSphere(origin + dir * 1.5f, 1f,
                LayerMaskDefaults.Get(LMD.Enemies), QueryTriggerInteraction.Collide);

            foreach (Collider c in closeHits)
            {
                if (c.TryGetComponent(out EnemyIdentifierIdentifier eiddd))
                {
                    DealSwordDamage(eiddd, c.transform.position, dir);
                    hitEnemy = true;
                    break;
                }
            }
        }

        if (hitEnemy)
        {
            MonoSingleton<TimeController>.Instance.HitStop(0.05f);
            CameraController.Instance.CameraShake(0.2f);
        }
    }

    void DealSwordDamage(EnemyIdentifierIdentifier eidd, Vector3 hitPoint, Vector3 direction)
    {
        EnemyIdentifier eid = eidd.eid;
        eid.DeliverDamage(
            eidd.gameObject,
            direction * 100f,
            hitPoint,
            25,
            false
        );
    }

    void TrowelBuild()
    {
        int bricksWide = 4;
        int bricksTall = 6;

        Vector3 forward = CameraController.Instance.transform.forward;
        Vector3 basePos = CameraController.Instance.transform.position + forward * 2.5f;

        Quaternion rot = Quaternion.LookRotation(forward);

        Vector3 brickSize = new Vector3(2f, 1f, 0.5f);
        float spacing = 0.05f;

        for (int y = 0; y < bricksTall; y++)
        {
            for (int x = 0; x < bricksWide; x++)
            {
                GameObject brick = GameObject.CreatePrimitive(PrimitiveType.Cube);

                brick.transform.rotation = rot;
                brick.transform.localScale = brickSize;

                // Offset sideways relative to camera direction
                Vector3 right = CameraController.Instance.transform.right;

                Vector3 spawnPos =
                    basePos +
                    right * ((x - (bricksWide - 1) * 0.5f) * (brickSize.x + spacing)) +
                    Vector3.up * (y * (brickSize.y + spacing) + 5f); // spawn above so it falls

                brick.transform.position = spawnPos;

                // Material
                var renderer = brick.GetComponent<MeshRenderer>();
                renderer.material = new Material(AddressableManager.unlit);
                renderer.material.color = Color.gray;

                // Physics
                Rigidbody rb = brick.AddComponent<Rigidbody>();
                rb.mass = 2f;
                rb.interpolation = RigidbodyInterpolation.Interpolate;

                // Optional slight randomness so stacking isn't perfect
                rb.AddForce(Random.insideUnitSphere * 0.3f, ForceMode.Impulse);

                Destroy(brick, 20f);
            }
        }
    }
}
