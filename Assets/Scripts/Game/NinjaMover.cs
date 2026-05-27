using UnityEngine;

public class NinjaMover : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Transform target;
    [SerializeField] private float speed = 1.2f;
    [SerializeField] private float stopDistance = 6f;

    [Header("Attack")]
    [SerializeField] private float attackInterval = 0.5f;
    [SerializeField] private int towerDamage = 40;

    [Header("Audio")]
    [SerializeField] private AudioSource hitAudioSource;
    [SerializeField] private AudioClip towerHitSound;

    private float attackTimer;
    private TempleHealth templeHealth;

    public void SetTarget(Transform t)
    {
        target = t;

        if (target != null)
        {
            templeHealth = target.GetComponent<TempleHealth>();

            if (templeHealth == null)
                templeHealth = target.GetComponentInParent<TempleHealth>();

            if (templeHealth == null)
                templeHealth = target.GetComponentInChildren<TempleHealth>();
        }
    }

    public void SetSpeed(float s)
    {
        speed = Mathf.Max(0f, s);
    }

    private void Start()
    {
        if (target != null && templeHealth == null)
        {
            templeHealth = target.GetComponent<TempleHealth>();

            if (templeHealth == null)
                templeHealth = target.GetComponentInParent<TempleHealth>();

            if (templeHealth == null)
                templeHealth = target.GetComponentInChildren<TempleHealth>();
        }
    }

    private void Update()
    {
        if (target == null)
        {
            Debug.LogWarning(gameObject.name + " has no target assigned.");
            return;
        }

        Vector3 pos = transform.position;
        Vector3 goal = target.position;

        goal.y = pos.y;

        Vector3 delta = goal - pos;
        float distance = delta.magnitude;

        if (distance <= stopDistance)
        {
            attackTimer += Time.deltaTime;

            if (attackTimer >= attackInterval)
            {
                attackTimer = 0f;
                AttackTower();
            }

            return;
        }

        Vector3 dir = delta.normalized;
        transform.position = pos + dir * speed * Time.deltaTime;

        if (dir.sqrMagnitude > 0.01f)
        {
            transform.forward = Vector3.Slerp(transform.forward, dir, 10f * Time.deltaTime);
        }
    }

    private void AttackTower()
    {
        Debug.Log("Ninja attacks tower");

        if (templeHealth == null)
        {
            Debug.LogError("Ninja cannot damage tower because TempleHealth was not found.");
            return;
        }

        templeHealth.TakeDamage(towerDamage);

        if (hitAudioSource != null && towerHitSound != null)
        {
            hitAudioSource.pitch = Random.Range(0.9f, 1.1f);
            hitAudioSource.PlayOneShot(towerHitSound);
        }
    }
}