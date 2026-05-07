using UnityEngine;

public class SkyNinjaAI : MonoBehaviour
{
    private enum State
    {
        Falling,
        Landing,
        Running,
        Attacking
    }

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform target;

    [Header("Movement")]
    [SerializeField] private float runSpeed = 3f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float attackDistance = 1.2f;

    [Header("Falling")]
    [SerializeField] private float gravity = 18f;
    [SerializeField] private float groundCheckDistance = 0.25f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundOffset = 0.02f;

    [Header("Animation Timing")]
    [SerializeField] private float landDuration = 0.6f;
    [SerializeField] private float attackCooldown = 1.2f;

    [Header("Animator Params")]
    [SerializeField] private string fallTrigger = "Fall";
    [SerializeField] private string landTrigger = "Land";
    [SerializeField] private string runBool = "Run";
    [SerializeField] private string attackTrigger = "Attack";
    [SerializeField] private string velocityParam = "Velocity";

    private State currentState;
    private float verticalVelocity;
    private float stateTimer;
    private float attackTimer;

    private void Awake()
    {
        if (!animator)
            animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        StartFalling();
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.Falling:
                UpdateFalling();
                break;

            case State.Landing:
                UpdateLanding();
                break;

            case State.Running:
                UpdateRunning();
                break;

            case State.Attacking:
                UpdateAttacking();
                break;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetRunSpeed(float newSpeed)
    {
        runSpeed = Mathf.Max(0f, newSpeed);
    }

    private void StartFalling()
    {
        currentState = State.Falling;
        verticalVelocity = 0f;

        if (animator)
        {
            animator.SetBool(runBool, false);
            animator.SetTrigger(fallTrigger);
        }
    }

    private void UpdateFalling()
    {
        verticalVelocity -= gravity * Time.deltaTime;
        transform.position += Vector3.up * verticalVelocity * Time.deltaTime;

        if (IsGrounded(out RaycastHit hit))
        {
            Vector3 groundedPosition = hit.point + Vector3.up * groundOffset;
            transform.position = groundedPosition;

            StartLanding();
        }
    }

    private void StartLanding()
    {
        currentState = State.Landing;
        stateTimer = 0f;

        if (animator)
        {
            animator.SetBool(runBool, false);
            animator.SetTrigger(landTrigger);
        }
    }

    private void UpdateLanding()
    {
        stateTimer += Time.deltaTime;

        if (stateTimer >= landDuration)
        {
            StartRunning();
        }
    }

    private void StartRunning()
    {
        currentState = State.Running;

        if (animator)
        {
            animator.SetBool(runBool, true);
        }
    }

    private void UpdateRunning()
    {
        if (!target)
        {
            if (animator)
                animator.SetBool(runBool, false);

            return;
        }

        Vector3 pos = transform.position;
        Vector3 goal = target.position;

        // Mantiene el movimiento en el suelo
        goal.y = pos.y;

        Vector3 delta = goal - pos;
        float distance = delta.magnitude;

        if (animator)
            animator.SetFloat(velocityParam, runSpeed);

        if (distance <= attackDistance)
        {
            StartAttacking();
            return;
        }

        Vector3 direction = delta.normalized;

        transform.position += direction * runSpeed * Time.deltaTime;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private void StartAttacking()
    {
        currentState = State.Attacking;
        attackTimer = 0f;

        if (animator)
        {
            animator.SetBool(runBool, false);
            animator.SetFloat(velocityParam, 0f);
            animator.SetTrigger(attackTrigger);
        }
    }

    private void UpdateAttacking()
    {
        attackTimer += Time.deltaTime;

        // Si quieres que ataque una sola vez, puedes dejarlo parado aquí.
        // Si quieres que ataque en loop, repetimos el trigger cada cooldown.
        if (attackTimer >= attackCooldown)
        {
            attackTimer = 0f;

            if (animator)
                animator.SetTrigger(attackTrigger);
        }

        FaceTarget();
    }

    private bool IsGrounded(out RaycastHit hit)
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.2f;

        return Physics.Raycast(
            rayOrigin,
            Vector3.down,
            out hit,
            groundCheckDistance + 0.3f,
            groundLayer
        );
    }

    private void FaceTarget()
    {
        if (!target) return;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}