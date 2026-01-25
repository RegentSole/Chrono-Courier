using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Settings")]
    [SerializeField] private GameSettings gameSettings;
    
    public enum GameState
    {
        MainMenu,
        Playing,
        Recording,
        Paused,
        LevelComplete,
        GameOver
    }
    
    private GameState currentState = GameState.MainMenu;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Инициализация систем
            InitializeSystems();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeSystems()
    {
        // Загружаем ScriptableObjects если они не назначены
        if (gameSettings == null)
        {
            // Правильный способ загрузки ScriptableObject
            string path = "Data/GameSettings";
            gameSettings = Resources.Load<GameSettings>(path);
            
            if (gameSettings == null)
            {
                Debug.LogError($"GameSettings not found at path: {path}. Creating default...");
                // Создаем временные настройки, если файл не найден
                gameSettings = ScriptableObject.CreateInstance<GameSettings>();
                gameSettings.totalLevels = 4;
                gameSettings.restartDelay = 2f;
                gameSettings.uiFadeTime = 0.3f;
            }
        }
        
        // Подписываемся на события загрузки сцен
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        Debug.Log("GameManager initialized");
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");
        
        // В зависимости от сцены устанавливаем состояние
        if (scene.name == "MainMenu")
        {
            SetState(GameState.MainMenu);
        }
        else if (scene.name.Contains("Level"))
        {
            SetState(GameState.Playing);
            StartLevel();
        }
    }
    
    public void SetState(GameState newState)
    {
        GameState previousState = currentState;
        currentState = newState;
        
        // Безопасный вызов события
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnGameStateChanged?.Invoke(previousState, newState);
        }
        
        Debug.Log($"Game state changed: {previousState} -> {newState}");
    }
    
    private void StartLevel()
    {
        // Инициализация уровня
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnLevelStarted?.Invoke();
        }
        
        Debug.Log("Level started");
    }
    
    public void CompleteLevel()
    {
        SetState(GameState.LevelComplete);
        
        // Сохраняем прогресс
        PlayerPrefs.SetInt("LastCompletedLevel", SceneManager.GetActiveScene().buildIndex);
        PlayerPrefs.Save();
        
        Debug.Log("Level completed!");
    }
    
    private void OnApplicationQuit()
    {
        // Отписываемся от событий
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}