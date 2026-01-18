using NewBananaWeapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DavidWeapon : MonoBehaviour
{
    public List<AudioClip> idleClips = new List<AudioClip>();

    public AudioClip ThereIsACar;
    public AudioClip AndItIsGoingToHitMe;
    public GameObject davidThrown;
    public GameObject car;

    float idleTimer = 0;
    AudioSource source;

    GameObject thrownDavid;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        StartCoroutine(ShaderManager.ApplyShaderToGameObject(car));
    }

    void Update()
    {
        Transform CamTrans = CameraController.Instance.transform;
        idleTimer -= Time.deltaTime;
        if(idleTimer < 0)
        {
            source.PlayOneShot(idleClips[Random.Range(0, idleClips.Count)]);
            idleTimer = Random.Range(3, 10);
        }

        if (InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame && thrownDavid == null)
        {
            if(Physics.Raycast(CamTrans.position, CamTrans.forward, out RaycastHit hit, 1000, LayerMaskDefaults.Get(LMD.Environment)))
            {
                thrownDavid = Instantiate(davidThrown, hit.point + new Vector3(0, davidThrown.transform.localScale.y / 2), davidThrown.transform.rotation);
            }
        }

        if (InputManager.Instance.InputSource.Fire2.WasPerformedThisFrame && thrownDavid != null)
        {
            source.PlayOneShot(ThereIsACar);

            Vector3 dir = FindBestCarSpawnDirection(thrownDavid.transform);
            Vector3 spawnPos = thrownDavid.transform.position + dir * 1000f;

            GameObject cra = Instantiate(car, spawnPos, Quaternion.identity);
            cra.GetComponent<CarProjectile>().target = thrownDavid.transform;
        }


    }

    Vector3 FindBestCarSpawnDirection(Transform thrownDavid, float hitRadius = 25)
    {
        List<EnemyIdentifier> enemies = EnemyTracker.Instance.GetCurrentEnemies();

        if (enemies == null || enemies.Count == 0)
            return Vector3.forward; // fallback

        Vector3 bestDir = Vector3.forward;
        int bestCount = 0;

        Vector3 origin = thrownDavid.position;

        foreach (var candidateEnemy in enemies)
        {
            if (candidateEnemy == null) continue;

            Vector3 candidateDir = (candidateEnemy.transform.position - origin).normalized;

            int count = 0;

            foreach (var testEnemy in enemies)
            {
                if (testEnemy == null) continue;

                Vector3 toEnemy = testEnemy.transform.position - origin;

                // Distance from enemy to the line through origin along candidateDir
                Vector3 projected = Vector3.Project(toEnemy, candidateDir);
                Vector3 perpendicular = toEnemy - projected;

                if (perpendicular.magnitude <= hitRadius)
                    count++;
            }

            if (count > bestCount)
            {
                bestCount = count;
                bestDir = candidateDir;
            }
        }

        return bestDir;
    }


    void OnEnable()
    {
        idleTimer = Random.Range(3, 10);
    }
}
