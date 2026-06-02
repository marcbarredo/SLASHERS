using UnityEngine;

public class GolemSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform towerTarget;
    [SerializeField] private GameObject golemPrefab;

    [Header("Spawn Timing")]
    [SerializeField] private float spawnAfterSeconds = 45f;

    [Header("Golem Movement")]
    [SerializeField] private float golemSpeed = 0.6f;
    [SerializeField] private float golemStopDistance = 5f; // NEW: Stop distance from tower

    [Header("Spawn Radius From Tower")]
    [SerializeField] private float spawnRadiusFromTower = 20f;

    [Header("Ground Snapping")]
    [SerializeField] private string groundTag = "Ground";
    [SerializeField] private float rayStartHeight = 20f;
    [SerializeField] private float groundOffset = 0.02f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip spawnSound;

    private float timer;
    private bool hasSpawned;

    private void Update()
    {
        if (hasSpawned)
            return;

        timer += Time.deltaTime;

        if (timer >= spawnAfterSeconds)
        {
            SpawnGolem();
        }
    }

    private void SpawnGolem()
    {
        if (hasSpawned) return;

        if (golemPrefab == null || towerTarget == null)
        {
            Debug.LogWarning("GolemSpawner missing references.");
            return;
        }

        hasSpawned = true;

        Vector3 spawnPosition = GetRandomPositionAroundTower();

        if (TryProjectToGround(spawnPosition, out Vector3 groundPosition))
            spawnPosition = groundPosition;

        Quaternion rotation = GetRotationFacingTower(spawnPosition);

        GameObject golem = Instantiate(golemPrefab, spawnPosition, rotation);

        if (audioSource != null && spawnSound != null)
            audioSource.PlayOneShot(spawnSound);

        // Setup movement
        SkeletonMover mover = golem.GetComponent<SkeletonMover>();
        if (mover == null)
            mover = golem.GetComponentInChildren<SkeletonMover>();

        if (mover != null)
        {
            mover.enabled = true;
            mover.SetTarget(towerTarget);
            mover.SetSpeed(golemSpeed);
            mover.SetStopDistance(golemStopDistance); 
        }
        else
        {
            Debug.LogError("Golem has no SkeletonMover component!");
        }
    }

    private Vector3 GetRandomPositionAroundTower()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float x = towerTarget.position.x + Mathf.Cos(angle) * spawnRadiusFromTower;
        float z = towerTarget.position.z + Mathf.Sin(angle) * spawnRadiusFromTower;
        return new Vector3(x, towerTarget.position.y, z);
    }

    private Quaternion GetRotationFacingTower(Vector3 spawnPosition)
    {
        Vector3 dir = towerTarget.position - spawnPosition;
        dir.y = 0f;
        return dir.sqrMagnitude > 0.01f ? Quaternion.LookRotation(dir) : Quaternion.identity;
    }

    private bool TryProjectToGround(Vector3 position, out Vector3 groundPosition)
    {
        Vector3 rayStart = new Vector3(position.x, position.y + rayStartHeight, position.z);
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, rayStartHeight * 2f))
        {
            if (hit.collider.CompareTag(groundTag))
            {
                groundPosition = hit.point + Vector3.up * groundOffset;
                return true;
            }
        }
        groundPosition = default;
        return false;
    }

    public void ResetSpawner()
    {
        timer = 0f;
        hasSpawned = false;
    }
}