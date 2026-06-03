using System.Collections.Generic;
using UnityEngine;

public class NinjaSpawner : MonoBehaviour
{
    public static NinjaSpawner Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Transform towerTarget;

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject[] ninjaPrefabs;
    [SerializeField] private GameObject[] skeletonPrefabs;
    [SerializeField] private GameObject[] golemPrefabs;

    [Header("Base Enemy Speeds")]
    [SerializeField] private float ninjaBaseSpeed = 2.5f;
    [SerializeField] private float skeletonBaseSpeed = 5f;
    [SerializeField] private float golemSpeed = 0.5f;

    [Header("Speed Randomness")]
    [SerializeField] private float startSpeedRandomRange = 0.3f;
    [SerializeField] private float finalSpeedRandomRange = 2f;
    [SerializeField] private float finalExtraSpeed = 1.8f;

    [Header("Map References")]
    [SerializeField] private Renderer landRenderer;
    [SerializeField] private Transform fallbackCenter;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip spawnSound;
    [SerializeField] private AudioClip golemSpawnSound;

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
    [SerializeField] private float rayStartHeight = 6f;
    [SerializeField] private float groundOffset = 0.02f;

    [Header("Difficulty")]
    [Tooltip("This is NOT a maximum anymore. This is the time where difficulty reaches 1. After that it keeps increasing forever.")]
    [SerializeField] private float timeToMaxDifficulty = 150f;

    [Header("Normal Enemy Spawning")]
    [SerializeField] private float startSpawnEverySeconds = 1.8f;
    [SerializeField] private float minSpawnEverySeconds = 0.55f;

    [SerializeField] private int startMaxAlive = 7;
    [SerializeField] private int finalMaxAlive = 26;

    [Header("Safety Limits")]
    [SerializeField] private float absoluteMinSpawnEverySeconds = 0.25f;
    [SerializeField] private int absoluteMaxAlive = 60;
    [SerializeField] private float maxSkeletonChance = 0.9f;
    [SerializeField] private float maxGolemChance = 0.55f;

    [Header("Golem Event Effect")]
    [Tooltip("Higher value = fewer ninjas while golem is alive. 1.5 is a small slowdown. 2.5 is a big slowdown.")]
    [SerializeField] private float spawnSlowdownWhileGolemAlive = 1.5f;

    [SerializeField] private int maxGolemsAlive = 1;

    [Header("Skeletons From Dead Ninjas")]
    [Range(0f, 1f)]
    [SerializeField] private float startSkeletonChance = 0.12f;

    [Range(0f, 1f)]
    [SerializeField] private float finalSkeletonChance = 0.55f;

    [SerializeField] private float skeletonSpawnRadius = 0.4f;

    [Header("Golems")]
    [Tooltip("Time in seconds. 100 = 1 minute and 40 seconds. 120 = 2 minutes. 180 = 3 minutes.")]
    [SerializeField] private float obligatoryGolemTime = 100f;

    [Range(0f, 1f)]
    [SerializeField] private float startGolemChance = 0.08f;

    [Range(0f, 1f)]
    [SerializeField] private float finalGolemChance = 0.30f;

    [SerializeField] private float minimumTimeBetweenGolems = 30f;

    private float timer;
    private float gameTime;
    private float lastGolemSpawnTime = -999f;
    private bool obligatoryGolemSpawned;

    private readonly List<GameObject> aliveEnemies = new List<GameObject>();
    private readonly List<GameObject> aliveGolems = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (towerTarget == null)
        {
            Debug.LogWarning("NinjaSpawner has no tower target assigned.");
            return;
        }

        if (ninjaPrefabs == null || ninjaPrefabs.Length == 0)
        {
            Debug.LogWarning("NinjaSpawner has no ninja prefabs assigned.");
            return;
        }

        gameTime += Time.deltaTime;
        timer += Time.deltaTime;

        CleanLists();

        TrySpawnObligatoryGolem();

        float currentSpawnTime = GetCurrentSpawnEverySeconds();

        if (IsGolemAlive())
        {
            currentSpawnTime *= spawnSlowdownWhileGolemAlive;
        }

        if (timer < currentSpawnTime)
            return;

        timer = 0f;

        if (CountAlive() >= GetCurrentMaxAlive())
            return;

        SpawnEnemyByDifficulty();
    }

    private void TrySpawnObligatoryGolem()
    {
        if (obligatoryGolemSpawned)
            return;

        if (gameTime < obligatoryGolemTime)
            return;

        if (!CanSpawnGolem())
            return;

        SpawnGolem();
        obligatoryGolemSpawned = true;
    }

    public void ResetSpawner()
    {
        timer = 0f;
        gameTime = 0f;

        lastGolemSpawnTime = -999f;
        obligatoryGolemSpawned = false;

        aliveEnemies.Clear();
        aliveGolems.Clear();

        Debug.Log("NinjaSpawner difficulty reset.");
    }

    private void SpawnEnemyByDifficulty()
    {
        if (ShouldSpawnRandomGolem())
        {
            SpawnGolem();
            return;
        }

        float ninjaSpeed = GetRandomizedSpeed(ninjaBaseSpeed);
        SpawnOne(ninjaPrefabs, ninjaSpeed, EnemyType.Ninja);
    }

    private bool ShouldSpawnRandomGolem()
    {
        if (!obligatoryGolemSpawned)
            return false;

        if (!CanSpawnGolem())
            return false;

        if (IsGolemAlive())
            return false;

        if (gameTime - lastGolemSpawnTime < minimumTimeBetweenGolems)
            return false;

        float chance = GetCurrentGolemChance();

        return Random.value <= chance;
    }

    private void SpawnGolem()
    {
        GameObject golem = SpawnOne(golemPrefabs, golemSpeed, EnemyType.Golem);

        if (golem != null)
        {
            aliveGolems.Add(golem);
            lastGolemSpawnTime = gameTime;

            if (audioSource != null && golemSpawnSound != null)
            {
                audioSource.PlayOneShot(golemSpawnSound);
            }
        }
    }

    public void TrySpawnSkeletonFromNinjaDeath(Vector3 deathPosition)
    {
        if (!CanSpawnSkeleton())
            return;

        if (CountAlive() >= GetCurrentMaxAlive())
            return;

        float chance = GetCurrentSkeletonChance();

        if (Random.value > chance)
            return;

        Vector3 spawnPosition = deathPosition;

        if (skeletonSpawnRadius > 0f)
        {
            Vector2 randomCircle = Random.insideUnitCircle * skeletonSpawnRadius;

            spawnPosition = new Vector3(
                deathPosition.x + randomCircle.x,
                deathPosition.y,
                deathPosition.z + randomCircle.y
            );
        }

        if (TryProjectToGround(spawnPosition, out Vector3 groundPos))
        {
            spawnPosition = groundPos;
        }

        float skeletonSpeed = GetRandomizedSpeed(skeletonBaseSpeed);
        SpawnAtPosition(skeletonPrefabs, spawnPosition, skeletonSpeed, EnemyType.Skeleton);
    }

    private GameObject SpawnOne(GameObject[] prefabList, float speed, EnemyType enemyType)
    {
        if (prefabList == null || prefabList.Length == 0)
        {
            Debug.LogWarning("Tried to spawn " + enemyType + " but its prefab list is empty.");
            return null;
        }

        if (!TryGetSpawnPosition(out Vector3 pos))
        {
            Debug.LogWarning("Spawner could not find a valid spawn position.");
            return null;
        }

        return SpawnAtPosition(prefabList, pos, speed, enemyType);
    }

    private GameObject SpawnAtPosition(GameObject[] prefabList, Vector3 pos, float speed, EnemyType enemyType)
    {
        if (prefabList == null || prefabList.Length == 0)
        {
            Debug.LogWarning("Tried to spawn " + enemyType + " but its prefab list is empty.");
            return null;
        }

        GameObject prefabToSpawn = prefabList[Random.Range(0, prefabList.Length)];

        if (prefabToSpawn == null)
        {
            Debug.LogWarning("One prefab inside " + enemyType + " prefab list is empty.");
            return null;
        }

        Quaternion rotation = GetRotationFacingTower(pos);

        GameObject enemy = Instantiate(prefabToSpawn, pos, rotation);

        aliveEnemies.Add(enemy);

        if (audioSource != null && spawnSound != null && enemyType != EnemyType.Golem)
        {
            audioSource.PlayOneShot(spawnSound);
        }

        SetupEnemyMovement(enemy, speed, enemyType);

        return enemy;
    }

    private void SetupEnemyMovement(GameObject enemy, float speed, EnemyType enemyType)
    {
        if (enemyType == EnemyType.Skeleton)
        {
            SkeletonMover skeletonMover = enemy.GetComponent<SkeletonMover>();

            if (skeletonMover == null)
                skeletonMover = enemy.GetComponentInChildren<SkeletonMover>();

            if (skeletonMover != null)
            {
                skeletonMover.SetTarget(towerTarget);
                skeletonMover.SetSpeed(speed);
                return;
            }

            Debug.LogWarning(enemy.name + " has no SkeletonMover.");
            return;
        }

        if (enemyType == EnemyType.Golem)
        {
            GolemMover golemMover = enemy.GetComponent<GolemMover>();

            if (golemMover == null)
                golemMover = enemy.GetComponentInChildren<GolemMover>();

            if (golemMover != null)
            {
                golemMover.SetTarget(towerTarget);
                golemMover.SetSpeed(speed);
                return;
            }

            Debug.LogWarning(enemy.name + " has no GolemMover.");
            return;
        }

        NinjaMover ninjaMover = enemy.GetComponent<NinjaMover>();

        if (ninjaMover == null)
            ninjaMover = enemy.GetComponentInChildren<NinjaMover>();

        if (ninjaMover != null)
        {
            ninjaMover.enabled = true;
            ninjaMover.SetTarget(towerTarget);
            ninjaMover.SetSpeed(speed);
            return;
        }

        Debug.LogWarning(enemy.name + " has no NinjaMover.");
    }

    private float GetDifficulty()
    {
        if (timeToMaxDifficulty <= 0f)
            return 1f;

        return gameTime / timeToMaxDifficulty;
    }

    private float GetCurrentSpawnEverySeconds()
    {
        float difficulty = GetDifficulty();

        float spawnTime = Mathf.Lerp(startSpawnEverySeconds, minSpawnEverySeconds, difficulty);

        return Mathf.Max(absoluteMinSpawnEverySeconds, spawnTime);
    }

    private int GetCurrentMaxAlive()
    {
        float difficulty = GetDifficulty();

        int maxAlive = Mathf.RoundToInt(Mathf.Lerp(startMaxAlive, finalMaxAlive, difficulty));

        return Mathf.Clamp(maxAlive, startMaxAlive, absoluteMaxAlive);
    }

    private float GetCurrentSkeletonChance()
    {
        float difficulty = GetDifficulty();

        float chance = Mathf.Lerp(startSkeletonChance, finalSkeletonChance, difficulty);

        return Mathf.Clamp(chance, 0f, maxSkeletonChance);
    }

    private float GetCurrentGolemChance()
    {
        if (gameTime < obligatoryGolemTime)
            return 0f;

        float difficulty = (gameTime - obligatoryGolemTime) / timeToMaxDifficulty;

        float chance = Mathf.Lerp(startGolemChance, finalGolemChance, difficulty);

        return Mathf.Clamp(chance, 0f, maxGolemChance);
    }

    private float GetRandomizedSpeed(float baseSpeed)
    {
        float difficulty = GetDifficulty();

        float extraSpeed = Mathf.Lerp(0f, finalExtraSpeed, difficulty);
        float randomRange = Mathf.Lerp(startSpeedRandomRange, finalSpeedRandomRange, difficulty);

        float minSpeed = baseSpeed + extraSpeed - randomRange;
        float maxSpeed = baseSpeed + extraSpeed + randomRange;

        minSpeed = Mathf.Max(0.1f, minSpeed);

        return Random.Range(minSpeed, maxSpeed);
    }

    private void CleanLists()
    {
        aliveEnemies.RemoveAll(enemy => enemy == null);
        aliveGolems.RemoveAll(golem => golem == null);
    }

    private int CountAlive()
    {
        CleanLists();
        return aliveEnemies.Count;
    }

    private bool IsGolemAlive()
    {
        CleanLists();
        return aliveGolems.Count > 0;
    }

    private bool CanSpawnSkeleton()
    {
        return skeletonPrefabs != null && skeletonPrefabs.Length > 0;
    }

    private bool CanSpawnGolem()
    {
        CleanLists();

        if (golemPrefabs == null || golemPrefabs.Length == 0)
            return false;

        return aliveGolems.Count < maxGolemsAlive;
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

    private Quaternion GetRotationFacingTower(Vector3 spawnPosition)
    {
        if (!towerTarget)
            return Quaternion.identity;

        Vector3 directionToTower = towerTarget.position - spawnPosition;
        directionToTower.y = 0f;

        if (directionToTower.sqrMagnitude > 0.01f)
            return Quaternion.LookRotation(directionToTower.normalized, Vector3.up);

        return Quaternion.identity;
    }

    private enum EnemyType
    {
        Ninja,
        Skeleton,
        Golem
    }
}