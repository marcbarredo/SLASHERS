using System.Collections.Generic;
using UnityEngine;

public class TwoBarrierCutEnemy : MonoBehaviour
{
    [Header("Cut Settings")]
    [SerializeField] private float maxTimeBetweenBarriers = 0.15f;
    [SerializeField] private string bladeTag = "Blade";

    [Header("Kill Cooldown")]
    [SerializeField] private float killCooldownAfterSuccess = 0.35f;

    [Header("Death")]
    [SerializeField] private AudioClip dieSound;
    [SerializeField] private float destroyDelay = 0.1f;

    private static readonly Dictionary<int, float> nextAllowedKillTimeByBladeOwner = new Dictionary<int, float>();

    private int firstBarrierTouched = 0;
    private float firstTouchTime;
    private Transform firstBladeOwner;
    private bool dead;

    public void BarrierTouched(int barrierNumber, Collider other)
    {
        if (dead)
            return;

        if (!other.CompareTag(bladeTag))
            return;

        Transform bladeOwner = other.transform.root;
        int bladeOwnerId = bladeOwner.GetInstanceID();

        if (IsBladeOnCooldown(bladeOwnerId))
        {
            Debug.Log($"{name}: blade is on kill cooldown.");
            return;
        }

        if (firstBarrierTouched == 0)
        {
            firstBarrierTouched = barrierNumber;
            firstTouchTime = Time.time;
            firstBladeOwner = bladeOwner;

            Debug.Log($"{name}: first barrier touched = {barrierNumber}");
            return;
        }

        if (barrierNumber == firstBarrierTouched)
            return;

        if (firstBladeOwner != bladeOwner)
        {
            Debug.Log($"{name}: second barrier touched by a different blade/player.");
            ResetCut();
            return;
        }

        float timeDifference = Time.time - firstTouchTime;

        if (timeDifference > maxTimeBetweenBarriers)
        {
            Debug.Log($"{name}: too slow between barriers.");
            ResetCut();
            return;
        }

        bool correctOrder = firstBarrierTouched == 1 && barrierNumber == 2;

        if (correctOrder)
        {
            PutBladeOnCooldown(bladeOwnerId);
            Die();
        }
        else
        {
            Debug.Log($"{name}: wrong cut direction.");
            ResetCut();
        }
    }

    private bool IsBladeOnCooldown(int bladeOwnerId)
    {
        if (!nextAllowedKillTimeByBladeOwner.ContainsKey(bladeOwnerId))
            return false;

        return Time.time < nextAllowedKillTimeByBladeOwner[bladeOwnerId];
    }

    private void PutBladeOnCooldown(int bladeOwnerId)
    {
        nextAllowedKillTimeByBladeOwner[bladeOwnerId] = Time.time + killCooldownAfterSuccess;
    }

    private void ResetCut()
    {
        firstBarrierTouched = 0;
        firstTouchTime = 0f;
        firstBladeOwner = null;
    }

    private void Die()
    {
        dead = true;

        Debug.Log($"{name}: killed by correct two-barrier cut.");

        if (dieSound != null)
        {
            GameObject soundObject = new GameObject("Die Sound");
            AudioSource source = soundObject.AddComponent<AudioSource>();

            source.clip = dieSound;
            source.volume = 1f;
            source.spatialBlend = 0f; // 0 = 2D sound, always audible
            source.playOnAwake = false;

            source.Play();

            Destroy(soundObject, dieSound.length + 0.1f);
        }
        else
        {
            Debug.LogWarning($"{name}: no die sound assigned.");
        }

        Destroy(gameObject, destroyDelay);
    }
}