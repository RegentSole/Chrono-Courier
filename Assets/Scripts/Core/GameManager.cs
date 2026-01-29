using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game Settings")]
    public int totalLevels = 4;
    public float restartDelay = 2f;
    public float uiFadeTime = 0.3f;
    
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
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSystems();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeSystems()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log("GameManager initialized");
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");
        
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
        
        // Вызываем через метод EventManager, а не событие напрямую
        if (EventManager.Instance != null)
        {
            EventManager.Instance.ChangeGameState(previousState, newState);
        }
        
        Debug.Log($"Game state changed: {previousState} -> {newState}");
    }
    
    private void StartLevel()
    {
        // Вызываем через метод EventManager
        if (EventManager.Instance != null)
        {
            EventManager.Instance.StartLevel();
        }
        
        Debug.Log("Level started");
    }
    
    public void CompleteLevel()
    {
        SetState(GameState.LevelComplete);
        PlayerPrefs.SetInt("LastCompletedLevel", SceneManager.GetActiveScene().buildIndex);
        PlayerPrefs.Save();
        
        // Вызываем через метод EventManager
        if (EventManager.Instance != null)
        {
            EventManager.Instance.CompleteLevel();
        }
        
        Debug.Log("Level completed!");
    }
    
    private void OnApplicationQuit()
    {
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