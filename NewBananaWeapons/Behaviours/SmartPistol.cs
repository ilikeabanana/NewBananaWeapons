using NewBananaWeapons;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class SmartPistol : MonoBehaviour
{
    public List<EnemyIdentifier> targets = new List<EnemyIdentifier>();
    public Dictionary<EnemyIdentifier, List<LineRenderObject>> lRend = new Dictionary<EnemyIdentifier, List<LineRenderObject>>();
    public Transform firePoint;
    public TMP_Text targetCountDisplay;
    public AudioClip shootSound;

    Animator anim;

    public class LineRenderObject
    {
        public LineRenderer line;
        public float bezierCurveAmount;

        public Vector3 direction;
        public LineRenderObject(LineRenderer lr, float bezierAmount)
        {
            line = lr;
            bezierCurveAmount = bezierAmount;

            direction = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)
            ).normalized;
            
        }
    }

    float targetingDelay = 0.1f;
    float t = 0f;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (!GunControl.Instance.activated) return;
        Transform camTrans = CameraController.Instance.transform;

        targetCountDisplay.text = "Targets: \n" + targets.Count;

        // === Targeting ===
        if (InputManager.Instance.InputSource.Fire2.IsPressed && targets.Count < 12)
        {
            t -= Time.deltaTime;
            if (t <= 0f)
            {
                t = targetingDelay;
                RaycastHit[] hits = Physics.SphereCastAll(camTrans.position, 10f, camTrans.forward, 100f, LayerMaskDefaults.Get(LMD.Enemies));
                foreach(RaycastHit hit in hits)
                {
                    if (hit.collider.TryGetComponent(out EnemyIdentifierIdentifier eidd))
                    {
                        EnemyIdentifier eid = eidd.eid;
                        if (eid.dead) return;
                        // Always add target (duplicates allowed)
                        targets.Add(eid);

                        // Only create a line if this enemy doesn't have one yet
                        GameObject lineObj = new GameObject("SmartPistolLine");
                        lineObj.transform.SetParent(transform);
                        lineObj.layer = gameObject.layer;

                        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
                        lr.positionCount = 2;
                        lr.startWidth = 0.03f;
                        lr.endWidth = 0.03f;
                        lr.useWorldSpace = true;
                        lr.material = AddressableManager.lineMat;
                        lr.startColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                        lr.endColor = lr.startColor;
                        
                        LineRenderObject lro = new LineRenderObject(lr, Random.Range(0, 4f));
                        if (!lRend.ContainsKey(eid))
                        {
                            lRend.Add(eid, new List<LineRenderObject>()
                            {
                                lro
                            });
                        }
                        else
                        {
                            lRend[eid].Add(lro);
                        }
                        break;
                    }
                }
            }
        }

        // === Update lines ===
        List<EnemyIdentifier> dead = null;

        foreach (var pair in lRend)
        {
            EnemyIdentifier enemy = pair.Key;
            List<LineRenderObject> lrs = pair.Value;

            if (enemy == null || enemy.transform == null)
            {
                dead = new List<EnemyIdentifier>();
                dead.Add(enemy);
                continue;
            }
            foreach (var lr in lrs)
            {
                int segments = 20;

                lr.line.positionCount = segments;

                Vector3 start = firePoint.position;
                Vector3 end = enemy.transform.position;

                if (enemy.weakPoint) end = enemy.weakPoint.transform.position;

                Vector3 midpoint = (start + end) / 2f;
                Vector3 controlPoint = midpoint + (lr.direction * lr.bezierCurveAmount);

                for (int i = 0; i < segments; i++)
                {
                    // t goes from 0 (start) to 1 (end)
                    float t = i / (float)(segments - 1);

                    float u = 1 - t;
                    Vector3 curvePosition = (u * u * start) + (2 * u * t * controlPoint) + (t * t * end);

                    lr.line.SetPosition(i, curvePosition);
                }
            }


        }

        // remove dead ones
        if (dead != null)
        {
            foreach (var d in dead)
            {
                if (lRend.TryGetValue(d, out var lr))
                {
                    foreach (var line in lr)
                    {
                        Destroy(line.line.gameObject);
                    }
                }

                lRend.Remove(d);
                targets.Remove(d);
            }
        }

        // === Fire ===
        if (InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame && targets.Count > 0)
        {
            StartCoroutine(FireBullets());
        }
    }

    void OnDisable()
    {
        StopAllCoroutines();

        foreach (var lr in lRend.Values)
            if (lr != null)
            {
                foreach (var line in lr)
                {
                    Destroy(line.line.gameObject);
                }
            }

        lRend.Clear();
        targets.Clear();
    }

    IEnumerator FireBullets()
    {
        Transform camTrans = CameraController.Instance.transform;

        // iterate over copy because we're modifying targets
        foreach (var target in new List<EnemyIdentifier>(targets))
        {
            if (target == null)
            {
                targets.Remove(target);
                continue;
            }
            anim.SetFloat("RandomChance", Random.Range(0f, 1f));
            GameObject beam = Instantiate(AddressableManager.normalBeam, firePoint.position, Quaternion.identity);
            anim.SetTrigger("Shoot");
            GetComponentInParent<AudioSource>().PlayOneShot(shootSound);
            Transform targetPoint = target.transform;
            if (target.weakPoint != null)
                targetPoint = target.weakPoint.transform;

            beam.transform.LookAt(targetPoint);

            // consume one lock
            targets.Remove(target);
            if (lRend.TryGetValue(target, out var lr))
            {
                LineRenderObject randomLine = lr[Random.Range(0, lr.Count)];
                Destroy(randomLine.line.gameObject);

                lr.Remove(randomLine);
            }


            // if no more locks on this enemy remove its line
            if (!targets.Contains(target))
            {
                if (lRend.TryGetValue(target, out var lrr))
                {
                    foreach (var line in lrr)
                    {
                        Destroy(line.line.gameObject);
                    }

                    lRend.Remove(target);
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

}
