using NewBananaWeapons;
using System.Collections;
using UnityEngine;

public class TheForceWeapon : MonoBehaviour
{
    Animator anim;
    EnemyIdentifier currentTarget;
    Vector3 lastCamForward;
    Vector3 accumulatedThrow;
    float distance = 25;
    Vector3 offset = Vector3.zero;

    float cooldown = 0;
    float cooldownOnCrush = 1.5f;

    GameObject manipulationEffectCurrent = null;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (currentTarget != null && currentTarget.dead)
            currentTarget = null;

        if(anim != null)
        { 
            if (anim.GetBool("Crush"))
            {
                anim.SetBool("Crush", false);
            }
            if(cooldown <= 0) 
                anim.SetBool("Holding", InputManager.Instance.InputSource.Fire1.IsPressed);
            else
                anim.SetBool("Holding", false);
        }

        if(cooldown > 0)
        {
            cooldown -= Time.deltaTime;
        }

        Transform camTransform = CameraController.Instance.transform;
        if (InputManager.Instance.InputSource.Fire1.IsPressed)
        {
            
            if (currentTarget != null)
            {
                if (!currentTarget.bigEnemy)
                {
                    Vector3 targetPosition;

                    if (Physics.Raycast(camTransform.position, camTransform.forward, out RaycastHit hit,
                        distance, LayerMaskDefaults.Get(LMD.Environment)))
                    {
                        targetPosition = hit.point;
                    }
                    else
                    {
                        targetPosition = camTransform.position +
                              camTransform.forward * distance;
                    }
                    currentTarget.transform.position = targetPosition + offset;
                    Vector3 camDelta = camTransform.forward - lastCamForward;

                    // Scale controls how strong the throw is
                    accumulatedThrow += camDelta * 40f;

                    lastCamForward = camTransform.forward;
                    
                }
                
            }
            else
            {
                if (Physics.Raycast(camTransform.position, camTransform.forward, out RaycastHit hit,
                    25, LayerMaskDefaults.Get(LMD.Enemies)))
                {
                    if(hit.collider.
                        gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier eidd))
                    {
                        EnemyIdentifier eid = eidd.eid;
                        currentTarget = eid;
                        lastCamForward = camTransform.forward;
                        accumulatedThrow = Vector3.zero;
                        distance = Vector3.Distance(camTransform.position, currentTarget.transform.position);
                        Vector3 target = camTransform.position +
                              camTransform.forward * distance;
                        offset = currentTarget.transform.position - target;
                        if(manipulationEffectCurrent != null)
                        {
                            Destroy(manipulationEffectCurrent);
                            manipulationEffectCurrent = null;
                        }
                        manipulationEffectCurrent = Instantiate(AddressableManager.manipulationEffect, currentTarget.transform);
                    }
                } 
            }
        }
        else
        {
            if (currentTarget != null && !currentTarget.bigEnemy)
            {
                currentTarget.rb.velocity = Vector3.zero;
                currentTarget.rb.useGravity = true;
                currentTarget.rb.AddForce(accumulatedThrow, ForceMode.VelocityChange);
            }
            currentTarget = null;
            if(manipulationEffectCurrent != null)
            {
                Destroy(manipulationEffectCurrent);
                manipulationEffectCurrent = null;
            }

        }

        if (InputManager.Instance.InputSource.Fire2.WasPerformedThisFrame)
        {
            if(currentTarget != null)
            {
                if (anim != null)
                    anim.SetBool("Crush", true);
                if (currentTarget.bigEnemy)
                {
                    currentTarget.hitter = "implosion";
                    currentTarget.SimpleDamage(5);
                }
                else
                {
                    StyleHUD.Instance.AddPoints(100, "Imploded", gameObject);
                    currentTarget.Explode(false);
                }
                if (manipulationEffectCurrent != null)
                {
                    Destroy(manipulationEffectCurrent);
                    manipulationEffectCurrent = null;
                }
                currentTarget = null;
                cooldown = cooldownOnCrush;
                    
            }
        }
    }
}