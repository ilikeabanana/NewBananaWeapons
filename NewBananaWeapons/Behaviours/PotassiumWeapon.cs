using NewBananaWeapons;
using System.Collections.Generic;
using UnityEngine;

public class PotassiumWeapon : MonoBehaviour
{
    public GameObject carPrefab;

    List<Transform> wayPoints = new List<Transform>();
    LineRenderer line;

    float curveThreshold = 8f;
    int curveSubdivisions = 5;
    float curveStrength = 4f;

    void Awake()
    {
        StartCoroutine(ShaderManager.ApplyShaderToGameObject(carPrefab));
    }
    void Update()
    {
        if (!GunControl.Instance.activated) return;

        Transform camTransform = CameraController.Instance.transform;

        // Add waypoint
        if (InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame)
        {
            if (Physics.Raycast(camTransform.position, camTransform.forward,
                out RaycastHit hit,
                100, LayerMaskDefaults.Get(LMD.Environment)))
            {
                AddWaypoint(hit.point);
            }
        }

        // Spawn car
        if (InputManager.Instance.InputSource.Fire2.WasPerformedThisFrame)
        {
            if (wayPoints.Count == 0) return;

            GameObject car = Instantiate(carPrefab);
            car.transform.position = wayPoints[0].position;

            // Give car the path
            CarPathFollower follower = car.GetComponent<CarPathFollower>();
            follower.SetPath(wayPoints);

            // Clear local list so new path can be drawn
            wayPoints = new List<Transform>();
        }

        UpdateLine();
    }

    void AddWaypoint(Vector3 newPos)
    {
        if (wayPoints.Count == 0)
        {
            CreateWaypoint(newPos);
            return;
        }

        Vector3 prevPos = wayPoints[wayPoints.Count - 1].position;

        Vector2 prevXZ = new Vector2(prevPos.x, prevPos.z);
        Vector2 newXZ = new Vector2(newPos.x, newPos.z);

        float xzDistance = Vector2.Distance(prevXZ, newXZ);

        if (xzDistance < curveThreshold)
        {
            CreateWaypoint(newPos);
            return;
        }

        Vector3 mid = (prevPos + newPos) * 0.5f;
        Vector3 dir = (newPos - prevPos).normalized;
        Vector3 side = new Vector3(-dir.z, 0f, dir.x);
        mid += side * curveStrength;

        for (int i = 1; i <= curveSubdivisions; i++)
        {
            float t = i / (float)(curveSubdivisions + 1);
            Vector3 p =
                Mathf.Pow(1 - t, 2) * prevPos +
                2 * (1 - t) * t * mid +
                Mathf.Pow(t, 2) * newPos;

            CreateWaypoint(p);
        }

        CreateWaypoint(newPos);
    }

    void CreateWaypoint(Vector3 pos)
    {
        GameObject waypoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        waypoint.transform.position = pos;
        wayPoints.Add(waypoint.transform);
    }

    void UpdateLine()
    {
        if (line == null)
            CreateLine();

        if (wayPoints.Count == 0)
        {
            line.positionCount = 0;
            return;
        }

        line.positionCount = wayPoints.Count;
        for (int i = 0; i < wayPoints.Count; i++)
            line.SetPosition(i, wayPoints[i].position);
    }

    void CreateLine()
    {
        GameObject lineobject = new GameObject("Pathway");
        line = lineobject.AddComponent<LineRenderer>();
    }
}
