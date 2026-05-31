using UnityEngine;

public class SlashSoundAndTrail : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip slashSound;

    [Header("Old Trail Renderer Optional")]
    [SerializeField] private TrailRenderer[] slashTrails;

    [Header("Mesh Trail")]
    [SerializeField] private BladeSlashMeshTrail[] meshTrails;

    [Header("Slash Detection")]
    [SerializeField] private float minSlashSpeed = 250f;
    [SerializeField] private float resetSpeed = 100f;
    [SerializeField] private float minDistanceForSlash = 1f;
    [SerializeField] private float slashCooldown = 0.5f;

    [Header("Trail")]
    [SerializeField] private float trailStopDelay = 0.2f;
    [SerializeField] private bool clearTrailWhenSlashStarts = true;

    [Header("Options")]
    [SerializeField] private bool ignoreVerticalMovement = true;
    [SerializeField] private bool showDebugSpeed = false;

    private Vector3 lastPosition;
    private Vector3 fastMovementStartPosition;

    private bool isInFastMovement;
    private bool trailIsActive;

    private float nextAllowedSoundTime;
    private float stopTrailTime;

    private void Start()
    {
        lastPosition = transform.position;
        fastMovementStartPosition = transform.position;

        SetTrailsEmitting(false);
        StopMeshTrails();
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        if (deltaTime <= 0f)
            return;

        Vector3 currentPosition = transform.position;
        Vector3 movement = currentPosition - lastPosition;

        if (ignoreVerticalMovement)
            movement.y = 0f;

        float speed = movement.magnitude / deltaTime;

        if (showDebugSpeed)
            Debug.Log($"{name}: slash speed = {speed}");

        if (!isInFastMovement)
        {
            if (speed >= minSlashSpeed)
            {
                isInFastMovement = true;
                fastMovementStartPosition = currentPosition;

                StartSlashTrail();

                if (Time.time >= nextAllowedSoundTime)
                {
                    PlaySlashSound();
                    nextAllowedSoundTime = Time.time + slashCooldown;
                }
            }
        }
        else
        {
            Vector3 totalMovement = currentPosition - fastMovementStartPosition;

            if (ignoreVerticalMovement)
                totalMovement.y = 0f;

            float totalDistance = totalMovement.magnitude;

            if (totalDistance >= minDistanceForSlash)
            {
                stopTrailTime = Time.time + trailStopDelay;
            }

            if (speed <= resetSpeed)
            {
                isInFastMovement = false;
                stopTrailTime = Time.time + trailStopDelay;
            }
        }

        if (trailIsActive && Time.time >= stopTrailTime && !isInFastMovement)
        {
            StopSlashTrail();
        }

        lastPosition = currentPosition;
    }

    private void StartSlashTrail()
    {
        trailIsActive = true;
        stopTrailTime = Time.time + trailStopDelay;

        if (clearTrailWhenSlashStarts)
        {
            foreach (TrailRenderer trail in slashTrails)
            {
                if (trail != null)
                    trail.Clear();
            }

            foreach (BladeSlashMeshTrail meshTrail in meshTrails)
            {
                if (meshTrail != null)
                    meshTrail.ClearTrail();
            }
        }

        SetTrailsEmitting(true);
        StartMeshTrails();
    }

    private void StopSlashTrail()
    {
        trailIsActive = false;

        SetTrailsEmitting(false);
        StopMeshTrails();
    }

    private void SetTrailsEmitting(bool emitting)
    {
        foreach (TrailRenderer trail in slashTrails)
        {
            if (trail != null)
                trail.emitting = emitting;
        }
    }

    private void StartMeshTrails()
    {
        foreach (BladeSlashMeshTrail meshTrail in meshTrails)
        {
            if (meshTrail != null)
                meshTrail.StartTrail();
        }
    }

    private void StopMeshTrails()
    {
        foreach (BladeSlashMeshTrail meshTrail in meshTrails)
        {
            if (meshTrail != null)
                meshTrail.StopTrail();
        }
    }

    private void PlaySlashSound()
    {
        if (slashSound == null)
        {
            Debug.LogWarning($"{name}: no slash sound assigned.");
            return;
        }

        if (audioSource != null)
        {
            audioSource.PlayOneShot(slashSound);
            return;
        }

        GameObject soundObject = new GameObject("Slash Sound");
        AudioSource source = soundObject.AddComponent<AudioSource>();

        source.clip = slashSound;
        source.volume = 1f;
        source.spatialBlend = 0f;
        source.playOnAwake = false;

        source.Play();

        Destroy(soundObject, slashSound.length + 0.1f);
    }
}