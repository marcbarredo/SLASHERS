using UnityEngine;

[RequireComponent(typeof(NinjaMover))]
public class NinjaAnimatorDriver : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Header("Animator Params (match controller)")]
    [SerializeField] private string velocityParam = "Velocity";
    [SerializeField] private string movingParam = "Moving";

    [Header("Tuning")]
    [SerializeField] private float dampTime = 0.12f;
    [SerializeField] private float velocityMultiplier = 1f;
    [SerializeField] private float movingThreshold = 0.05f; // meters/sec

    private Vector3 _lastPos;

    void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        _lastPos = transform.position;
    }

    void Update()
    {
        if (!animator) return;

        float mps = (transform.position - _lastPos).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
        _lastPos = transform.position;

        animator.SetBool(movingParam, mps > movingThreshold);

        float v = mps * velocityMultiplier;
        animator.SetFloat(velocityParam, v, dampTime, Time.deltaTime);
    }
}