using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }
    
    // Игровые события
    public event System.Action OnRecordingStarted;
    public event System.Action OnRecordingStopped;
    public event System.Action OnGhostSpawned;
    public event System.Action OnPlayerDetected;
    public event System.Action OnLevelStarted;
    public event System.Action<GameManager.GameState, GameManager.GameState> OnGameStateChanged;
    public event System.Action OnLevelCompleted;
    public event System.Action OnPlayerCaught;
    
    // UI события
    public event System.Action<float> OnRecordTimerUpdated;
    
    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Методы для вызова событий
    public void StartRecording()
    {
        Debug.Log("EventManager: Запись начата");
        OnRecordingStarted?.Invoke();
    }
    
    public void StopRecording()
    {
        Debug.Log("EventManager: Запись остановлена");
        OnRecordingStopped?.Invoke();
    }
    
    public void SpawnGhost()
    {
        Debug.Log("EventManager: Призрак создан");
        OnGhostSpawned?.Invoke();
    }
    
    public void PlayerDetected()
    {
        Debug.Log("EventManager: Игрок обнаружен");
        OnPlayerDetected?.Invoke();
    }
    
    public void StartLevel()
    {
        Debug.Log("EventManager: Уровень начат");
        OnLevelStarted?.Invoke();
    }
    
    public void ChangeGameState(GameManager.GameState previousState, GameManager.GameState newState)
    {
        Debug.Log($"EventManager: Состояние игры изменено {previousState} -> {newState}");
        OnGameStateChanged?.Invoke(previousState, newState);
    }
    
    public void CompleteLevel()
    {
        Debug.Log("EventManager: Уровень пройден");
        OnLevelCompleted?.Invoke();
    }
    
    public void PlayerCaught()
    {
        Debug.Log("EventManager: Игрок пойман");
        OnPlayerCaught?.Invoke();
    }
    
    public void UpdateRecordTimer(float timeRemaining)
    {
        OnRecordTimerUpdated?.Invoke(timeRemaining);
    }
}