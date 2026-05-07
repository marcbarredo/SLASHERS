using UnityEngine;
using UnityEngine.UI;

public class TempleHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    [Header("Health Bar")]
    [SerializeField] private Slider healthBar;

    [Header("Death Effect")]
    [SerializeField] private GameObject smokeParticlesPrefab;
    [SerializeField] private float smokeYOffset = 1.5f;
    [SerializeField] private float destroySmokeAfter = 4f;

    [Header("Death Audio")]
    [SerializeField] private AudioSource explosionAudioSource;
    [SerializeField] private AudioClip explosionClip;
    private bool isDead = false;

    private void Awake()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.value = (float)currentHealth / maxHealth;
        }
    }

    private void Die()
    {
        isDead = true;

        if (smokeParticlesPrefab != null)
        {
            Vector3 smokePosition = transform.position + Vector3.up * smokeYOffset;

            GameObject smoke = Instantiate(
                smokeParticlesPrefab,
                smokePosition,
                Quaternion.identity
            );

            Destroy(smoke, destroySmokeAfter);
        }

        if (explosionAudioSource != null && explosionClip != null)
        {
            explosionAudioSource.PlayOneShot(explosionClip);
        }

        Destroy(gameObject);
    }
}