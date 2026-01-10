using System.Collections;
using UnityEngine;


public class GambleGun : MonoBehaviour
{
    public GameObject[] slots;

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
            speeds[i] = Random.Range(0.0f, 3.0f);
        }

        float timer = 0;
        while (timer < slots.Length)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].transform.Rotate(new Vector3(0, 0, 2 * speeds[i]));
            }

            timer += Time.deltaTime;
            yield return null;
        }

        foreach (var slot in slots)
        {
            var t = slot.transform;

            float z = t.localEulerAngles.z;

            float snappedZ = Mathf.Round(z / 72f) * 72f;
            t.localEulerAngles = new Vector3(
                t.localEulerAngles.x,
                t.localEulerAngles.y,
                snappedZ
            );
        }

    }
}
