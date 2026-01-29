using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

// Используем псевдоним для избежания конфликтов
using UnityButton = UnityEngine.UI.Button;
using UnityImage = UnityEngine.UI.Image;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Recording UI")]
    [SerializeField] private GameObject recordingPanel;
    [SerializeField] private UnityImage recordTimerFill;
    [SerializeField] private TMP_Text recordTimerText;
    [SerializeField] private Color readyColor = Color.green;
    [SerializeField] private Color recordingColor = Color.red;
    [SerializeField] private Color cooldownColor = Color.yellow;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private UnityButton resumeButton;
    [SerializeField] private UnityButton restartButton;
    [SerializeField] private UnityButton menuButton;

    [Header("Level Complete")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private TMP_Text levelCompleteText;
    [SerializeField] private UnityButton nextLevelButton;
    [SerializeField] private UnityButton levelCompleteRestartButton;
    [SerializeField] private UnityButton levelCompleteMenuButton;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverText;
    [SerializeField] private UnityButton gameOverRestartButton;
    [SerializeField] private UnityButton gameOverMenuButton;

    private bool isRecording = false;
    private float maxRecordDuration = 5f;
    private float currentRecordTime = 0f;
    private bool isPaused = false;
    private bool isGameOver = false;
    private bool isLevelComplete = false;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        // Инициализация панели записи
        if (recordingPanel != null)
        {
            recordingPanel.SetActive(true);
            if (recordTimerFill != null)
            {
                recordTimerFill.color = readyColor;
                recordTimerFill.fillAmount = 1f;
            }
            if (recordTimerText != null)
            {
                recordTimerText.text = "✓";
            }
        }
        else
        {
            Debug.LogError("Recording Panel не назначен в UIManager!");
        }

        // Инициализация меню паузы (скрываем)
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
            
            // Назначаем обработчики кнопок
            if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
            if (restartButton != null) restartButton.onClick.AddListener(RestartLevel);
            if (menuButton != null) menuButton.onClick.AddListener(LoadMainMenu);
        }

        // Инициализация панели завершения уровня (скрываем)
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
            
            if (nextLevelButton != null) nextLevelButton.onClick.AddListener(LoadNextLevel);
            if (levelCompleteRestartButton != null) levelCompleteRestartButton.onClick.AddListener(RestartLevel);
            if (levelCompleteMenuButton != null) levelCompleteMenuButton.onClick.AddListener(LoadMainMenu);
        }

        // Инициализация панели поражения (скрываем)
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
            
            if (gameOverRestartButton != null) gameOverRestartButton.onClick.AddListener(RestartLevel);
            if (gameOverMenuButton != null) gameOverMenuButton.onClick.AddListener(LoadMainMenu);
        }
    }

    private void Update()
    {
        // Обновление таймера записи
        if (isRecording)
        {
            UpdateRecordingTimer();
        }

        // Обработка паузы по нажатию ESC (только если не завершен уровень и не поражение)
        if (Input.GetKeyDown(KeyCode.Escape) && !isLevelComplete && !isGameOver)
        {
            TogglePause();
        }
    }

    #region Пауза
    // Метод для переключения паузы
    public void TogglePause()
    {
        isPaused = !isPaused;
        
        // Останавливаем/возобновляем время
        Time.timeScale = isPaused ? 0f : 1f;
        
        // Показываем/скрываем меню паузы
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(isPaused);
        }
        
        // Показываем/скрываем курсор
        Cursor.visible = isPaused;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        
        Debug.Log(isPaused ? "Игра приостановлена" : "Игра возобновлена");
    }

    // Методы для кнопок меню паузы
    public void ResumeGame()
    {
        TogglePause();
    }
    #endregion

    #region Запись
    // Существующие методы для управления записью
    public void StartRecording()
    {
        if (isRecording || isPaused || isLevelComplete || isGameOver) return;
        
        isRecording = true;
        currentRecordTime = 0f;
        
        if (recordTimerFill != null)
        {
            recordTimerFill.color = recordingColor;
        }
        
        if (recordTimerText != null)
        {
            recordTimerText.text = maxRecordDuration.ToString();
        }
        
        Debug.Log("Запись начата");
    }

    public void StopRecording()
    {
        if (!isRecording) return;
        
        isRecording = false;
        
        if (recordTimerFill != null)
        {
            recordTimerFill.color = cooldownColor;
            recordTimerFill.fillAmount = 1f;
        }
        
        if (recordTimerText != null)
        {
            recordTimerText.text = "✓";
        }
        
        // Через 2 секунды возвращаем в готовность
        Invoke("ResetToReady", 2f);
        
        Debug.Log("Запись остановлена");
    }
    
    public void UpdateRecordTimer(float timeRemaining)
    {
        if (!isRecording) return;
        
        if (recordTimerFill != null)
        {
            float fillAmount = Mathf.Clamp01(timeRemaining / maxRecordDuration);
            recordTimerFill.fillAmount = fillAmount;
        }

        if (recordTimerText != null && timeRemaining > 0)
        {
            recordTimerText.text = Mathf.CeilToInt(timeRemaining).ToString();
            
            // Мигание при малом времени
            if (timeRemaining < 2f)
            {
                recordTimerText.color = Color.Lerp(Color.white, Color.red, Mathf.PingPong(Time.time * 5f, 1f));
            }
            else
            {
                recordTimerText.color = Color.white;
            }
        }
    }

    private void UpdateRecordingTimer()
    {
        if (!isRecording) return;
        
        currentRecordTime += Time.deltaTime;
        float timeRemaining = Mathf.Max(0f, maxRecordDuration - currentRecordTime);
        
        UpdateRecordTimer(timeRemaining);
        
        // Автоматическая остановка при достижении лимита
        if (currentRecordTime >= maxRecordDuration)
        {
            StopRecording();
        }
    }

    private void ResetToReady()
    {
        if (!isRecording)
        {
            if (recordTimerFill != null)
            {
                recordTimerFill.color = readyColor;
            }
            
            if (recordTimerText != null)
            {
                recordTimerText.text = "✓";
                recordTimerText.color = Color.white;
            }
        }
    }
    #endregion

    #region Завершение уровня
    // Метод для отображения панели завершения уровня
    public void ShowLevelComplete(string levelName = "")
    {
        if (isLevelComplete) return;
        
        isLevelComplete = true;
        Time.timeScale = 0f; // Останавливаем игру
        
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
            
            if (levelCompleteText != null)
            {
                string message = "Уровень пройден!";
                if (!string.IsNullOrEmpty(levelName))
                {
                    message += $"\n{levelName}";
                }
                levelCompleteText.text = message;
            }
        }
        
        // Скрываем другие UI элементы
        if (recordingPanel != null) recordingPanel.SetActive(false);
        if (pauseMenu != null && pauseMenu.activeSelf) pauseMenu.SetActive(false);
        
        // Показываем курсор
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        Debug.Log("Панель завершения уровня показана");
    }
    #endregion

    #region Поражение
    // Метод для отображения панели поражения
    public void ShowGameOver(string reason = "Вас обнаружили!")
    {
        if (isGameOver) return;
        
        isGameOver = true;
        Time.timeScale = 0f; // Останавливаем игру
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (gameOverText != null)
            {
                gameOverText.text = reason;
            }
        }
        
        // Скрываем другие UI элементы
        if (recordingPanel != null) recordingPanel.SetActive(false);
        if (pauseMenu != null && pauseMenu.activeSelf) pauseMenu.SetActive(false);
        
        // Показываем курсор
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        Debug.Log("Панель поражения показана: " + reason);
    }
    #endregion

    #region Управление сценами
    public void RestartLevel()
    {
        Time.timeScale = 1f; // Восстанавливаем время
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            // Если следующего уровня нет, возвращаем в меню
            LoadMainMenu();
        }
    }
    #endregion

    #region Вспомогательные методы
    // Метод для сброса всех состояний UI
    public void ResetUI()
    {
        isPaused = false;
        isGameOver = false;
        isLevelComplete = false;
        
        Time.timeScale = 1f;
        
        // Скрываем все панели кроме записи
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        
        // Восстанавливаем панель записи
        if (recordingPanel != null)
        {
            recordingPanel.SetActive(true);
            ResetToReady();
        }
        
        // Скрываем курсор
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    #endregion
}