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

    [Header("Audio")]
    [SerializeField] private AudioSource hitAudioSource;
    [SerializeField] private AudioClip towerHitSound;

    private float attackTimer;

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

    private void AttackTower()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer < attackInterval)
            return;

        attackTimer = 0f;

        TempleHealth templeHealth = target.GetComponent<TempleHealth>();

        if (templeHealth == null)
            templeHealth = target.GetComponentInParent<TempleHealth>();

        if (templeHealth != null)
        {
            templeHealth.TakeDamage(towerDamage);
        }

        if (hitAudioSource != null && towerHitSound != null)
        {
            hitAudioSource.PlayOneShot(towerHitSound);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
}