using UnityEngine;

public class NinjaMover : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float speed = 1.2f;
    [SerializeField] private float stopDistance = 0.35f;

    public void SetTarget(Transform t) => target = t;
    public void SetSpeed(float s) => speed = Mathf.Max(0f, s);
    void Update()
    {
        if (!target) return;

        Vector3 pos = transform.position;
        Vector3 goal = target.position;

        // Keep movement on the floor (XZ)
        goal.y = pos.y;

        Vector3 delta = goal - pos;
        float dist = delta.magnitude;

        if (dist <= stopDistance) return;

        Vector3 dir = delta / dist;
        transform.position = pos + dir * speed * Time.deltaTime;

        // Face movement direction
        if (dir.sqrMagnitude > 0.01f)
            transform.forward = Vector3.Slerp(transform.forward, dir, 10f * Time.deltaTime);
    }
}
