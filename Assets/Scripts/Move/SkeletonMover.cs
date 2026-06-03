using UnityEngine;

public class SkeletonMover : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Transform target;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float stopDistance = 2f;

    [Header("Start Delay")]
    [SerializeField] private float moveStartDelay = 0.5f;

    [Header("Attack")]
    [SerializeField] private float attackInterval = 0.6f;
    [SerializeField] private int towerDamage = 5;

    [Header("Audio")]
    [SerializeField] private AudioSource hitAudioSource;
    [SerializeField] private AudioClip towerHitSound;
    [SerializeField] private Vector2 hitPitchRange = new Vector2(0.9f, 1.1f);

    private float attackTimer;
    private float moveDelayTimer;
    private TempleHealth templeHealth;

    private void Awake()
    {
        if (hitAudioSource == null)
        {
            hitAudioSource = GetComponent<AudioSource>();
        }
    }

    private void Start()
    {
        moveDelayTimer = moveStartDelay;
        FindTempleHealth();
    }

    private void Update()
    {
        if (target == null)
            return;

        if (moveDelayTimer > 0f)
        {
            moveDelayTimer -= Time.deltaTime;
            return;
        }

        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = target.position;
        targetPosition.y = currentPosition.y;

        Vector3 directionToTarget = targetPosition - currentPosition;
        float distance = directionToTarget.magnitude;

        if (distance <= stopDistance)
        {
            AttackTowerOverTime();
            return;
        }

        MoveTowardsTarget(directionToTarget);
    }

    private void MoveTowardsTarget(Vector3 directionToTarget)
    {
        if (directionToTarget.sqrMagnitude <= 0.001f)
            return;

        Vector3 moveDirection = directionToTarget.normalized;

        transform.position += moveDirection * speed * Time.deltaTime;

        Quaternion lookRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 10f * Time.deltaTime);
    }

    private void AttackTowerOverTime()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer < attackInterval)
            return;

        attackTimer = 0f;

        AttackTower();
    }

    private void AttackTower()
    {
        if (templeHealth == null)
        {
            FindTempleHealth();
        }

        if (templeHealth != null)
        {
            templeHealth.TakeDamage(towerDamage);
        }
        else
        {
            Debug.LogWarning(gameObject.name + " cannot damage tower because TempleHealth was not found.");
        }

        PlayHitSound();
    }

    private void PlayHitSound()
    {
        if (hitAudioSource == null || towerHitSound == null)
            return;

        hitAudioSource.pitch = Random.Range(hitPitchRange.x, hitPitchRange.y);
        hitAudioSource.PlayOneShot(towerHitSound);
    }

    private void FindTempleHealth()
    {
        if (target == null)
            return;

        templeHealth = target.GetComponent<TempleHealth>();

        if (templeHealth == null)
            templeHealth = target.GetComponentInParent<TempleHealth>();

        if (templeHealth == null)
            templeHealth = target.GetComponentInChildren<TempleHealth>();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        FindTempleHealth();
    }

    public void SetSpeed(float newSpeed)
    {
        speed = Mathf.Max(0f, newSpeed);
    }
}