using UnityEngine;

public class NinjaAttackEffect : MonoBehaviour
{
    [SerializeField] private GameObject slashEffect;
    [SerializeField] private float effectDuration = 0.3f;

    public void PlaySlashEffect()
    {
        if (!slashEffect) return;

        slashEffect.SetActive(false);
        slashEffect.SetActive(true);

        CancelInvoke(nameof(StopSlashEffect));
        Invoke(nameof(StopSlashEffect), effectDuration);
    }

    private void StopSlashEffect()
    {
        if (slashEffect)
            slashEffect.SetActive(false);
    }
}