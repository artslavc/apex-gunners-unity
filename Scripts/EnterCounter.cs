using UnityEngine;
using UnityEngine.SceneManagement;

public class EnterCounter : MonoBehaviour
{
    [Header("Настройки звука")]
    public AudioSource audioSource;
    public AudioClip enterSound;

    [Header("Настройки текста")]
    public string startMessage = "Нажми ENTER";
    public string messageAt5 = "Достигнуто 5 нажатий!";
    public string messageAt7 = ""; // Пустое сообщение для 7 нажатий
    public string killMessage = "KILL"; // Красная надпись после 10 нажатий
    public Color textColor = Color.white;
    public Color killColor = Color.red; // Красный цвет для KILL
    public int fontSize = 30;

    [Header("Настройки сцен")]
    public string targetSceneName = "GameScene";
    public float loadDelayAfterKill = 6f; // Задержка 6 секунд после KILL

    [Header("Счетчики")]
    public int currentCount = 0;

    private bool messageShownAt5 = false;
    private bool messageHiddenAt7 = false;
    private bool killModeActive = false; // Режим KILL активирован
    private string currentMessage = "";
    private Color currentColor;
    private float pulseScale = 1f;
    private bool isPulsing = false;
    private float killTimer = 0f;

    void Start()
    {
        // Устанавливаем начальное сообщение и цвет
        currentMessage = startMessage;
        currentColor = textColor;
        Debug.Log("Начальное сообщение: " + startMessage);

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    void Update()
    {
        // Проверяем нажатие Enter
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnEnterPressed();
        }

        // Обновляем таймер в режиме KILL
        if (killModeActive)
        {
            killTimer -= Time.deltaTime;

            // Каждую секунду пульсация
            if (Mathf.Abs(killTimer - Mathf.Floor(killTimer)) < 0.1f)
            {
                if (!isPulsing)
                {
                    StartCoroutine(PulseText());
                }
            }

            // Когда таймер доходит до 0 - загружаем сцену
            if (killTimer <= 0)
            {
                killModeActive = false;
                LoadTargetScene();
            }
        }
    }

    void OnGUI()
    {
        // Настройка стиля текста
        GUIStyle style = new GUIStyle();
        style.fontSize = Mathf.RoundToInt(fontSize * pulseScale);
        style.normal.textColor = currentColor;
        style.alignment = TextAnchor.MiddleCenter;
        style.fontStyle = FontStyle.Bold;

        // Отображаем текст в центре экрана
        if (!string.IsNullOrEmpty(currentMessage))
        {
            float textWidth = 400f;
            float textHeight = 100f;
            Rect textRect = new Rect(Screen.width / 2 - textWidth / 2,
                                     Screen.height / 2 - textHeight / 2,
                                     textWidth, textHeight);

            GUI.Label(textRect, currentMessage, style);
        }

        // Показываем счетчик нажатий в углу
        GUIStyle counterStyle = new GUIStyle();
        counterStyle.fontSize = 20;
        counterStyle.normal.textColor = killModeActive ? killColor : Color.gray;
        counterStyle.alignment = TextAnchor.UpperRight;

        Rect counterRect = new Rect(Screen.width - 150, 20, 130, 30);

        if (killModeActive)
        {
            // В режиме KILL показываем таймер
            int secondsLeft = Mathf.CeilToInt(killTimer);
        }
        else
        {
        }
    }

    void OnEnterPressed()
    {
        // Если режим KILL активен, просто увеличиваем счетчик и обновляем пульсацию
        if (killModeActive)
        {
            currentCount++;
            Debug.Log($"KILL MODE - Нажатие {currentCount}");

            // Звук
            if (audioSource != null && enterSound != null)
            {
                audioSource.PlayOneShot(enterSound);
            }

            // Пульсация
            StartCoroutine(PulseText());

            // Сбрасываем таймер при каждом нажатии в режиме KILL
            killTimer = loadDelayAfterKill;

            // Делаем надпись еще более интенсивной
            StartCoroutine(IntensePulse());

            return;
        }

        // Обычный режим - увеличиваем счетчик
        currentCount++;
        Debug.Log($"Enter нажат {currentCount} раз");

        // Звук
        if (audioSource != null && enterSound != null)
        {
            audioSource.PlayOneShot(enterSound);
        }

        // Пульсация
        StartCoroutine(PulseText());

        // Проверка условий
        if (currentCount >= 10)
        {
            // Активируем режим KILL
            EnterKillMode();
        }
        else if (currentCount >= 7 && !messageHiddenAt7)
        {
            currentMessage = messageAt7;
            currentColor = textColor;
            messageHiddenAt7 = true;
            Debug.Log("Текст скрыт (7 нажатий)");
        }
        else if (currentCount >= 5 && !messageShownAt5)
        {
            currentMessage = messageAt5;
            currentColor = textColor;
            messageShownAt5 = true;
            Debug.Log("Показан текст: " + messageAt5);
        }
        else if (currentCount < 5)
        {
            // Если меньше 5 нажатий, показываем начальное сообщение
            currentMessage = startMessage;
            currentColor = textColor;
        }
    }

    void EnterKillMode()
    {
        killModeActive = true;
        currentMessage = killMessage;
        currentColor = killColor;
        killTimer = loadDelayAfterKill;

        // Специальный эффект для входа в KILL режим
        StartCoroutine(KillModeEntryEffect());

        Debug.Log("РЕЖИМ KILL АКТИВИРОВАН! Нажимай Enter для продления таймера");
    }

    System.Collections.IEnumerator KillModeEntryEffect()
    {
        // Серия пульсаций при входе в режим KILL
        for (int i = 0; i < 3; i++)
        {
            float elapsedTime = 0f;
            float duration = 0.15f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                pulseScale = Mathf.Lerp(1f, 1.8f, t);
                fontSize = Mathf.RoundToInt(40 * pulseScale);
                yield return null;
            }

            elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                pulseScale = Mathf.Lerp(1.8f, 1f, t);
                fontSize = Mathf.RoundToInt(40 * pulseScale);
                yield return null;
            }
        }

        pulseScale = 1f;
    }

    System.Collections.IEnumerator IntensePulse()
    {
        // Более интенсивная пульсация для режима KILL
        float elapsedTime = 0f;
        float duration = 0.1f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            pulseScale = Mathf.Lerp(1f, 1.6f, t);
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            pulseScale = Mathf.Lerp(1.6f, 1f, t);
            yield return null;
        }

        pulseScale = 1f;
    }

    System.Collections.IEnumerator PulseText()
    {
        isPulsing = true;

        float elapsedTime = 0f;
        float pulseDuration = killModeActive ? 0.15f : 0.2f;
        float targetPulse = killModeActive ? 1.5f : 1.3f;

        // Увеличиваем
        while (elapsedTime < pulseDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (pulseDuration / 2);
            pulseScale = Mathf.Lerp(1f, targetPulse, t);
            yield return null;
        }

        elapsedTime = 0f;

        // Уменьшаем
        while (elapsedTime < pulseDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (pulseDuration / 2);
            pulseScale = Mathf.Lerp(targetPulse, 1f, t);
            yield return null;
        }

        pulseScale = 1f;
        isPulsing = false;
    }

    void LoadTargetScene()
    {
        Debug.Log("Загрузка сцены: " + targetSceneName);
        SceneManager.LoadScene(targetSceneName);
    }

    public void ResetCounter()
    {
        currentCount = 0;
        messageShownAt5 = false;
        messageHiddenAt7 = false;
        killModeActive = false;
        currentMessage = startMessage;
        currentColor = textColor;
        Debug.Log("Счетчик сброшен");
    }
}