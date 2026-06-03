using UnityEngine;
using UnityEngine.UI;

public class GolemCollider : MonoBehaviour
{
    [Header("Hit Settings")]
    [SerializeField] private string bladeTag = "Blade";
    [SerializeField] private float hitCooldown = 0.25f;
    [SerializeField] private int damagePerHit = 1;

    [Header("Health")]
    [SerializeField] private int maxHealth = 10;

    [Header("Health Bar")]
    [SerializeField] private Slider healthSlider;

    [Header("Death")]
    [SerializeField] private float destroyDelay = 0.1f;

    private int currentHealth;
    private float nextAllowedHitTime;
    private bool dead;

    private void Awake()
    {
        currentHealth = maxHealth;

        FindHealthSlider();
        UpdateHealthBar();

        Debug.Log($"{name}: golem health initialized {currentHealth}/{maxHealth}");
    }

    private void OnTriggerEnter(Collider other)
    {
        TryTakeHit(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryTakeHit(collision.gameObject);
    }

    private void TryTakeHit(GameObject other)
    {
        if (dead)
            return;

        if (!other.CompareTag(bladeTag))
            return;

        if (Time.time < nextAllowedHitTime)
            return;

        nextAllowedHitTime = Time.time + hitCooldown;

        TakeDamage(damagePerHit);
    }

    private void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth < 0)
            currentHealth = 0;

        Debug.Log($"{name}: golem health {currentHealth}/{maxHealth}");

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void FindHealthSlider()
    {
        if (healthSlider != null)
            return;

        GameObject golemRoot = GetGolemRoot();

        if (golemRoot == null)
            return;

        healthSlider = golemRoot.GetComponentInChildren<Slider>();
    }

    private void UpdateHealthBar()
    {
        if (healthSlider == null)
            return;

        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
    }

    private GameObject GetGolemRoot()
    {
        NinjaMover ninjaMover = GetComponentInParent<NinjaMover>();

        if (ninjaMover != null)
            return ninjaMover.gameObject;

        return transform.root.gameObject;
    }

    private void Die()
    {
        if (dead)
            return;

        dead = true;

        GameObject golemRoot = GetGolemRoot();

        NinjaMover mover = golemRoot.GetComponent<NinjaMover>();

        if (mover != null)
            mover.enabled = false;

        Collider[] colliders = golemRoot.GetComponentsInChildren<Collider>();

        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        Destroy(golemRoot, destroyDelay);
    }
}