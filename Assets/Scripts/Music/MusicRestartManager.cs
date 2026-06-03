using UnityEngine;

public class MusicRestartManager : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] private AudioSource musicSource;

    [Header("Settings")]
    [SerializeField] private bool playOnStart = true;

    private void Awake()
    {
        if (musicSource == null)
            musicSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (playOnStart)
        {
            RestartMusic();
        }
    }

    public void RestartMusic()
    {
        if (musicSource == null)
        {
            Debug.LogWarning("MusicRestartManager has no music source assigned.");
            return;
        }

        musicSource.Stop();
        musicSource.time = 0f;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource == null)
            return;

        musicSource.Stop();
    }

    public void PauseMusic()
    {
        if (musicSource == null)
            return;

        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        if (musicSource == null)
            return;

        musicSource.UnPause();
    }
}