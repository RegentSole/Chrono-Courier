using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    [Header("Recording UI")]
    [SerializeField] private GameObject recordingPanel;
    [SerializeField] private Image recordTimerFill;
    [SerializeField] private TextMeshProUGUI recordTimerText;
    [SerializeField] private Color readyColor = Color.green;
    [SerializeField] private Color recordingColor = Color.red;
    [SerializeField] private Color cooldownColor = Color.yellow;
    
    [Header("Tutorial UI")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private float tutorialDisplayTime = 5f;
    
    [Header("Game UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private TextMeshProUGUI levelText;
    
    private bool isRecording = false;
    private float recordCooldown = 2f;
    private float currentCooldown = 0f;
    
    private void Awake()
    {
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
        // Подписываемся на события
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnRecordingStarted += OnRecordingStarted;
            EventManager.Instance.OnRecordingStopped += OnRecordingStopped;
            EventManager.Instance.OnRecordTimerUpdated += UpdateRecordTimer;
            EventManager.Instance.OnPlayerDetected += ShowAlert;
        }
        
        // Скрываем UI элементы
        if (recordingPanel != null) recordingPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        
        // Показываем туториал
        ShowTutorial("Двигайтесь: A/D или ←/→\nПрыжок: Пробел\nЗапись: Удерживайте R");
    }
    
    private void Update()
    {
        // Обновление кулдауна
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
            UpdateCooldownUI();
        }
    }
    
    private void OnRecordingStarted()
    {
        isRecording = true;
        if (recordingPanel != null) recordingPanel.SetActive(true);
        
        if (recordTimerFill != null)
        {
            recordTimerFill.color = recordingColor;
        }
    }
    
    private void OnRecordingStopped()
    {
        isRecording = false;
        currentCooldown = recordCooldown;
        
        if (recordTimerFill != null)
        {
            recordTimerFill.color = cooldownColor;
        }
    }
    
    private void UpdateRecordTimer(float timeRemaining)
    {
        if (recordTimerFill != null)
        {
            float fillAmount = timeRemaining / 5f; // 5 секунд максимальная запись
            recordTimerFill.fillAmount = fillAmount;
        }
        
        if (recordTimerText != null)
        {
            recordTimerText.text = Mathf.CeilToInt(timeRemaining).ToString();
        }
    }
    
    private void UpdateCooldownUI()
    {
        if (currentCooldown <= 0)
        {
            if (recordTimerFill != null)
            {
                recordTimerFill.color = readyColor;
                recordTimerFill.fillAmount = 1f;
            }
            
            if (recordTimerText != null)
            {
                recordTimerText.text = "Готово";
            }
            
            if (recordingPanel != null && !isRecording)
            {
                recordingPanel.SetActive(false);
            }
        }
        else
        {
            if (recordTimerFill != null)
            {
                recordTimerFill.fillAmount = currentCooldown / recordCooldown;
            }
            
            if (recordTimerText != null)
            {
                recordTimerText.text = Mathf.CeilToInt(currentCooldown).ToString();
            }
        }
    }
    
    public void ShowTutorial(string message)
    {
        if (tutorialPanel == null || tutorialText == null) return;
        
        tutorialText.text = message;
        tutorialPanel.SetActive(true);
        
        // Автоматическое скрытие через время
        Invoke("HideTutorial", tutorialDisplayTime);
    }
    
    public void HideTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
    }
    
    private void ShowAlert()
    {
        // Можно добавить мигание UI или звуковое оповещение
        Debug.Log("ALERT: Player detected!");
    }
    
    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }
    
    public void ShowLevelComplete()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
        }
    }
    
    public void RestartLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }
    
    public void LoadNextLevel()
    {
        int nextSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0); // Возврат в меню
        }
    }
    
    public void QuitToMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
    
    private void OnDestroy()
    {
        // Отписываемся от событий
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnRecordingStarted -= OnRecordingStarted;
            EventManager.Instance.OnRecordingStopped -= OnRecordingStopped;
            EventManager.Instance.OnRecordTimerUpdated -= UpdateRecordTimer;
            EventManager.Instance.OnPlayerDetected -= ShowAlert;
        }
    }
}