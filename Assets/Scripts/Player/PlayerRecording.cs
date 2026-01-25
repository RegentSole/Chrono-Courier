using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RecordFrame
{
    public float timestamp;
    public Vector2 position;
    public Vector2 velocity;
    public bool isGrounded;
    public bool isJumping;
    public bool isInteracting;
    
    public RecordFrame(float time, Vector2 pos, Vector2 vel, bool grounded, bool jumping, bool interacting)
    {
        timestamp = time;
        position = pos;
        velocity = vel;
        isGrounded = grounded;
        isJumping = jumping;
        isInteracting = interacting;
    }
}

public class PlayerRecording : MonoBehaviour
{
    [Header("Recording Settings")]
    [SerializeField] private float maxRecordDuration = 5f;
    [SerializeField] private Color recordingColor = new Color(1f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color normalColor = Color.white;
    
    private List<RecordFrame> recording = new List<RecordFrame>();
    private float recordStartTime;
    private bool isRecording = false;
    
    public event System.Action<RecordFrame[]> OnRecordingComplete;
    
    private void Start()
    {
        // Подписываемся на события
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnRecordingStarted += StartRecording;
            EventManager.Instance.OnRecordingStopped += StopRecording;
        }
    }
    
    public void StartRecording()
    {
        if (isRecording) return;
        
        recording.Clear();
        recordStartTime = Time.time;
        isRecording = true;
        
        // Меняем цвет игрока
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = recordingColor;
        }
        
        Debug.Log("Recording started");
    }
    
    private void FixedUpdate()
    {
        if (!isRecording) return;
        
        RecordCurrentFrame();
        
        // Обновляем UI таймера
        float timeRemaining = maxRecordDuration - (Time.time - recordStartTime);
        if (EventManager.Instance != null)
        {
            EventManager.Instance.UpdateRecordTimer(timeRemaining);
        }
        
        // Ограничиваем длительность записи
        if (Time.time - recordStartTime >= maxRecordDuration)
        {
            StopRecording();
        }
    }
    
    private void RecordCurrentFrame()
    {
        var playerController = GetComponent<PlayerController>();
        if (playerController == null) return;
        
        var frame = new RecordFrame(
            Time.time - recordStartTime,
            transform.position,
            playerController.Velocity,
            playerController.IsGrounded,
            playerController.IsJumping,
            playerController.IsInteracting
        );
        
        recording.Add(frame);
    }
    
    public void StopRecording()
    {
        if (!isRecording) return;
        
        isRecording = false;
        Debug.Log($"Recording stopped. Frames: {recording.Count}");
        
        // Возвращаем нормальный цвет
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }
        
        // Отправляем запись системе воспроизведения
        OnRecordingComplete?.Invoke(recording.ToArray());
        
        // Очищаем запись
        recording.Clear();
        
        // Обнуляем таймер в UI
        if (EventManager.Instance != null)
        {
            EventManager.Instance.UpdateRecordTimer(0f);
        }
    }
    
    private void Update()
    {
        // Тестовое управление записи
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartRecording();
        }
        
        if (Input.GetKeyUp(KeyCode.R))
        {
            StopRecording();
        }
    }
    
    private void OnDestroy()
    {
        // Отписываемся от событий
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnRecordingStarted -= StartRecording;
            EventManager.Instance.OnRecordingStopped -= StopRecording;
        }
    }
}