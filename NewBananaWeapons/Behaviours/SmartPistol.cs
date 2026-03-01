using NewBananaWeapons;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SmartPistol : BaseWeapon
{
    public List<EnemyIdentifier> targets = new List<EnemyIdentifier>();
    public Dictionary<EnemyIdentifier, List<LineRenderObject>> lRend = new Dictionary<EnemyIdentifier, List<LineRenderObject>>();
    public Transform firePoint;
    public TMP_Text targetCountDisplay;
    public AudioClip shootSound;

    public override string GetWeaponDescription()
    {
        return "Right click selects targets, left click to fire to all targets";
    }
    static ConfigVar<int> maxTargets;
    public override void SetupConfigs(string sectionName)
    {
        maxTargets = new ConfigVar<int>(sectionName, "Max Targets", 12);
        base.SetupConfigs(sectionName);
    }

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
            direction = Random.onUnitSphere;
        }
    }

    float targetingDelay = 0.1f;
    float t = 0f;

    // cone targeting tuning
    const float coneRadius = 0.75f;
    const float maxDistance = 60f;
    const float minDot = 0.94f;
    // 0.94 ≈ ~20° cone. Raise for stricter aim.

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (!GunControl.Instance.activated) return;

        Transform camTrans = CameraController.Instance.transform;
        targetCountDisplay.text = "Targets:\n" + targets.Count;

        // === TARGETING ===
        if (InputManager.Instance.InputSource.Fire2.IsPressed && targets.Count < maxTargets.Value)
        {
            t -= Time.deltaTime;
            if (t <= 0f)
            {
                t = targetingDelay;

                EnemyIdentifier bestTarget = null;
                float bestDot = minDot;

                // small forgiving spherecast forward
                RaycastHit[] hits = Physics.SphereCastAll(
                    camTrans.position,
                    coneRadius,
                    camTrans.forward,
                    maxDistance,
                    LayerMaskDefaults.Get(LMD.Enemies)
                );

                foreach (var hit in hits)
                {
                    if (!hit.collider.TryGetComponent(out EnemyIdentifierIdentifier eidd))
                        continue;

                    EnemyIdentifier eid = eidd.eid;
                    if (eid.dead) continue;

                    Vector3 toEnemy = (eid.transform.position - camTrans.position).normalized;
                    float dot = Vector3.Dot(camTrans.forward, toEnemy);

                    if (dot > bestDot)
                    {
                        bestDot = dot;
                        bestTarget = eid;
                    }
                }

                if (bestTarget != null)
                {
                    AddTarget(bestTarget);
                }
            }
        }

        // === UPDATE LINES ===
        List<EnemyIdentifier> dead = null;

        foreach (var pair in lRend)
        {
            EnemyIdentifier enemy = pair.Key;
            List<LineRenderObject> lrs = pair.Value;

            if (enemy == null || enemy.transform == null)
            {
                if (dead == null) dead = new List<EnemyIdentifier>();
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

                Vector3 midpoint = (start + end) * 0.5f;
                Vector3 controlPoint = midpoint + (lr.direction * lr.bezierCurveAmount);

                for (int i = 0; i < segments; i++)
                {
                    float tt = i / (float)(segments - 1);
                    float u = 1 - tt;
                    Vector3 curvePos =
                        (u * u * start) +
                        (2 * u * tt * controlPoint) +
                        (tt * tt * end);

                    lr.line.SetPosition(i, curvePos);
                }
            }
        }

        // === REMOVE DEAD TARGETS ===
        if (dead != null)
        {
            foreach (var d in dead)
            {
                if (lRend.TryGetValue(d, out var lr))
                {
                    foreach (var line in lr)
                        Destroy(line.line.gameObject);
                }

                lRend.Remove(d);
                targets.Remove(d);
            }
        }

        // === FIRE ===
        if (InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame && targets.Count > 0)
        {
            StartCoroutine(FireBullets());
        }
    }

    void AddTarget(EnemyIdentifier eid)
    {
        // allow multiple locks on same enemy
        targets.Add(eid);

        GameObject lineObj = new GameObject("SmartPistolLine");
        lineObj.transform.SetParent(transform);
        lineObj.layer = gameObject.layer;

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.startWidth = 0.03f;
        lr.endWidth = 0.03f;
        lr.useWorldSpace = true;
        lr.material = AddressableManager.lineMat;
        lr.startColor = Random.ColorHSV();
        lr.endColor = lr.startColor;

        LineRenderObject lro = new LineRenderObject(lr, Random.Range(0f, 4f));

        if (!lRend.ContainsKey(eid))
            lRend.Add(eid, new List<LineRenderObject> { lro });
        else
            lRend[eid].Add(lro);
    }

    void OnDisable()
    {
        StopAllCoroutines();

        foreach (var lr in lRend.Values)
            foreach (var line in lr)
                Destroy(line.line.gameObject);

        lRend.Clear();
        targets.Clear();
    }

    IEnumerator FireBullets()
    {
        foreach (var target in new List<EnemyIdentifier>(targets))
        {
            if (target == null)
            {
                targets.Remove(target);
                continue;
            }

            anim.SetFloat("RandomChance", Random.Range(0f, 1f));
            anim.SetTrigger("Shoot");
            GetComponentInParent<AudioSource>().pitch = Random.Range(0.9f, 1.1f);
            GetComponentInParent<AudioSource>().PlayOneShot(shootSound);

            GameObject beam = Instantiate(AddressableManager.normalBeam, firePoint.position, Quaternion.identity);

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

                if (lr.Count == 0)
                    lRend.Remove(target);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}
