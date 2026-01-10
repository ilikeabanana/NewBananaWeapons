using System.Collections;
using UnityEngine;


public class GambleGun : MonoBehaviour
{
    public Transform firePoint;
    Animator anim;
    public GameObject[] slots;

    public GameObject[] projectilesPerFireMode;

    // Each slot is 72° 
    // Filth = 72
    // Maurice = 144
    // Seven = 216
    // Coin = 288
    // Kitr = 0 or 360

    void Update()
    {
        if (InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame)
        {
            StartCoroutine(Spin());
        }
    }

    IEnumerator Spin()
    {
        float[] speeds = new float[slots.Length];
        for (int i = 0; i < speeds.Length; i++)
        {
            speeds[i] = Random.Range(1.0f, 3.0f);
        }

        float timer = 0;
        float duration = 1.5f;
        while (timer < duration)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].transform.Rotate(new Vector3(0, 0, 2 * speeds[i] * Time.deltaTime));
            }

            timer += Time.deltaTime;
            yield return null;
        }

        foreach (var slot in slots)
        {
            var t = slot.transform;

            float z = t.localEulerAngles.z;

            float snappedZ = Mathf.Round(z / 72f) * 72f % 360f;

            t.localEulerAngles = new Vector3(
                t.localEulerAngles.x,
                t.localEulerAngles.y,
                snappedZ
            );
        }
        Fire();
    }

    void Fire()
    {
        anim.SetTrigger("Fire");
        int fireMode = getFireMode();

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

        projectile.transform.LookAt(targetPoint, Vector3.up);
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
