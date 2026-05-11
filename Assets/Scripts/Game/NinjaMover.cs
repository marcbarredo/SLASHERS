using UnityEngine;

public class NinjaMover : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Transform target;
    [SerializeField] private float speed = 1.2f;
    [SerializeField] private float stopDistance = 5f;

    [Header("Attack")]
    [SerializeField] private float attackInterval = 1f;
    private float attackTimer;

    [Header("Audio")]
    [SerializeField] private AudioSource hitAudioSource;
    [SerializeField] private AudioClip towerHitSound;

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

        // ATTACK STATE
        if (dist <= stopDistance)
        {
            attackTimer += Time.deltaTime;

            if (attackTimer >= attackInterval)
            {
                attackTimer = 0f;

                PlayTowerHitSound();
            }

            return;
        }

        Vector3 dir = delta / dist;

        transform.position = pos + dir * speed * Time.deltaTime;

        // Face movement direction
        if (dir.sqrMagnitude > 0.01f)
            transform.forward = Vector3.Slerp(transform.forward, dir, 10f * Time.deltaTime);
    }

    private void PlayTowerHitSound()
    {
        Debug.Log("HIT");

        if (hitAudioSource != null && towerHitSound != null)
        {
            hitAudioSource.pitch = Random.Range(0.9f, 1.1f);
            hitAudioSource.PlayOneShot(towerHitSound);
        }
    }
}