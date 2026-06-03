using UnityEngine;

public class GolemMover : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Transform target;
    [SerializeField] private float speed = 0.5f;
    [SerializeField] private float stopDistance = 6f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string attackTriggerName = "Attack";

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
    private bool isAttacking;
    private TempleHealth templeHealth;

    private void Awake()
    {
        if (stepAudioSource == null)
            stepAudioSource = GetComponent<AudioSource>();

        if (hitAudioSource == null)
            hitAudioSource = GetComponent<AudioSource>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator != null)
            animator.applyRootMotion = false;
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

        if (distance <= stopDistance)
        {
            FaceTarget(direction);

            if (!isAttacking)
            {
                isAttacking = true;
                attackTimer = 0f;
                AttackTower();
            }
            else
            {
                HandleAttackTimer();
            }

            return;
        }

        isAttacking = false;

        MoveTowardsTarget(direction, distance);
        HandleStepSound();
    }

    private void MoveTowardsTarget(Vector3 direction, float distance)
    {
        if (direction.sqrMagnitude <= 0.001f)
            return;

        Vector3 moveDirection = direction.normalized;

        float moveAmount = speed * Time.deltaTime;
        float remainingDistanceBeforeStop = distance - stopDistance;

        moveAmount = Mathf.Min(moveAmount, remainingDistanceBeforeStop);

        transform.position += moveDirection * moveAmount;

        FaceTarget(direction);
    }

    private void FaceTarget(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        Vector3 moveDirection = direction.normalized;

        Quaternion lookRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 8f * Time.deltaTime);
    }

    private void HandleAttackTimer()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer >= attackInterval)
        {
            attackTimer = 0f;
            AttackTower();
        }
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
        if (animator != null)
        {
            animator.SetTrigger(attackTriggerName);
        }

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