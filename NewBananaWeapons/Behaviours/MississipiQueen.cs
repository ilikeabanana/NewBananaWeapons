using System.Collections;
using UnityEngine;

public class MississipiQueen : MonoBehaviour
{
    public Material interiorMat;
    float fadeDuration = 1f;
    float bulletRange = 100f;
    float bulletDamage = 10f;
    public Transform firePoint;

    AudioSource missippiQueen;

    Animator anim;
    bool doingSequence = false;

    void Awake()
    {
        anim = GetComponent<Animator>();
        missippiQueen = GetComponent<AudioSource>();
    }

    void Update()
    {
        anim.SetBool("Fire", false);
        if (InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame && !doingSequence)
        {
            doingSequence = true;
            anim.SetBool("Fire", true);
            missippiQueen.Play();

            // Stop time (Ultrakill-style)
            TimeController.Instance.timeScale = 0;
            TimeController.Instance.RestoreTime();

            // Play animator sequence if exists
            if (anim != null)
                anim.SetTrigger("StartSequence");
        }
    }

    // Called by animation event
    public void FadeInInterior()
    {
        StartCoroutine(FadeMaterial(0f, 1f));
    }

    // Called by animation event
    public void FadeOutInterior()
    {
        StartCoroutine(FadeMaterial(1f, 0f));
    }

    IEnumerator FadeMaterial(float from, float to)
    {
        if (interiorMat == null) yield break;

        float t = 0f;
        Color c = interiorMat.color;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(from, to, t / fadeDuration);
            interiorMat.color = c;
            yield return null;
        }

        c.a = to;
        interiorMat.color = c;
    }

    // Called by animation event
    public void FireBullet()
    {
        if (firePoint == null)
            firePoint = transform;

        Ray ray = new Ray(firePoint.position, firePoint.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, bulletRange))
        {
            // Try deal damage if target has health
            var health = hit.collider.GetComponent<EnemyIdentifier>();
            if (health != null)
            {
                health.SimpleDamage(bulletDamage);
            }

            // Debug impact
            Debug.DrawLine(ray.origin, hit.point, Color.red, 1f);
        }
    }

    // Called by animation event at end
    public void EndSequence()
    {
        doingSequence = false;
        missippiQueen.Stop();
        // Restore normal time
        TimeController.Instance.timeScale = 1f;
        TimeController.Instance.RestoreTime();
    }
}
