using UnityEngine;

public class NinjaSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform towerTarget;
    [SerializeField] private GameObject ninjaPrefab;
    [SerializeField] private float ninjaSpeed = 2.0f;

    [SerializeField] private Renderer landRenderer;
    [SerializeField] private Transform fallbackCenter;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip spawnSound;

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

    private float _timer;

    void Update()
    {
        if (!towerTarget || !ninjaPrefab) return;

        _timer += Time.deltaTime;
        if (_timer < spawnEverySeconds) return;
        _timer = 0f;

        if (CountAlive() >= maxAlive) return;

        SpawnOne();
    }

    int CountAlive()
    {
        return FindObjectsByType<NinjaMover>(FindObjectsSortMode.None).Length;
    }

    void SpawnOne()
    {
        if (!TryGetSpawnPosition(out Vector3 pos))
            return;

        GameObject ninja = Instantiate(ninjaPrefab, pos, Quaternion.identity);

        if (audioSource != null && spawnSound != null)
        {
            audioSource.PlayOneShot(spawnSound);
        }

        var mover = ninja.GetComponent<NinjaMover>();
        if (mover != null)
        {
            mover.SetTarget(towerTarget);
            mover.SetSpeed(ninjaSpeed);
        }
    }

    bool TryGetSpawnPosition(out Vector3 pos)
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

    Vector3 GetSpawnCenter()
    {
        if (landRenderer) return landRenderer.bounds.center;
        if (fallbackCenter) return fallbackCenter.position;
        return towerTarget.position;
    }

    float GetSpawnRadius()
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

    Vector3 RandomPointOnCircle(Vector3 center, float radius)
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float x = center.x + Mathf.Cos(angle) * radius;
        float z = center.z + Mathf.Sin(angle) * radius;

        return new Vector3(x, center.y, z);
    }

    bool FarEnoughFromTower(Vector3 p)
    {
        Vector3 towerXZ = new Vector3(towerTarget.position.x, 0f, towerTarget.position.z);
        Vector3 pXZ = new Vector3(p.x, 0f, p.z);
        return Vector3.Distance(pXZ, towerXZ) >= minDistanceFromTower;
    }

    bool TryProjectToGround(Vector3 xz, out Vector3 groundPos)
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