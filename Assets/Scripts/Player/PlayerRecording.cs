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
    
    private SpriteRenderer spriteRenderer;
    
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        Debug.Log("PlayerRecording инициализирован");
    }
    
    private void Update()
    {
        // Простое управление - нажал R для старта, отпустил для стопа
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartRecording();
        }
        
        if (Input.GetKeyUp(KeyCode.R))
        {
            StopRecording();
        }
    }
    
    private void FixedUpdate()
    {
        if (!isRecording) return;
        
        RecordCurrentFrame();
        
        // Автоматическая остановка при достижении лимита
        if (Time.time - recordStartTime >= maxRecordDuration)
        {
            StopRecording();
        }
    }
    
    private void StartRecording()
    {
        if (isRecording) return;
        
        recording.Clear();
        recordStartTime = Time.time;
        isRecording = true;
        
        // Меняем цвет игрока
        if (spriteRenderer != null)
        {
            spriteRenderer.color = recordingColor;
        }
        
        // Прямой вызов UIManager
        if (UIManager.Instance != null)
        {
            UIManager.Instance.StartRecording();
        }
        else
        {
            Debug.LogWarning("UIManager.Instance is null!");
        }
        
        Debug.Log("Игрок: Запись начата");
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
            false // IsInteracting пока не используем
        );
        
        recording.Add(frame);
        
        // Обновляем таймер в UI
        float timeRemaining = maxRecordDuration - (Time.time - recordStartTime);
        if (UIManager.Instance != null)
        {
            // Создаем метод UpdateRecordTimer в UIManager
            UIManager.Instance.UpdateRecordTimer(timeRemaining);
        }
    }
    
    private void StopRecording()
    {
        if (!isRecording) return;
        
        isRecording = false;
        
        // Возвращаем нормальный цвет
        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }
        
        // Прямой вызов UIManager
        if (UIManager.Instance != null)
        {
            UIManager.Instance.StopRecording();
        }
        
        Debug.Log($"Игрок: Запись остановлена. Записано кадров: {recording.Count}");
        
        // Создаем призрака (прямой вызов)
        if (recording.Count > 0)
        {
            CreateGhost();
        }
        
        // Очищаем запись
        recording.Clear();
    }
    
    private void CreateGhost()
    {
        // Ищем GhostSpawner в сцене
        GhostSpawner ghostSpawner = FindObjectOfType<GhostSpawner>();
        if (ghostSpawner != null)
        {
            // Преобразуем список в массив
            RecordFrame[] recordingArray = recording.ToArray();
            ghostSpawner.SpawnGhost(recordingArray, transform.position);
        }
        else
        {
            Debug.LogWarning("GhostSpawner не найден в сцене!");
        }
    }
}