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
        if (hasSpawned)
            return;

        if (golemPrefab == null)
        {
            Debug.LogWarning("GolemSpawner has no golem prefab assigned.");
            return;
        }

        if (towerTarget == null)
        {
            Debug.LogWarning("GolemSpawner has no tower target assigned.");
            return;
        }

        hasSpawned = true;

        Vector3 spawnPosition = GetRandomPositionAroundTower();

        if (TryProjectToGround(spawnPosition, out Vector3 groundPosition))
        {
            spawnPosition = groundPosition;
        }

        Quaternion rotation = GetRotationFacingTower(spawnPosition);

        GameObject golem = Instantiate(golemPrefab, spawnPosition, rotation);

        Debug.Log("GOLEM SPAWNED: " + golem.name);

        if (audioSource != null && spawnSound != null)
        {
            audioSource.PlayOneShot(spawnSound);
        }

        NinjaMover mover = golem.GetComponent<NinjaMover>();

        if (mover == null)
            mover = golem.GetComponentInChildren<NinjaMover>();

        if (mover == null)
        {
            Debug.LogError("Golem has no NinjaMover on root or children.");
            return;
        }

        mover.enabled = true;
        mover.SetTarget(towerTarget);
        mover.SetSpeed(golemSpeed);

        Animator animator = golem.GetComponentInChildren<Animator>();

        if (animator != null)
            animator.applyRootMotion = false;

        Debug.Log("GOLEM TARGET SET TO: " + towerTarget.name);
        Debug.Log("GOLEM SPEED SET TO: " + golemSpeed);
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
        Vector3 directionToTower = towerTarget.position - spawnPosition;
        directionToTower.y = 0f;

        if (directionToTower.sqrMagnitude > 0.01f)
            return Quaternion.LookRotation(directionToTower.normalized, Vector3.up);

        return Quaternion.identity;
    }

    private bool TryProjectToGround(Vector3 position, out Vector3 groundPosition)
    {
        Vector3 rayStart = new Vector3(
            position.x,
            position.y + rayStartHeight,
            position.z
        );

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
}