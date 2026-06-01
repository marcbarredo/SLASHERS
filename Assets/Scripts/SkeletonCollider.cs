using UnityEngine;

public class SkeletonCollider : MonoBehaviour
{
    [Header("Cut Settings")]
    [SerializeField] private string bladeTag = "Blade";

    [Header("Hits To Kill")]
    [SerializeField] private int minHitsToKill = 1;
    [SerializeField] private int maxHitsToKill = 3;

    [Header("Hit Cooldown")]
    [SerializeField] private float hitCooldown = 0.25f;

    [Header("3D Text")]
    [SerializeField] private TextMesh hitsText;

    [Header("Death Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string deathTriggerName = "Die";
    [SerializeField] private float destroyAfterDeathAnimation = 1.5f;

    [Header("Death")]
    [SerializeField] private AudioClip dieSound;

    private int hitsNeeded;
    private int currentHits;
    private float nextAllowedHitTime;
    private bool dead;

    private void Awake()
    {
        hitsNeeded = Random.Range(minHitsToKill, maxHitsToKill + 1);

        if (hitsNeeded < 1)
            hitsNeeded = 1;

        if (animator == null)
            animator = GetComponentInParent<Animator>();

        FindHitsText();
        UpdateHitsText();

        Debug.Log($"{name}: skeleton needs {hitsNeeded} hit/s to die.");
    }

    private void OnTriggerEnter(Collider other)
    {
        TryRegisterHit(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryRegisterHit(collision.gameObject);
    }

    private void TryRegisterHit(GameObject other)
    {
        if (dead)
            return;

        if (!other.CompareTag(bladeTag))
            return;

        if (Time.time < nextAllowedHitTime)
            return;

        nextAllowedHitTime = Time.time + hitCooldown;

        currentHits++;

        Debug.Log($"{name}: skeleton hit {currentHits}/{hitsNeeded}");

        UpdateHitsText();

        if (currentHits >= hitsNeeded)
        {
            Die();
        }
    }

    private void FindHitsText()
    {
        if (hitsText != null)
            return;

        GameObject skeletonRoot = GetSkeletonRoot();

        if (skeletonRoot == null)
            return;

        Transform nhits = skeletonRoot.transform.Find("Nhits");

        if (nhits != null)
            hitsText = nhits.GetComponent<TextMesh>();
    }

    private void UpdateHitsText()
    {
        if (hitsText == null)
            return;

        int hitsRemaining = hitsNeeded - currentHits;

        if (hitsRemaining < 0)
            hitsRemaining = 0;

        hitsText.text = hitsRemaining.ToString();
    }

    private GameObject GetSkeletonRoot()
    {
        SkeletonMover skeletonMover = GetComponentInParent<SkeletonMover>();

        if (skeletonMover != null)
            return skeletonMover.gameObject;

        return transform.root.gameObject;
    }

    private void Die()
    {
        if (dead)
            return;

        dead = true;

        GameObject skeletonRoot = GetSkeletonRoot();

        if (hitsText != null)
            hitsText.text = "0";

        SkeletonMover mover = skeletonRoot.GetComponent<SkeletonMover>();

        if (mover != null)
            mover.enabled = false;

        Collider[] colliders = skeletonRoot.GetComponentsInChildren<Collider>();

        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        if (animator != null)
        {
            animator.SetTrigger(deathTriggerName);
        }

        if (dieSound != null)
        {
            GameObject soundObject = new GameObject("Skeleton Die Sound");
            AudioSource source = soundObject.AddComponent<AudioSource>();

            source.clip = dieSound;
            source.volume = 1f;
            source.spatialBlend = 0f;
            source.playOnAwake = false;

            source.Play();

            Destroy(soundObject, dieSound.length + 0.1f);
        }

        Destroy(skeletonRoot, destroyAfterDeathAnimation);
    }
}