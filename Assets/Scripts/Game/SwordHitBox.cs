using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    [SerializeField] private int damage = 30;
    [SerializeField] private float hitCooldown = 1f;

    private float _lastHitTime;

    private void OnTriggerEnter(Collider other)
    {
        TempleHealth temple = other.GetComponent<TempleHealth>();

        if (temple == null)
            temple = other.GetComponentInParent<TempleHealth>();

        if (temple == null)
            return;

        if (Time.time < _lastHitTime + hitCooldown)
            return;

        _lastHitTime = Time.time;

        temple.TakeDamage(damage);
    }
}