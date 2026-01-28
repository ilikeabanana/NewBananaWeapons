using NewBananaWeapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobloxWeapon : MonoBehaviour
{
    int curWeapon = 0;

    public List<GameObject> weaponObjects = new List<GameObject>()
    {
        null, null, null, null, null, null, null, null, null
    };

    void Update()
    {
        // ROBLOX, ITS FREEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE

        if (GunControl.Instance.activated)
        {
            if(curWeapon == 8)
            {
                NewMovement.Instance.walkSpeed = 1500;
            }
            else
            {
                NewMovement.Instance.walkSpeed = 750;
            }

            if (InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame)
            {
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

            if (InputManager.Instance.InputSource.Fire2.WasPerformedThisFrame)
            {
                curWeapon = (curWeapon + 1) % weaponObjects.Count;
            }
        }
    }

    void Hamburger()
    {
        NewMovement.Instance.GetHealth(25, false);
    }

    void SlingshotAttack()
    {
        GameObject pellet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pellet.transform.position = CameraController.Instance.transform.position + CameraController.Instance.transform.forward * 1.2f;
        pellet.transform.localScale *= 0.5f;

        Rigidbody rb = pellet.AddComponent<Rigidbody>();
        Collider col = pellet.GetOrAddComponent<Collider>();
        col.material = new PhysicMaterial()
        {
            bounciness = 0.9f,
            bounceCombine = PhysicMaterialCombine.Maximum,
            frictionCombine = PhysicMaterialCombine.Minimum
        };

        rb.velocity = CameraController.Instance.transform.forward * 25f;

        pellet.GetComponent<MeshRenderer>().material = new Material(AddressableManager.unlit);
        pellet.GetComponent<MeshRenderer>().material.color = Color.white;

        pellet.AddComponent<SuperBallProjectile>().damage = 15;
        Destroy(pellet, 9f);
    }

    void SuperballAttack()
    {
        GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.transform.position = CameraController.Instance.transform.position + CameraController.Instance.transform.forward * 1.2f;
        ball.transform.localScale *= 1.2f;
        ball.AddComponent<SuperBallProjectile>();

        Rigidbody rb = ball.AddComponent<Rigidbody>();
        rb.mass = 0.8f;
        rb.velocity = CameraController.Instance.transform.forward * 18f;
        rb.angularVelocity = Random.insideUnitSphere * 10f;

        Collider col = ball.GetOrAddComponent<Collider>();
        col.material = new PhysicMaterial()
        {
            bounciness = 0.9f,
            bounceCombine = PhysicMaterialCombine.Maximum,
            frictionCombine = PhysicMaterialCombine.Minimum
        };

        ball.GetComponent<MeshRenderer>().material = new Material(AddressableManager.unlit);
        ball.GetComponent<MeshRenderer>().material.color = Color.yellow;

        Destroy(ball, 10f);
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

    void RocketAttack()
    {
        GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rock.transform.position = CameraController.Instance.transform.position;
        rock.transform.forward = CameraController.Instance.transform.forward;
        rock.transform.localScale *= 1.3f;
        Rigidbody rb = rock.AddComponent<Rigidbody>();
        rock.GetOrAddComponent<Collider>().isTrigger = true;
        rb.isKinematic = true;
        rock.AddComponent<Rocket>();
        rock.GetComponent<MeshRenderer>().material = new Material(AddressableManager.unlit);
        rock.GetComponent<MeshRenderer>().material.color = Color.grey;
    }

    void BombAttack()
    {
        GameObject bomb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bomb.transform.position = CameraController.Instance.transform.position;
        bomb.transform.localScale *= 2;
        bomb.AddComponent<Rigidbody>();
        AudioSource aud = bomb.AddComponent<AudioSource>();
        aud.pitch = 2;
        bomb.AddComponent<Bomb>();
        bomb.GetComponent<MeshRenderer>().material = new Material(AddressableManager.unlit);
        bomb.GetComponent<MeshRenderer>().material.color = Color.black;
    }

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

        if (hit.collider != null)
        {
            if (hit.collider.TryGetComponent(out EnemyIdentifierIdentifier eidd))
            {
                DealSwordDamage(eidd, hit.point, dir);
                hitEnemy = true;
            }
        }

        if (!hitEnemy)
        {
            Collider[] closeHits = Physics.OverlapSphere(origin + dir * 1.5f, 1f,
                LayerMaskDefaults.Get(LMD.Enemies), QueryTriggerInteraction.Collide);

            foreach (Collider c in closeHits)
            {
                if (c.TryGetComponent(out EnemyIdentifierIdentifier eidd))
                {
                    DealSwordDamage(eidd, c.transform.position, dir);
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


}
