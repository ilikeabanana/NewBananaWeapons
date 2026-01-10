using System.Collections;
using UnityEngine;

public class TheForceWeapon : MonoBehaviour
{
    Animator anim;
    EnemyIdentifier currentTarget;
    Vector3 lastCamForward;
    Vector3 accumulatedThrow;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (currentTarget != null && currentTarget.dead)
            currentTarget = null;

        Transform camTransform = CameraController.Instance.transform;


        if (InputManager.Instance.InputSource.Fire1.IsPressed)
        {
            if(currentTarget != null)
            {
                if (!currentTarget.bigEnemy)
                {
                    Vector3 targetPosition;

                    if (Physics.Raycast(camTransform.position, camTransform.forward, out RaycastHit hit,
                        25, LayerMaskDefaults.Get(LMD.Environment)))
                    {
                        targetPosition = hit.point;
                    }
                    else
                    {
                        targetPosition = CameraController.Instance.transform.position +
                              CameraController.Instance.transform.forward * 25;
                    }
                    currentTarget.transform.position = targetPosition;
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

        }

        if (InputManager.Instance.InputSource.Fire2.WasPerformedThisFrame)
        {
            if(currentTarget != null)
            {
                if(currentTarget.bigEnemy)
                {
                    currentTarget.hitter = "implosion";
                    currentTarget.SimpleDamage(5);
                }
                else
                {
                    StyleHUD.Instance.AddPoints(100, "Imploded", gameObject);
                    currentTarget.Explode(false);
                }
                    
            }
        }
    }
}