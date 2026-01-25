using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }
    
    // Игровые события
    public System.Action OnRecordingStarted;
    public System.Action OnRecordingStopped;
    public System.Action OnGhostSpawned;
    public System.Action OnPlayerDetected;
    public System.Action OnLevelStarted;
    public System.Action<GameManager.GameState, GameManager.GameState> OnGameStateChanged;
    
    // UI события
    public System.Action<float> OnRecordTimerUpdated;
    
    private void Awake()
    {
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
        OnRecordingStarted?.Invoke();
    }
    
    public void StopRecording()
    {
        OnRecordingStopped?.Invoke();
    }
    
    public void UpdateRecordTimer(float timeRemaining)
    {
        OnRecordTimerUpdated?.Invoke(timeRemaining);
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}