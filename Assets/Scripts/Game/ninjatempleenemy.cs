using UnityEngine;

public class NinjaTempleEnemy : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject targetTemple;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float attackDistance = 1.5f;

    [Header("Falling")]
    [SerializeField] private float gravity = 20f;
    [SerializeField] private string groundTag = "Ground";
    [SerializeField] private float groundOffset = 0.03f;

    [Header("Animator Parameters")]
    [SerializeField] private string runParam = "Run";
    [SerializeField] private string attackParam = "Attack";

    private bool hasLanded = false;
    private bool isAttacking = false;
    private float verticalVelocity = 0f;

    private void Awake()
    {
        if (!animator)
            animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (!hasLanded)
        {
            FallUntilGround();
            return;
        }

        if (!targetTemple) return;

        if (isAttacking)
        {
            FaceTarget();
            return;
        }

        MoveToTemple();
    }

    public void SetTarget(GameObject target)
    {
        targetTemple = target;
    }

    public void SetSpeed(float speed)
    {
        moveSpeed = Mathf.Max(0f, speed);
    }

    private void FallUntilGround()
    {
        verticalVelocity -= gravity * Time.deltaTime;

        Vector3 currentPosition = transform.position;
        Vector3 nextPosition = currentPosition + Vector3.up * verticalVelocity * Time.deltaTime;

        float rayDistance = Vector3.Distance(currentPosition, nextPosition) + 0.5f;

        if (Physics.Raycast(currentPosition, Vector3.down, out RaycastHit hit, rayDistance))
        {
            if (hit.collider.CompareTag(groundTag))
            {
                transform.position = hit.point + Vector3.up * groundOffset;
                Land();
                return;
            }
        }

        transform.position = nextPosition;
    }

    private void Land()
    {
        hasLanded = true;

        if (animator)
        {
            animator.SetBool(runParam, true);
            animator.SetBool(attackParam, false);
        }
    }

    private void MoveToTemple()
    {
        Vector3 position = transform.position;
        Vector3 targetPosition = targetTemple.transform.position;

        targetPosition.y = position.y;

        Vector3 direction = targetPosition - position;
        float distance = direction.magnitude;

        if (distance <= attackDistance)
        {
            StartAttack();
            return;
        }

        direction.Normalize();

        transform.position += direction * moveSpeed * Time.deltaTime;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        if (animator)
        {
            animator.SetBool(runParam, true);
            animator.SetBool(attackParam, false);
        }
    }

    private void StartAttack()
    {
        isAttacking = true;

        if (animator)
        {
            animator.SetBool(runParam, false);
            animator.SetBool(attackParam, true);
        }

        FaceTarget();
    }

    private void FaceTarget()
    {
        if (!targetTemple) return;

        Vector3 direction = targetTemple.transform.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f) return;

        Quaternion lookRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            lookRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}