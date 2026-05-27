using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text survivedTimeText;

    private float startTime;
    private bool gameEnded = false;

    private void Start()
    {
        Time.timeScale = 1f;
        startTime = Time.time;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void EndGame()
    {
        if (gameEnded) return;
        gameEnded = true;

        float survivedTime = Time.time - startTime;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (survivedTimeText != null)
        {
            survivedTimeText.text = "Has aguantat " + survivedTime.ToString("F1") + " segons";
        }

        Debug.Log("Game Over. Survived time: " + survivedTime);

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}