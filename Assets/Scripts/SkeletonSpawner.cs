using UnityEngine;

public class SkeletonSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform towerTarget;
    [SerializeField] private GameObject[] skeletonPrefabs;
    [SerializeField] private float skeletonSpeed = 2.0f;

    [SerializeField] private Renderer landRenderer;
    [SerializeField] private Transform fallbackCenter;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip spawnSound;

    [Header("Normal Spawning")]
    [SerializeField] private bool allowNormalSpawning = false;

    [Header("Spawn Circle Settings")]
    [Range(0.1f, 1f)]
    [SerializeField] private float radiusFactor = 0.9f;

    [SerializeField] private float fixedRadius = 10f;
    [SerializeField] private float edgeInset = 0.25f;

    [Header("Spawn Rules")]
    [SerializeField] private float minDistanceFromTower = 3f;
    [SerializeField] private int maxTries = 40;

    [Header("Ground Snapping")]
    [SerializeField] private string groundTag = "Ground";
    [SerializeField] private float rayStartHeight = 10f;
    [SerializeField] private float groundOffset = 0.02f;

    [Header("Spawning")]
    [SerializeField] private float spawnEverySeconds = 2f;
    [SerializeField] private int maxAlive = 12;

    [Header("Spawn On Death")]
    [SerializeField] private bool spawnOnEnemyDeath = true;
    [SerializeField] private int amountToSpawnOnDeath = 1;
    [SerializeField] private float deathSpawnRadius = 0f;

    private float _timer;

    private void Update()
    {
        if (!allowNormalSpawning)
            return;

        if (!towerTarget)
        {
            Debug.LogWarning("SkeletonSpawner has no tower target assigned.");
            return;
        }

        if (skeletonPrefabs == null || skeletonPrefabs.Length == 0)
        {
            Debug.LogWarning("No skeleton prefabs assigned to SkeletonSpawner.");
            return;
        }

        _timer += Time.deltaTime;

        if (_timer < spawnEverySeconds)
            return;

        _timer = 0f;

        if (CountAlive() >= maxAlive)
            return;

        SpawnOne();
    }

    private int CountAlive()
    {
        return FindObjectsByType<SkeletonMover>(FindObjectsSortMode.None).Length;
    }

    private void SpawnOne()
    {
        if (!TryGetSpawnPosition(out Vector3 pos))
        {
            Debug.LogWarning("SkeletonSpawner could not find a valid spawn position.");
            return;
        }

        SpawnPrefabAtRandom(pos);
    }

    public void SpawnAtDeathPosition(Vector3 deathPosition)
    {
        if (!spawnOnEnemyDeath)
            return;

        if (skeletonPrefabs == null || skeletonPrefabs.Length == 0)
        {
            Debug.LogWarning("No skeleton prefabs assigned to SkeletonSpawner.");
            return;
        }

        for (int i = 0; i < amountToSpawnOnDeath; i++)
        {
            Vector3 spawnPosition = deathPosition;

            if (deathSpawnRadius > 0f)
            {
                Vector2 randomCircle = Random.insideUnitCircle * deathSpawnRadius;

                spawnPosition = new Vector3(
                    deathPosition.x + randomCircle.x,
                    deathPosition.y,
                    deathPosition.z + randomCircle.y
                );
            }

            if (TryProjectToGround(spawnPosition, out Vector3 groundPos))
            {
                SpawnPrefabAtRandom(groundPos);
            }
            else
            {
                SpawnPrefabAtRandom(spawnPosition);
            }
        }
    }

    private void SpawnPrefabAtRandom(Vector3 pos)
    {
        if (skeletonPrefabs == null || skeletonPrefabs.Length == 0)
            return;

        GameObject prefabToSpawn = skeletonPrefabs[Random.Range(0, skeletonPrefabs.Length)];

        Quaternion rotation = Quaternion.identity;

        if (towerTarget != null)
        {
            Vector3 directionToTower = towerTarget.position - pos;
            directionToTower.y = 0f;

            if (directionToTower.sqrMagnitude > 0.01f)
                rotation = Quaternion.LookRotation(directionToTower.normalized, Vector3.up);
        }

        GameObject skeleton = Instantiate(prefabToSpawn, pos, rotation);

        if (audioSource != null && spawnSound != null)
        {
            audioSource.PlayOneShot(spawnSound);
        }

        SkeletonMover mover = skeleton.GetComponent<SkeletonMover>();

        if (mover == null)
            mover = skeleton.GetComponentInChildren<SkeletonMover>();

        if (mover != null)
        {
            mover.SetTarget(towerTarget);
            mover.SetSpeed(skeletonSpeed);
        }
        else
        {
            Debug.LogWarning("Spawned skeleton does not have a SkeletonMover component.");
        }
    }

    private bool TryGetSpawnPosition(out Vector3 pos)
    {
        Vector3 center = GetSpawnCenter();
        float radius = GetSpawnRadius();

        for (int i = 0; i < maxTries; i++)
        {
            Vector3 xz = RandomPointOnCircle(center, radius);

            if (!FarEnoughFromTower(xz))
                continue;

            if (TryProjectToGround(xz, out Vector3 groundPos))
            {
                pos = groundPos;
                return true;
            }
        }

        pos = default;
        return false;
    }

    private Vector3 GetSpawnCenter()
    {
        if (landRenderer)
            return landRenderer.bounds.center;

        if (fallbackCenter)
            return fallbackCenter.position;

        if (towerTarget)
            return towerTarget.position;

        return transform.position;
    }

    private float GetSpawnRadius()
    {
        if (landRenderer)
        {
            Bounds b = landRenderer.bounds;

            float halfX = b.extents.x;
            float halfZ = b.extents.z;
            float r = Mathf.Min(halfX, halfZ) * radiusFactor;

            r = Mathf.Max(0.1f, r - edgeInset);
            return r;
        }

        return Mathf.Max(0.1f, fixedRadius);
    }

    private Vector3 RandomPointOnCircle(Vector3 center, float radius)
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

        float x = center.x + Mathf.Cos(angle) * radius;
        float z = center.z + Mathf.Sin(angle) * radius;

        return new Vector3(x, center.y, z);
    }

    private bool FarEnoughFromTower(Vector3 p)
    {
        if (!towerTarget)
            return true;

        Vector3 towerXZ = new Vector3(towerTarget.position.x, 0f, towerTarget.position.z);
        Vector3 pXZ = new Vector3(p.x, 0f, p.z);

        return Vector3.Distance(pXZ, towerXZ) >= minDistanceFromTower;
    }

    private bool TryProjectToGround(Vector3 xz, out Vector3 groundPos)
    {
        Vector3 rayStart = new Vector3(xz.x, xz.y + rayStartHeight, xz.z);

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, rayStartHeight * 2f))
        {
            if (hit.collider.CompareTag(groundTag))
            {
                groundPos = hit.point + Vector3.up * groundOffset;
                return true;
            }
        }

        groundPos = default;
        return false;
    }
}