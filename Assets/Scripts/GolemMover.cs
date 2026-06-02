using UnityEngine;

public class GolemMover : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Transform target;
    [SerializeField] private float speed = 0.5f;
    [SerializeField] private float stopDistance = 6f;

    [Header("Attack")]
    [SerializeField] private float attackInterval = 1.5f;
    [SerializeField] private int towerDamage = 80;

    [Header("Step Audio")]
    [SerializeField] private AudioSource stepAudioSource;
    [SerializeField] private AudioClip stepSound;
    [SerializeField] private float stepInterval = 0.9f;
    [SerializeField] private Vector2 stepPitchRange = new Vector2(0.85f, 1.05f);
    [SerializeField] private float minSpeedForSteps = 0.05f;

    [Header("Hit Audio")]
    [SerializeField] private AudioSource hitAudioSource;
    [SerializeField] private AudioClip towerHitSound;

    private float attackTimer;
    private float stepTimer;
    private TempleHealth templeHealth;

    private void Awake()
    {
        if (stepAudioSource == null)
            stepAudioSource = GetComponent<AudioSource>();

        if (hitAudioSource == null)
            hitAudioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        FindTempleHealth();
    }

    private void Update()
    {
        if (target == null)
            return;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance > stopDistance)
        {
            MoveTowardsTarget(direction);
            HandleStepSound();
        }
        else
        {
            AttackTower();
        }
    }

    private void MoveTowardsTarget(Vector3 direction)
    {
        if (direction.sqrMagnitude <= 0.001f)
            return;

        Vector3 moveDirection = direction.normalized;

        transform.position += moveDirection * speed * Time.deltaTime;

        Quaternion lookRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 8f * Time.deltaTime);
    }

    private void HandleStepSound()
    {
        if (speed < minSpeedForSteps)
            return;

        if (stepAudioSource == null || stepSound == null)
            return;

        stepTimer += Time.deltaTime;

        if (stepTimer < stepInterval)
            return;

        stepTimer = 0f;

        stepAudioSource.pitch = Random.Range(stepPitchRange.x, stepPitchRange.y);
        stepAudioSource.PlayOneShot(stepSound);
    }

    private void AttackTower()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer < attackInterval)
            return;

        attackTimer = 0f;

        if (templeHealth == null)
            FindTempleHealth();

        if (templeHealth != null)
        {
            templeHealth.TakeDamage(towerDamage);
        }

        if (hitAudioSource != null && towerHitSound != null)
        {
            hitAudioSource.PlayOneShot(towerHitSound);
        }
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