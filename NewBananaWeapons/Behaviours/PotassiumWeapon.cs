using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotassiumWeapon : MonoBehaviour
{
    GameObject currentCar;
    List<Transform> wayPoints = new List<Transform>();

    int currentWaypointTarget = 0;
    LineRenderer line;

    // Update is called once per frame
    void Update()
    {
        Transform camTransform = CameraController.Instance.transform;
        if (InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame && currentCar == null)
        {
            if (Physics.Raycast(camTransform.position, camTransform.forward, 
                out RaycastHit hit, 
                100, LayerMaskDefaults.Get(LMD.Environment)))
            {
                GameObject waypoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                waypoint.transform.position = hit.point;
                wayPoints.Add(waypoint.transform);
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
        }
        UpdateLine();
        
    }

    void UpdateCarPosition()
    {
        Vector3 dir = (wayPoints[currentWaypointTarget].position -
            currentCar.transform.position).normalized;
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
