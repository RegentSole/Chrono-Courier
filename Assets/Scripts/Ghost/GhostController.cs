using UnityEngine;

public class GhostController : MonoBehaviour
{
    [Header("Ghost Settings")]
    [SerializeField] private float replaySpeed = 1f;
    [SerializeField] private float destroyAfterSeconds = 5f;
    
    private RecordFrame[] recording;
    private int currentFrame = 0;
    private float replayStartTime;
    private bool isReplaying = false;
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.6f);
        }
    }
    
    public void StartReplay(RecordFrame[] recordFrames)
    {
        recording = recordFrames;
        currentFrame = 0;
        replayStartTime = Time.time;
        isReplaying = true;
        
        gameObject.SetActive(true);
        
        Debug.Log($"Призрак: Начинаю воспроизведение с {recording.Length} кадрами");
        
        // Уничтожаем через указанное время
        if (destroyAfterSeconds > 0)
        {
            Destroy(gameObject, destroyAfterSeconds);
        }
    }
    
    private void FixedUpdate()
    {
        if (!isReplaying || recording == null || currentFrame >= recording.Length)
        {
            return;
        }
        
        float currentTime = (Time.time - replayStartTime) * replaySpeed;
        
        // Находим подходящий кадр для текущего времени
        while (currentFrame < recording.Length - 1 && 
               recording[currentFrame + 1].timestamp <= currentTime)
        {
            currentFrame++;
        }
        
        ApplyFrame(recording[currentFrame]);
        
        // Проверяем окончание записи
        if (currentFrame >= recording.Length - 1 && 
            currentTime >= recording[recording.Length - 1].timestamp)
        {
            FinishReplay();
        }
    }
    
    private void ApplyFrame(RecordFrame frame)
    {
        if (rb == null) return;
        
        // Интерполяция для плавности
        float t = ((Time.time - replayStartTime) * replaySpeed - frame.timestamp) * 50f;
        t = Mathf.Clamp01(t);
        
        Vector2 targetPosition = Vector2.Lerp(transform.position, frame.position, t);
        rb.MovePosition(targetPosition);
        
        // Поворот спрайта
        if (spriteRenderer != null && frame.velocity.x != 0)
        {
            spriteRenderer.flipX = frame.velocity.x < 0;
        }
    }
    
    private void FinishReplay()
    {
        isReplaying = false;
        Debug.Log("Призрак: Воспроизведение завершено");
    }
}