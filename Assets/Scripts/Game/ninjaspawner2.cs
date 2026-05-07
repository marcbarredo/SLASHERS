using UnityEngine;

public class NinjaTempleSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject ninjaPrefab;
    [SerializeField] private GameObject japaneseTemple;

    [Header("Spawn Area")]
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;
    [SerializeField] private float fixedY = 15f;
    [SerializeField] private float fixedZ = -12f;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnEverySeconds = 2f;
    [SerializeField] private int maxAlive = 15;
    [SerializeField] private float ninjaSpeed = 3f;

    private float timer;

    private void Update()
    {
        if (!ninjaPrefab || !japaneseTemple) return;

        timer += Time.deltaTime;

        if (timer < spawnEverySeconds) return;

        timer = 0f;

        if (CountAliveNinjas() >= maxAlive) return;

        SpawnNinja();
    }

    private void SpawnNinja()
    {
        Vector3 spawnPosition = new Vector3(
            Random.Range(minX, maxX),
            fixedY,
            fixedZ
        );

        GameObject ninja = Instantiate(
            ninjaPrefab,
            spawnPosition,
            Quaternion.identity
        );

        NinjaTempleEnemy enemy = ninja.GetComponent<NinjaTempleEnemy>();

        if (enemy)
        {
            enemy.SetTarget(japaneseTemple);
            enemy.SetSpeed(ninjaSpeed);
        }
    }

    private int CountAliveNinjas()
    {
        return FindObjectsByType<NinjaTempleEnemy>(FindObjectsSortMode.None).Length;
    }
}