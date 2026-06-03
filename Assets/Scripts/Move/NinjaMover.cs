using UnityEngine;

public class NinjaMover : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Transform target;
    [SerializeField] private float speed = 1.2f;
    [SerializeField] private float stopDistance = 6f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string attackTriggerName = "Attack";

    [Header("Attack")]
    [SerializeField] private float attackInterval = 0.5f;
    [SerializeField] private int towerDamage = 40;

    [Header("Audio")]
    [SerializeField] private AudioSource hitAudioSource;
    [SerializeField] private AudioClip towerHitSound;

    private float attackTimer;
    private bool isAttacking;
    private TempleHealth templeHealth;

    private void Awake()
    {
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
            FaceTarget(delta);

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

        Vector3 dir = delta.normalized;

        float moveAmount = speed * Time.deltaTime;
        float remainingDistanceBeforeStop = distance - stopDistance;

        moveAmount = Mathf.Min(moveAmount, remainingDistanceBeforeStop);

        transform.position = pos + dir * moveAmount;

        FaceTarget(delta);
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

    private void AttackTower()
    {
        Debug.Log("Ninja attacks tower");

        if (animator != null)
        {
            animator.SetTrigger(attackTriggerName);
        }

        if (templeHealth == null)
        {
            FindTempleHealth();
        }

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

    private void FaceTarget(Vector3 delta)
    {
        delta.y = 0f;

        if (delta.sqrMagnitude <= 0.01f)
            return;

        Vector3 dir = delta.normalized;
        transform.forward = Vector3.Slerp(transform.forward, dir, 10f * Time.deltaTime);
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

    public void SetTarget(Transform t)
    {
        target = t;
        FindTempleHealth();
    }

    public void SetSpeed(float s)
    {
        speed = Mathf.Max(0f, s);
    }
}