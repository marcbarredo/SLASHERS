using System.Collections;
using TMPro;
using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    [Header("Start Screen UI")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private TMP_Text p1StatusText;
    [SerializeField] private TMP_Text p2StatusText;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text survivedTimeText;
    [SerializeField] private float returnToStartDelay = 3f;

    [Header("Gameplay References")]
    [SerializeField] private NinjaSpawner spawner;
    [SerializeField] private TempleHealth templeHealth;
    [SerializeField] private GameObject startDummiesRoot;

    [Header("Start Delay")]
    [SerializeField] private float startRoundDelay = 1.5f;
    private bool roundStarting = false;

    private bool p1Ready;
    private bool p2Ready;
    private bool roundRunning;
    private float roundStartTime;

    private void Start()
    {
        ShowStartScreen();
    }

    public void RegisterPlayerReady(int playerId)
    {
        if (playerId == 1)
            p1Ready = true;
        else if (playerId == 2)
            p2Ready = true;

        UpdateReadyUI();

        if (p1Ready && p2Ready && !roundStarting)
        {
            roundStarting = true;
            StartCoroutine(StartRoundAfterDelay());
        }
    }

    private IEnumerator StartRoundAfterDelay()
    {
        yield return new WaitForSeconds(startRoundDelay);
        StartRound();
    }

    private void StartRound()
    {
        roundRunning = true;
        roundStartTime = Time.time;

        if (startPanel != null)
            startPanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (startDummiesRoot != null)
            startDummiesRoot.SetActive(false);

        if (spawner != null)
            spawner.enabled = true;

        if (templeHealth != null)
            templeHealth.ResetTemple();
    }

    public void OnTowerDestroyed()
    {
        if (!roundRunning) return;

        roundRunning = false;

        float survivedTime = Time.time - roundStartTime;

        if (spawner != null)
            spawner.enabled = false;

        DestroyActiveEnemies();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (survivedTimeText != null)
            survivedTimeText.text = "Has aguantat " + survivedTime.ToString("F1") + " segons";

        StartCoroutine(ReturnToStartScreenAfterDelay());
    }

    private IEnumerator ReturnToStartScreenAfterDelay()
    {
        yield return new WaitForSeconds(returnToStartDelay);
        ShowStartScreen();
    }

    private void ShowStartScreen()
    {
        roundRunning = false;
        roundStarting = false;

        p1Ready = false;
        p2Ready = false;
        UpdateReadyUI();

        if (startPanel != null)
            startPanel.SetActive(true);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (startDummiesRoot != null)
        {
            startDummiesRoot.SetActive(true);

            StartDummyReady[] dummies = startDummiesRoot.GetComponentsInChildren<StartDummyReady>(true);
            foreach (StartDummyReady dummy in dummies)
            {
                dummy.ResetDummy();
            }
        }

        if (spawner != null)
            spawner.enabled = false;

        DestroyActiveEnemies();

        if (templeHealth != null)
            templeHealth.ResetTemple();
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
        {
            Destroy(enemy);
        }
    }
}