using NewBananaWeapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RobloxWeapon : MonoBehaviour
{
    int curWeapon = 2;
    public List<GameObject> weaponObjects = new List<GameObject>()
    {
        null, null, null
    };
    void Update()
    {
        if (GunControl.Instance.activated)
        {
            if (InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame)
            {
                switch (curWeapon)
                {
                    case 0:
                        SwordAttack();
                        break;
                    case 1:
                        BombAttack();
                        break;
                    case 2:
                        RocketAttack();
                        break;
                }
            }

            if (InputManager.Instance.InputSource.Fire2.WasPerformedThisFrame)
            {
                curWeapon++;
                if(curWeapon >= weaponObjects.Count)
                {
                    curWeapon = 0;
                }
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
        rock.GetComponent<MeshRenderer>().material.color = Color.grey;
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
