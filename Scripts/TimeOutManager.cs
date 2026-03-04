using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TimeOutManager : MonoBehaviour
{
    public enum ExitAction { LoadScene, QuitGame }

    [Header("Настройки таймера")]
    [Tooltip("Время в секундах до выполнения действия")]
    public float delaySeconds = 10f;

    [Header("Действие")]
    public ExitAction action = ExitAction.LoadScene;

    [Tooltip("Имя сцены (используется, если выбрано LoadScene)")]
    public string sceneToLoad;

    void Start()
    {
        StartCoroutine(StartTimer());
    }

    IEnumerator StartTimer()
    {
        yield return new WaitForSecondsRealtime(delaySeconds);

        ExecuteAction();
    }

    void ExecuteAction()
    {
        if (action == ExitAction.QuitGame)
        {
            Debug.Log("Выход из игры");
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
        else if (action == ExitAction.LoadScene)
        {
            if (!string.IsNullOrEmpty(sceneToLoad))
            {
                Debug.Log("Загрузка сцены: " + sceneToLoad);
                SceneManager.LoadScene(sceneToLoad);
            }
            else
            {
                Debug.LogError("Имя сцены не указано!");
            }
        }
    }
}