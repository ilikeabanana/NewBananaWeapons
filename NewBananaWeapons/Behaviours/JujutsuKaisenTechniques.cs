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
    float purpleTimer;

    [Header("Charge Visuals")]
    public float maxChargeScale = 1.5f;
    public float scaleSpeed = 2f;
    public float spiralRadius = 0.2f;
    public float spiralSpeed = 6f;

    Vector3 blueStartScale;
    Vector3 redStartScale;
    Vector3 purpleStartScale;

    float spiralTime;


    InputManager inman;
    AudioSource source;

    void Awake()
    {
        inman = InputManager.Instance;
        source = GetComponent<AudioSource>();

        // Disable visuals at start
        blueCharge.gameObject.SetActive(false);
        redCharge.gameObject.SetActive(false);

        purpleCharge.gameObject.SetActive(false);

        blueStartScale = blueCharge.localScale;
        redStartScale = redCharge.localScale;
        purpleStartScale = purpleCharge.localScale;

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
            chargingBlue = true;
            blueTimer += Time.deltaTime;
        }
        else
        {
            chargingBlue = false;
            blueTimer = 0f;
        }

        // RED
        if (inman.InputSource.Fire2.IsPressed && !chargingPurple)
        {
            chargingRed = true;
            redTimer += Time.deltaTime;
        }
        else
        {
            chargingRed = false;
            redTimer = 0f;
        }

        // PURPLE (both held for 1 second)
        if (chargingBlue && chargingRed && blueTimer >= chargeTime && redTimer >= chargeTime)
        {
            chargingPurple = true;
            purpleTimer += Time.deltaTime;
        }
        else
        {
            chargingPurple = false;
            purpleTimer = 0f;
        }
    }

    void HandleRelease()
    {
        if (inman.InputSource.Fire1.WasCanceledThisFrame || inman.InputSource.Fire2.WasCanceledThisFrame)
        {
            Vector3 pos = CameraController.Instance.transform.position;
            Quaternion rot = CameraController.Instance.transform.rotation;

            if (chargingPurple && purpleTimer >= chargeTime)
            {
                Instantiate(purpleProjectile, pos, rot).transform.localScale *= 10f;
            }
            else if (chargingBlue && blueTimer >= chargeTime)
            {
                Instantiate(blueProjectile, pos, rot);
            }
            else if (chargingRed && redTimer >= chargeTime)
            {
                Instantiate(redProjectile, pos, rot);
            }

            ResetAll();
        }
    }

    void HandleVisuals()
    {
        spiralTime += Time.deltaTime;

        // BLUE
        if (chargingBlue)
        {
            blueCharge.gameObject.SetActive(true);

            float t = Mathf.Clamp01(blueTimer / chargeTime);
            blueCharge.localScale = Vector3.Lerp(
                blueStartScale,
                blueStartScale * maxChargeScale,
                t
            );

            Vector3 spiralOffset = new Vector3(
                Mathf.Cos(spiralTime * spiralSpeed),
                Mathf.Sin(spiralTime * spiralSpeed),
                0f
            ) * spiralRadius;

            blueCharge.localPosition = spiralOffset;
        }
        else
        {
            blueCharge.gameObject.SetActive(false);
            blueCharge.localScale = blueStartScale;
        }

        // RED
        if (chargingRed)
        {
            redCharge.gameObject.SetActive(true);

            float t = Mathf.Clamp01(redTimer / chargeTime);
            redCharge.localScale = Vector3.Lerp(
                redStartScale,
                redStartScale * maxChargeScale,
                t
            );

            Vector3 spiralOffset = new Vector3(
                Mathf.Cos(-spiralTime * spiralSpeed),
                Mathf.Sin(-spiralTime * spiralSpeed),
                0f
            ) * spiralRadius;

            redCharge.localPosition = spiralOffset;
        }
        else
        {
            redCharge.gameObject.SetActive(false);
            redCharge.localScale = redStartScale;
        }

        // PURPLE (COMBINATION)
        if (chargingPurple)
        {
            purpleCharge.gameObject.SetActive(true);

            float t = Mathf.Clamp01(purpleTimer / chargeTime);
            purpleCharge.localScale = Vector3.Lerp(
                purpleStartScale,
                purpleStartScale * (maxChargeScale * 1.5f),
                t
            );

            // Pull blue & red inward while spiraling
            blueCharge.position = Vector3.Lerp(
                blueCharge.position,
                purpleCharge.position,
                Time.deltaTime * 6f
            );

            redCharge.position = Vector3.Lerp(
                redCharge.position,
                purpleCharge.position,
                Time.deltaTime * 6f
            );
        }
        else
        {
            purpleCharge.gameObject.SetActive(false);
            purpleCharge.localScale = purpleStartScale;
        }
    }

    void ResetAll()
    {
        chargingBlue = chargingRed = chargingPurple = false;
        blueTimer = redTimer = purpleTimer = 0f;

        blueCharge.gameObject.SetActive(false);
        redCharge.gameObject.SetActive(false);
        purpleCharge.gameObject.SetActive(false);
    }
}
