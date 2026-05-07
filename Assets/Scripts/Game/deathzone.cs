using UnityEngine;

public class WaterDeathZone : MonoBehaviour
{
    [SerializeField] private string enemyTag = "Enemy";

    [Header("Splash Effect")]
    [SerializeField] private GameObject splashEffectPrefab;
    [SerializeField] private float destroySplashAfter = 2f;

    [Header("Splash Audio")]
    [SerializeField] private AudioSource splashAudioSource;
    [SerializeField] private AudioClip splashClip;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(enemyTag))
            return;

        if (splashEffectPrefab != null)
        {
            GameObject splash = Instantiate(
                splashEffectPrefab,
                other.transform.position,
                Quaternion.identity
            );

            Destroy(splash, destroySplashAfter);
        }

        if (splashAudioSource != null && splashClip != null)
        {
            splashAudioSource.PlayOneShot(splashClip);
        }

        Destroy(other.gameObject);
    }
}