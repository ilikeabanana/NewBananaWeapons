using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarPathFollower : MonoBehaviour
{
    List<Transform> wayPoints;
    int currentWaypointTarget;
    bool active;

    public float moveSpeed = 25f;
    public AudioClip enemyHit;
    AudioSource source;

    void Awake()
    {
        source = transform.Find("CarYelp").GetComponent<AudioSource>();
    }

    public void SetPath(List<Transform> newPath)
    {
        wayPoints = newPath;
        currentWaypointTarget = 0;
        active = true;
    }

    void Update()
    {
        if (!active || wayPoints == null || wayPoints.Count == 0) return;

        UpdateCarPosition();
        CheckForEnemies();
    }

    void UpdateCarPosition()
    {
        Vector3 targetPos = wayPoints[currentWaypointTarget].position;
        Vector3 dir = (targetPos - transform.position).normalized;

        transform.forward = dir;
        transform.position += dir * moveSpeed * Time.deltaTime;

        if (Vector3.Distance(targetPos, transform.position) <= 0.5f)
        {
            currentWaypointTarget++;
            if (DeltaruneTextBox.Instance != null)
                DeltaruneTextBox.Instance.TextboxText("Potassium", transform);

            if (currentWaypointTarget >= wayPoints.Count)
            {
                CleanupPath();
                Destroy(gameObject);
            }
        }
    }

    void CleanupPath()
    {
        foreach (var way in wayPoints)
            if (way != null)
                Destroy(way.gameObject);
    }

    void CheckForEnemies()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, 5);
        foreach (var col in cols)
        {
            if (col.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier eidd))
                StartCoroutine(KillEnemy(eidd.eid));
        }
    }

    IEnumerator KillEnemy(EnemyIdentifier eid)
    {
        GameObject knocked = Instantiate(eid.gameObject, eid.transform.position, eid.transform.rotation);
        knocked.SetActive(false);

        foreach (var comp in knocked.GetComponentsInChildren<Component>(true))
        {
            if (comp is Transform) continue;
            if (comp is Renderer) continue;
            DestroyImmediate(comp);
        }

        Rigidbody rb = knocked.AddComponent<Rigidbody>();
        knocked.transform.Rotate(0f, 0f, 180f);
        knocked.SetActive(true);

        yield return new WaitForFixedUpdate();
        rb.isKinematic = false;
        rb.AddForce(Vector3.up * 50, ForceMode.VelocityChange);

        eid.InstaKill();
        Destroy(eid.gameObject);

        if(source != null)
        {
            source.PlayOneShot(enemyHit);
        }
    }
}
