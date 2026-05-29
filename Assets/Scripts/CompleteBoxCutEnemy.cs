using System.Collections.Generic;
using UnityEngine;

public class CompleteBoxCutEnemy : MonoBehaviour
{
    [Header("Arrow Direction Points")]
    [SerializeField] private Transform arrowStartPoint;
    [SerializeField] private Transform arrowEndPoint;

    [Header("Cut Settings")]
    [SerializeField] private string bladeTag = "Blade";

    [Tooltip("Minimum distance the blade must travel while inside the collider.")]
    [SerializeField] private float minSlashDistance = 0.8f;

    [Tooltip("Maximum time allowed between entering and exiting the collider.")]
    [SerializeField] private float maxTimeInsideCollider = 0.6f;

    [Tooltip("Minimum speed required. Speed = distance / time.")]
    [SerializeField] private float minSlashSpeed = 3.0f;

    [Tooltip("Maximum allowed angle between the slash and the arrow direction.")]
    [SerializeField] private float maxAngleDifference = 35f;

    [Header("Kill Cooldown")]
    [SerializeField] private float killCooldownAfterSuccess = 0.35f;

    [Header("Death")]
    [SerializeField] private AudioClip dieSound;
    [SerializeField] private float destroyDelay = 0.1f;

    private static readonly Dictionary<int, float> nextAllowedKillTimeByBladeOwner = new Dictionary<int, float>();
    private readonly Dictionary<int, SlashEntryData> activeSlashes = new Dictionary<int, SlashEntryData>();

    private bool dead;

    private struct SlashEntryData
    {
        public Vector3 enterPosition;
        public float enterTime;
    }

    private void OnTriggerEnter(Collider other)
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

        activeSlashes[bladeOwnerId] = new SlashEntryData
        {
            enterPosition = other.transform.position,
            enterTime = Time.time
        };

        Debug.Log($"{name}: blade entered complete collider.");
    }

    private void OnTriggerExit(Collider other)
    {
        if (dead)
            return;

        if (!other.CompareTag(bladeTag))
            return;

        Transform bladeOwner = other.transform.root;
        int bladeOwnerId = bladeOwner.GetInstanceID();

        if (!activeSlashes.ContainsKey(bladeOwnerId))
            return;

        SlashEntryData entryData = activeSlashes[bladeOwnerId];
        activeSlashes.Remove(bladeOwnerId);

        Vector3 exitPosition = other.transform.position;

        Vector3 slashWorldVector = exitPosition - entryData.enterPosition;
        slashWorldVector.y = 0f;

        float slashDistance = slashWorldVector.magnitude;
        float timeInside = Time.time - entryData.enterTime;

        if (timeInside <= 0f)
            timeInside = 0.001f;

        float slashSpeed = slashDistance / timeInside;

        if (slashDistance < minSlashDistance)
        {
            Debug.Log($"{name}: slash too short. Distance = {slashDistance}");
            return;
        }

        if (timeInside > maxTimeInsideCollider)
        {
            Debug.Log($"{name}: slash too slow by time. Time = {timeInside}");
            return;
        }

        if (slashSpeed < minSlashSpeed)
        {
            Debug.Log($"{name}: slash too slow by speed. Speed = {slashSpeed}");
            return;
        }

        if (!TryGetArrowDirection(out Vector3 arrowWorldVector))
            return;

        Vector3 slashDirection = slashWorldVector.normalized;
        Vector3 arrowDirection = arrowWorldVector.normalized;

        float dot = Vector3.Dot(slashDirection, arrowDirection);
        float angle = Vector3.Angle(slashDirection, arrowDirection);

        Debug.Log($"{name}: slash distance = {slashDistance}");
        Debug.Log($"{name}: slash time = {timeInside}");
        Debug.Log($"{name}: slash speed = {slashSpeed}");
        Debug.Log($"{name}: dot = {dot}");
        Debug.Log($"{name}: angle difference = {angle}");

        bool sameSign = dot > 0f;
        bool angleIsValid = angle <= maxAngleDifference;

        if (sameSign && angleIsValid)
        {
            PutBladeOnCooldown(bladeOwnerId);
            Die();
        }
        else
        {
            Debug.Log($"{name}: wrong slash direction.");
        }
    }

    private bool TryGetArrowDirection(out Vector3 arrowWorldVector)
    {
        arrowWorldVector = Vector3.zero;

        if (arrowStartPoint == null || arrowEndPoint == null)
        {
            Debug.LogWarning($"{name}: arrow start or arrow end point is not assigned.");
            return false;
        }

        arrowWorldVector = arrowEndPoint.position - arrowStartPoint.position;
        arrowWorldVector.y = 0f;

        if (arrowWorldVector.sqrMagnitude < 0.001f)
        {
            Debug.LogWarning($"{name}: arrow direction is too small.");
            return false;
        }

        return true;
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

    private void Die()
    {
        dead = true;

        Debug.Log($"{name}: killed by complete box direction cut.");

        if (dieSound != null)
        {
            GameObject soundObject = new GameObject("Die Sound");
            AudioSource source = soundObject.AddComponent<AudioSource>();

            source.clip = dieSound;
            source.volume = 1f;
            source.spatialBlend = 0f;
            source.playOnAwake = false;

            source.Play();

            Destroy(soundObject, dieSound.length + 0.1f);
        }

        Destroy(gameObject, destroyDelay);
    }
}