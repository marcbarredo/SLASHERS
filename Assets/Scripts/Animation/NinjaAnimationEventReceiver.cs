using UnityEngine;

public class NinjaAnimationEventReceiver : MonoBehaviour
{
    [Header("Footstep Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip footstepSound;

    [Header("Attack Effect")]
    [SerializeField] private NinjaAttackEffect attackEffect;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (attackEffect == null)
            attackEffect = GetComponent<NinjaAttackEffect>();
    }

    // Called by Run animation event
    public void FootR()
    {
        PlayFootstep();
    }

    // Called by Run animation event
    public void FootL()
    {
        PlayFootstep();
    }

    // Called by Attack animation event
    public void Hit()
    {
        if (attackEffect != null)
        {
            attackEffect.PlaySlashEffect();
        }
    }

    private void PlayFootstep()
    {
        if (audioSource != null && footstepSound != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(footstepSound);
        }
    }
}