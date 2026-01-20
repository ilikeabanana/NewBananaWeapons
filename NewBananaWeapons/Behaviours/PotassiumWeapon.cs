using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotassiumWeapon : MonoBehaviour
{
    public GameObject carPrefab;

    GameObject currentCar;
    List<Transform> wayPoints = new List<Transform>();

    int currentWaypointTarget = 0;
    LineRenderer line;

    float curveThreshold = 8f;
    int curveSubdivisions = 5;
    float curveStrength = 4f; // sideways bend amount



    // Update is called once per frame
    void Update()
    {
        if (!GunControl.Instance.activated) return;
        Transform camTransform = CameraController.Instance.transform;
        if (InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame && currentCar == null)
        {
            if (Physics.Raycast(camTransform.position, camTransform.forward,
                out RaycastHit hit,
                100, LayerMaskDefaults.Get(LMD.Environment)))
            {
                AddWaypoint(hit.point);
            }
        }

        if (InputManager.Instance.InputSource.Fire2.WasPerformedThisFrame)
        {
            currentCar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            currentCar.transform.position = wayPoints[0].position;
            currentWaypointTarget = 0;
        }
        if (currentCar != null)
        {
            UpdateCarPosition();
            CheckForEnemies();
        }
        UpdateLine();
        
    }

    void UpdateCarPosition()
    {
        Vector3 dir = (wayPoints[currentWaypointTarget].position -
            currentCar.transform.position).normalized;
        currentCar.transform.forward = dir;
        currentCar.transform.position += dir * 25 * Time.deltaTime;
        if (Vector3.Distance(wayPoints[currentWaypointTarget].position,
            currentCar.transform.position) <= 0.5f) 
        {
            currentWaypointTarget++;
            if(currentWaypointTarget >= wayPoints.Count)
            {
                foreach (var way in wayPoints)
                {
                    Destroy(way.gameObject);
                }
                wayPoints.Clear();
                Destroy(currentCar);
                return;
            }
        }
    }

    void AddWaypoint(Vector3 newPos)
    {
        // If no previous waypoint, just place one
        if (wayPoints.Count == 0)
        {
            CreateWaypoint(newPos);
            return;
        }

        Vector3 prevPos = wayPoints[wayPoints.Count - 1].position;

        // Only consider XZ distance
        Vector2 prevXZ = new Vector2(prevPos.x, prevPos.z);
        Vector2 newXZ = new Vector2(newPos.x, newPos.z);

        float xzDistance = Vector2.Distance(prevXZ, newXZ);

        // If below threshold, just add normally
        if (xzDistance < curveThreshold)
        {
            CreateWaypoint(newPos);
            return;
        }

        Vector3 mid = (prevPos + newPos) * 0.5f;

        Vector3 dir = (newPos - prevPos).normalized;
        Vector3 side = new Vector3(-dir.z, 0f, dir.x); // perpendicular on XZ

        mid += side * curveStrength;


        for (int i = 1; i <= curveSubdivisions; i++)
        {
            float t = i / (float)(curveSubdivisions + 1);

            // Quadratic Bezier
            Vector3 p =
                Mathf.Pow(1 - t, 2) * prevPos +
                2 * (1 - t) * t * mid +
                Mathf.Pow(t, 2) * newPos;

            CreateWaypoint(p);
        }

        // Finally add the real target point
        CreateWaypoint(newPos);
    }

    void CreateWaypoint(Vector3 pos)
    {
        GameObject waypoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        waypoint.transform.position = pos;
        wayPoints.Add(waypoint.transform);
    }


    void CheckForEnemies()
    {
        Collider[] cols = Physics.OverlapSphere(currentCar.transform.position, 5);
        if(cols.Length > 0)
        {
            foreach (var col in cols)
            {
                if(col.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier eidd))
                {
                    StartCoroutine(KillEnemy(eidd.eid));
                }
            }
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

    }

    void UpdateLine()
    {
        if(wayPoints.Count == 0)
        {
            line.positionCount = 0;
        }
        if (line == null)
            CreateLine();
        if (wayPoints.Count == 0)
        {
            line.positionCount = 0;
            return;
        }

 

        line.positionCount = wayPoints.Count;

        for (int i = 0; i < wayPoints.Count; i++)
        {
            line.SetPosition(i, wayPoints[i].position);
        }
    }

    void CreateLine()
    {
        GameObject lineobject = new GameObject("Pathway");
        line = lineobject.AddComponent<LineRenderer>();
    }


}
