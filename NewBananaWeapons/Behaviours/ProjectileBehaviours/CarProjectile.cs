using UnityEngine;

public class CarProjectile : MonoBehaviour
{
    [HideInInspector] public Transform target;
    public float timeBeforeHittingDavid = 3.5f;

    private Vector3 direction;
    private float speed;
    private float timer;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("CarProjectile has no target!");
            enabled = false;
            return;
        }

        Vector3 startPos = transform.position;
        Vector3 targetPos = target.position;

        Vector3 toTarget = targetPos - startPos;
        float distance = toTarget.magnitude;

        direction = toTarget.normalized;

        // Constant speed so we reach target exactly at given time
        speed = distance / timeBeforeHittingDavid;
        transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, 90f, 0f);


    }

    void Update()
    {
        timer += Time.deltaTime;

        // Move at constant velocity forever
        transform.position += direction * speed * Time.deltaTime;

        Collider[] hitCols = Physics.OverlapSphere(transform.position, 25);
        if(hitCols.Length > 0)
        {
            foreach (var col in hitCols)
            {
                if (col.gameObject.TryGetComponent<EnemyIdentifierIdentifier>(out EnemyIdentifierIdentifier enemyHit))
                {
                    enemyHit.eid.hitter = "car";
                    enemyHit.eid.DeliverDamage(col.gameObject, CameraController.Instance.transform.forward * 20000 * 20, enemyHit.transform.position, 25, false);
                }

            }
        }
        if (timer >= timeBeforeHittingDavid)
        {
            Destroy(target.gameObject);
        }
    }
}
