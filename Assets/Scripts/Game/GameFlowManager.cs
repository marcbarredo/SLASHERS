using System.Collections;
using TMPro;
using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    [Header("Start Screen UI")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private TextMesh p1StatusText;
    [SerializeField] private TextMesh p2StatusText;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text survivedTimeText;
    [SerializeField] private float returnToStartDelay = 3f;

    [Header("Gameplay References")]
    [SerializeField] private NinjaSpawner spawner;
    [SerializeField] private TempleHealth templeHealth;

    [Tooltip("Put Environment here. This is the gameplay world/tower/arena that appears only during the round.")]
    [SerializeField] private GameObject objectToAppearOnStart;

    [Header("Start Dummies")]
    [SerializeField] private GameObject startDummiesRoot;

    [SerializeField] private GameObject dummyP1Prefab;
    [SerializeField] private GameObject dummyP2Prefab;

    [SerializeField] private Transform dummyP1Spawn;
    [SerializeField] private Transform dummyP2Spawn;

    [SerializeField] private Transform player1BladeRoot;
    [SerializeField] private Transform player2BladeRoot;

    private GameObject currentDummyP1;
    private GameObject currentDummyP2;

    [Header("Audio")]
    [SerializeField] private MusicRestartManager musicRestartManager;

    [Header("Gameplay UI")]
    [SerializeField] private TMP_Text timerText;

    [Header("Start Delay")]
    [SerializeField] private float startRoundDelay = 1.5f;

    private bool roundStarting;
    private bool p1Ready;
    private bool p2Ready;
    private bool roundRunning;

    private float roundStartTime;
    private Coroutine startRoundCoroutine;
    private Coroutine returnToStartCoroutine;

    private void Start()
    {
        ShowStartScreen(true);
    }

    private void Update()
    {
        if (!roundRunning)
            return;

        float elapsedTime = Time.time - roundStartTime;

        if (timerText != null)
            timerText.text = FormatTime(elapsedTime);
    }

    public void RegisterPlayerReady(int playerId)
    {
        if (roundRunning || roundStarting)
            return;

        if (playerId == 1)
            p1Ready = true;
        else if (playerId == 2)
            p2Ready = true;

        UpdateReadyUI();

        if (p1Ready && p2Ready)
        {
            roundStarting = true;

            if (startRoundCoroutine != null)
                StopCoroutine(startRoundCoroutine);

            startRoundCoroutine = StartCoroutine(StartRoundAfterDelay());
        }
    }

    private IEnumerator StartRoundAfterDelay()
    {
        yield return new WaitForSeconds(startRoundDelay);
        StartRound();
    }

    private void StartRound()
    {
        roundStarting = false;
        roundRunning = true;
        roundStartTime = Time.time;

        if (startPanel != null)
            startPanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Show environment/tower/arena when game starts.
        if (objectToAppearOnStart != null)
            objectToAppearOnStart.SetActive(true);

        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
            timerText.text = "0:00.0";
        }

        // Remove start dummies when round begins.
        DestroyStartDummies();

        // Reset tower health after environment is active.
        if (templeHealth != null)
        {
            templeHealth.gameObject.SetActive(true);
            templeHealth.ResetTemple();
        }
        else
        {
            Debug.LogWarning("GameFlowManager has no TempleHealth assigned.");
        }

        // Start enemy spawning.
        if (spawner != null)
        {
            spawner.gameObject.SetActive(true);
            spawner.ResetSpawner();
            spawner.enabled = true;
        }
        else
        {
            Debug.LogWarning("GameFlowManager has no Spawner assigned.");
        }
    }

    public void OnTowerDestroyed()
    {
        if (!roundRunning)
            return;

        roundRunning = false;

        float survivedTime = Time.time - roundStartTime;

        if (timerText != null)
            timerText.gameObject.SetActive(false);

        if (spawner != null)
            spawner.enabled = false;

        DestroyActiveEnemies();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (survivedTimeText != null)
            survivedTimeText.text = "You have survived " + FormatTime(survivedTime);

        if (returnToStartCoroutine != null)
            StopCoroutine(returnToStartCoroutine);

        returnToStartCoroutine = StartCoroutine(ReturnToStartScreenAfterDelay());
    }

    private IEnumerator ReturnToStartScreenAfterDelay()
    {
        yield return new WaitForSeconds(returnToStartDelay);
        ShowStartScreen(true);
    }

    private void ShowStartScreen(bool restartMusic)
    {
        roundRunning = false;
        roundStarting = false;

        p1Ready = false;
        p2Ready = false;

        if (restartMusic)
            RestartMusic();

        UpdateReadyUI();

        if (spawner != null)
            spawner.enabled = false;

        DestroyActiveEnemies();

        // Hide/reset gameplay world when returning to menu.
        if (objectToAppearOnStart != null)
            objectToAppearOnStart.SetActive(false);

        if (templeHealth != null)
            templeHealth.ResetTemple();

        if (startPanel != null)
            startPanel.SetActive(true);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
            timerText.text = "0:00.0";
        }

        RespawnStartDummies();
    }

    private void RespawnStartDummies()
    {
        if (startDummiesRoot != null)
            startDummiesRoot.SetActive(true);

        DestroyStartDummies();

        if (dummyP1Prefab != null && dummyP1Spawn != null)
        {
            currentDummyP1 = Instantiate(
                dummyP1Prefab,
                dummyP1Spawn.position,
                dummyP1Prefab.transform.rotation,
                startDummiesRoot != null ? startDummiesRoot.transform : null
            );

            SetupDummy(currentDummyP1, 1, player1BladeRoot);
        }
        else
        {
            Debug.LogWarning("Dummy P1 Prefab or Dummy P1 Spawn is not assigned.");
        }

        if (dummyP2Prefab != null && dummyP2Spawn != null)
        {
            currentDummyP2 = Instantiate(
                dummyP2Prefab,
                dummyP2Spawn.position,
                dummyP2Prefab.transform.rotation,
                startDummiesRoot != null ? startDummiesRoot.transform : null
            );

            SetupDummy(currentDummyP2, 2, player2BladeRoot);
        }
        else
        {
            Debug.LogWarning("Dummy P2 Prefab or Dummy P2 Spawn is not assigned.");
        }
    }

    private void DestroyStartDummies()
    {
        if (currentDummyP1 != null)
        {
            Destroy(currentDummyP1);
            currentDummyP1 = null;
        }

        if (currentDummyP2 != null)
        {
            Destroy(currentDummyP2);
            currentDummyP2 = null;
        }
    }

    private void SetupDummy(GameObject dummy, int playerId, Transform bladeRoot)
    {
        if (dummy == null)
            return;

        StartDummyReady ready = dummy.GetComponent<StartDummyReady>();

        if (ready == null)
            ready = dummy.GetComponentInChildren<StartDummyReady>();

        if (ready != null)
        {
            ready.Setup(playerId, this, bladeRoot);
            ready.ResetDummy();
        }
        else
        {
            Debug.LogWarning(dummy.name + " has no StartDummyReady script.");
        }
    }

    private void RestartMusic()
    {
        if (musicRestartManager == null)
            musicRestartManager = FindFirstObjectByType<MusicRestartManager>();

        if (musicRestartManager != null)
            musicRestartManager.RestartMusic();
    }

    private void UpdateReadyUI()
    {
        if (p1StatusText != null)
            p1StatusText.text = p1Ready ? "P1 READY" : "P1 WAITING";

        if (p2StatusText != null)
            p2StatusText.text = p2Ready ? "P2 READY" : "P2 WAITING";
    }

    private void DestroyActiveEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
            Destroy(enemy);
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int tenths = Mathf.FloorToInt((time * 10f) % 10f);

        return minutes + ":" + seconds.ToString("00") + "." + tenths;
    }
}