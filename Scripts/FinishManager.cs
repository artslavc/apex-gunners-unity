using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class FinishManager : MonoBehaviour
{
    public static FinishManager Instance;

    [Header("Настройки завершения уровня")]
    public int botsNeededToFinish = 5;
    public string nextSceneName;
    public float delayBeforeLoad = 2f;

    private int currentDeaths = 0;
    private bool levelComplete = false;

    void Awake()
    {
        // Инициализация Instance
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterBotDeath()
    {
        if (levelComplete) return;

        currentDeaths++;
        Debug.Log($"Ботов убито: {currentDeaths} из {botsNeededToFinish}");

        if (currentDeaths >= botsNeededToFinish)
        {
            levelComplete = true;
            StartCoroutine(FinishLevel());
        }
    }

    IEnumerator FinishLevel()
    {
        yield return new WaitForSeconds(delayBeforeLoad);

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("Забыл указать имя следующей сцены в Инспекторе!");
        }
    }
}