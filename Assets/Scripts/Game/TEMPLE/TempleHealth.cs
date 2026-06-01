using UnityEngine;
using UnityEngine.UI;

public class TempleHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    [Header("Health Bar")]
    [SerializeField] private Slider healthBar;

    [Header("Visuals")]
    [SerializeField] private GameObject templeVisualRoot;
    [SerializeField] private Collider templeCollider;

    [Header("Death Effect")]
    [SerializeField] private GameObject smokeParticlesPrefab;
    [SerializeField] private float smokeYOffset = 1.5f;
    [SerializeField] private float destroySmokeAfter = 4f;

    [Header("Death Audio")]
    [SerializeField] private AudioSource explosionAudioSource;
    [SerializeField] private AudioClip explosionClip;

    [Header("Flow")]
    [SerializeField] private GameFlowManager gameFlowManager;

    private bool isDead = false;

    private void Start()
    {
        ResetTemple();

        if (gameFlowManager == null)
            gameFlowManager = FindFirstObjectByType<GameFlowManager>();
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

    public void ResetTemple()
    {
        isDead = false;
        currentHealth = maxHealth;
        UpdateHealthBar();

        if (templeVisualRoot != null)
            templeVisualRoot.SetActive(true);

        if (templeCollider != null)
            templeCollider.enabled = true;
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
        if (isDead) return;
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

        if (templeVisualRoot != null)
            templeVisualRoot.SetActive(false);

        if (templeCollider != null)
            templeCollider.enabled = false;

        if (gameFlowManager != null)
            gameFlowManager.OnTowerDestroyed();

        Destroy(gameObject);
    }
}