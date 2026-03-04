using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseText;
    public AudioSource soundToMute;
    public AudioSource soundToPlay;

    private bool isPaused = false;

    void Start()
    {
        if (pauseText != null)
            pauseText.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }

        if (isPaused && Input.GetKeyDown(KeyCode.Alpha1))
        {
            QuitGame();
        }
    }

    void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pauseText != null)
            pauseText.SetActive(true);

        if (soundToMute != null)
            soundToMute.Pause();

        if (soundToPlay != null)
            soundToPlay.Play();
    }

    void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pauseText != null)
            pauseText.SetActive(false);

        if (soundToMute != null)
            soundToMute.UnPause();

        if (soundToPlay != null)
            soundToPlay.Stop();
    }

    void QuitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}