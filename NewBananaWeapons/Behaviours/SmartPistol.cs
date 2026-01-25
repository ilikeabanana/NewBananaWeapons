using NewBananaWeapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartPistol : MonoBehaviour
{
    public List<EnemyIdentifier> targets = new List<EnemyIdentifier>();
    public Dictionary<EnemyIdentifier, LineRenderer> lRend = new Dictionary<EnemyIdentifier, LineRenderer>();

    float targetingDelay = 0.2f;
    float t = 0f;

    void Update()
    {
        Transform camTrans = CameraController.Instance.transform;

        // === Targeting ===
        if (InputManager.Instance.InputSource.Fire2.IsPressed && targets.Count < 12)
        {
            t -= Time.deltaTime;
            if (t <= 0f)
            {
                t = targetingDelay;

                if (Physics.SphereCast(camTrans.position, 2f, camTrans.forward,
                    out RaycastHit hit, 100f, LayerMaskDefaults.Get(LMD.Enemies)))
                {
                    if (hit.collider.TryGetComponent(out EnemyIdentifierIdentifier eidd))
                    {
                        EnemyIdentifier eid = eidd.eid;

                        // Always add target (duplicates allowed)
                        targets.Add(eid);

                        // Only create a line if this enemy doesn't have one yet
                        if (!lRend.ContainsKey(eid))
                        {
                            GameObject lineObj = new GameObject("SmartPistolLine");
                            lineObj.transform.SetParent(transform);

                            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
                            lr.positionCount = 2;
                            lr.startWidth = 0.03f;
                            lr.endWidth = 0.03f;
                            lr.useWorldSpace = true;

                            lRend.Add(eid, lr);
                        }

                    }
                }
            }
        }

        // === Update lines ===
        List<EnemyIdentifier> dead = null;

        foreach (var pair in lRend)
        {
            EnemyIdentifier enemy = pair.Key;
            LineRenderer lr = pair.Value;

            if (enemy == null || enemy.transform == null)
            {
                dead = new List<EnemyIdentifier>();
                dead.Add(enemy);
                continue;
            }

            lr.SetPosition(0, camTrans.position + (camTrans.forward * 3));
            lr.SetPosition(1, enemy.transform.position);
        }

        // remove dead ones
        if (dead != null)
        {
            foreach (var d in dead)
            {
                if (lRend.TryGetValue(d, out var lr))
                    Destroy(lr.gameObject);

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
            if (lr != null) Destroy(lr.gameObject);

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

            GameObject beam = Instantiate(AddressableManager.normalBeam, camTrans.position, Quaternion.identity);

            Transform targetPoint = target.transform;
            if (target.weakPoint != null)
                targetPoint = target.weakPoint.transform;

            beam.transform.LookAt(targetPoint);

            // consume one lock
            targets.Remove(target);

            // if no more locks on this enemy → remove its line
            if (!targets.Contains(target))
            {
                if (lRend.TryGetValue(target, out var lr))
                {
                    Destroy(lr.gameObject);
                    lRend.Remove(target);
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

}
