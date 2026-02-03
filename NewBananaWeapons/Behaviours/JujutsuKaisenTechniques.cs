using NewBananaWeapons;
using UnityEngine;

public class JujutsuKaisenTechniques : MonoBehaviour
{
    public Transform blueCharge;
    public Transform redCharge;
    public Transform purpleCharge;

    public GameObject blueProjectile;
    public GameObject redProjectile;
    public GameObject purpleProjectile;

    public float chargeTime = 1f;

    bool chargingBlue;
    bool chargingRed;
    bool chargingPurple;

    float blueTimer;
    float redTimer;

    [Header("Charge Visuals")]
    public float maxChargeScale = 1.5f;

    Vector3 blueStartScale;
    Vector3 redStartScale;
    Vector3 purpleStartScale;

    Vector3 blueStartPos;
    Vector3 redStartPos;
    Vector3 purpleStartPos;

    InputManager inman;
    AudioSource source;

    void Awake()
    {
        inman = InputManager.Instance;
        source = GetComponent<AudioSource>();

        blueCharge.gameObject.SetActive(false);
        redCharge.gameObject.SetActive(false);
        purpleCharge.gameObject.SetActive(false);

        // ✅ CORRECT: scale vs position
        blueStartScale = blueCharge.localScale;
        redStartScale = redCharge.localScale;
        purpleStartScale = purpleCharge.localScale;

        blueStartPos = blueCharge.localPosition;
        redStartPos = redCharge.localPosition;
        purpleStartPos = purpleCharge.localPosition;
    }

    void Update()
    {
        if (!GunControl.Instance.activated) return;
        if (Banana_WeaponsPlugin.cooldowns.ContainsKey(gameObject)) return;

        HandleCharging();
        HandleRelease();
        HandleVisuals();
    }

    void HandleCharging()
    {
        // BLUE
        if (inman.InputSource.Fire1.IsPressed && !chargingPurple)
        {
            blueTimer += Time.deltaTime;
            if (blueTimer >= chargeTime)
                chargingBlue = true;
        }

        // RED
        if (inman.InputSource.Fire2.IsPressed && !chargingPurple)
        {
            redTimer += Time.deltaTime;
            if (redTimer >= chargeTime)
                chargingRed = true;
        }

        // PURPLE — latch immediately once both are charged
        if (chargingBlue && chargingRed)
        {
            chargingPurple = true;
        }
    }

    void HandleRelease()
    {
        if (inman.InputSource.Fire1.WasCanceledThisFrame ||
            inman.InputSource.Fire2.WasCanceledThisFrame)
        {
            Vector3 pos = CameraController.Instance.transform.position;
            Quaternion rot = CameraController.Instance.transform.rotation;

            if (chargingPurple)
            {
                Instantiate(purpleProjectile, pos, rot);
            }
            else if (chargingBlue)
            {
                Instantiate(blueProjectile, pos, rot);
            }
            else if (chargingRed)
            {
                Instantiate(redProjectile, pos, rot);
            }

            ResetAll();
        }
    }

    void HandleVisuals()
    {
        // BLUE
        if (inman.InputSource.Fire1.IsPressed)
        {
            blueCharge.gameObject.SetActive(true);
            float t = Mathf.Clamp01(blueTimer / chargeTime);
            blueCharge.localScale = Vector3.Lerp(
                blueStartScale,
                blueStartScale * maxChargeScale,
                t
            );
        }
        else
        {
            blueCharge.gameObject.SetActive(false);
            blueCharge.localScale = blueStartScale;
            blueCharge.localPosition = blueStartPos;
        }

        // RED
        if (inman.InputSource.Fire2.IsPressed)
        {
            redCharge.gameObject.SetActive(true);
            float t = Mathf.Clamp01(redTimer / chargeTime);
            redCharge.localScale = Vector3.Lerp(
                redStartScale,
                redStartScale * maxChargeScale,
                t
            );
        }
        else
        {
            redCharge.gameObject.SetActive(false);
            redCharge.localScale = redStartScale;
            redCharge.localPosition = redStartPos;
        }

        // PURPLE
        if (chargingPurple)
        {
            purpleCharge.gameObject.SetActive(true);
            purpleCharge.localScale = Vector3.Lerp(
                purpleStartScale,
                purpleStartScale * (maxChargeScale * 2.5f),
                Time.deltaTime * 10f
            );

            // Visual merge
            blueCharge.position = Vector3.Lerp(
                blueCharge.position,
                purpleCharge.position,
                Time.deltaTime * 8f
            );

            redCharge.position = Vector3.Lerp(
                redCharge.position,
                purpleCharge.position,
                Time.deltaTime * 8f
            );
        }
        else
        {
            purpleCharge.gameObject.SetActive(false);
            purpleCharge.localScale = purpleStartScale;
            purpleCharge.localPosition = purpleStartPos;
        }
    }

    void ResetAll()
    {
        chargingBlue = chargingRed = chargingPurple = false;
        blueTimer = redTimer = 0f;

        blueCharge.gameObject.SetActive(false);
        redCharge.gameObject.SetActive(false);
        purpleCharge.gameObject.SetActive(false);

        blueCharge.localScale = blueStartScale;
        redCharge.localScale = redStartScale;
        purpleCharge.localScale = purpleStartScale;

        blueCharge.localPosition = blueStartPos;
        redCharge.localPosition = redStartPos;
        purpleCharge.localPosition = purpleStartPos;
    }
}
