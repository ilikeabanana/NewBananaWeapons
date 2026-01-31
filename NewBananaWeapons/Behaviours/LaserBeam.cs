using System.Collections;
using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    public GameObject chargeParticle;

    float charging = 0.0f;
    InputManager inman;
    CameraController cam;
    LineRenderer line;

    float damageDelay;

    void Awake()
    {
        inman = InputManager.Instance;
        cam = CameraController.Instance;
        line = gameObject.AddComponent<LineRenderer>();
        line.widthMultiplier = 3; 
    }

    // Update is called once per frame
    void Update()
    {
        damageDelay -= Time.deltaTime;

        if (inman.InputSource.Fire1.IsPressed)
        {
            charging += Time.deltaTime;
            if(charging > 1f)
            {
                if (Physics.SphereCast(cam.GetDefaultPos(), 3, 
                    cam.transform.forward, out RaycastHit hit, 80, LayerMaskDefaults.Get(LMD.EnemiesAndEnvironment)))
                {
                    if(hit.collider.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier eidd))
                    {
                        if(damageDelay <= 0)
                        {
                            eidd.eid.hitter = "beam";
                            eidd.eid.SimpleDamage(0.5f);
                            damageDelay = 0.26f;
                        }

                        
                    }

                    line.positionCount = 2;
                    line.SetPosition(0, cam.GetDefaultPos() + Vector3.down);
                    line.SetPosition(1, hit.point);
                }
                else
                {
                    line.positionCount = 2;
                    line.SetPosition(0, cam.GetDefaultPos() + Vector3.down);
                    line.SetPosition(1, cam.GetDefaultPos() + (cam.transform.forward * 10));
                }
            }
            else
            {
                line.positionCount = 0;
            } 
        }
        else
        {
            line.positionCount = 0;
            charging = 0;
        }
    }
}
