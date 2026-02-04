using System.Collections;
using UnityEngine;

public class MississipiQueen : MonoBehaviour
{
    public Renderer interiorMat;
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
        if (doingSequence)
        {
            TimeController.Instance.timeScaleModifier = 0;
        }

        anim.SetBool("Fire", false);
        if (InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame && !doingSequence)
        {
            doingSequence = true;
            anim.SetBool("Fire", true);
            missippiQueen.Play();

            // Stop time (Ultrakill-style)
            TimeController.Instance.timeScaleModifier = 0;
            //Time.timeScale = 0;
            TimeController.Instance.RestoreTime();
            anim.speed = 0.25f;

            AudioMixerController.Instance.SetMusicVolume(0);
        }
    }

    void OnDisable()
    {
        EndSequence();
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

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            interiorMat.material.SetFloat("_Opacity", Mathf.Lerp(from, to, t / fadeDuration));
            yield return null;
        }

        interiorMat.material.SetFloat("_Opacity", to);
    }

    // Called by animation event
    public void FireBullet()
    {
        if (firePoint == null)
            firePoint = CameraController.Instance.transform;

        Ray ray = new Ray(firePoint.position, firePoint.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, bulletRange, LayerMaskDefaults.Get(LMD.Enemies)))
        {
            // Try deal damage if target has health
            var health = hit.collider.GetComponent<EnemyIdentifierIdentifier>();
            if (health != null)
            {
                health.eid.SimpleDamage(bulletDamage);
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
        anim.speed = 1f;
        // Restore normal time
        TimeController.Instance.timeScaleModifier = 1f;
        TimeController.Instance.RestoreTime();
        AudioMixerController.Instance.SetMusicVolume(AudioMixerController.Instance.optionsMusicVolume);
    }
}
