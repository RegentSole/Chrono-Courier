using UnityEngine;

public class GhostController : MonoBehaviour
{
    [Header("Ghost Settings")]
    [SerializeField] private Color ghostColor = new Color(1f, 1f, 1f, 0.6f);
    [SerializeField] private float replaySpeed = 1f;
    
    private RecordFrame[] recording;
    private int currentFrame = 0;
    private float replayStartTime;
    private bool isReplaying = false;
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator; // Добавляем аниматор
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>(); // Получаем компонент аниматора
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = ghostColor;
            spriteRenderer.sortingOrder = -1;
        }
    }
    
    public void StartReplay(RecordFrame[] recordFrames)
    {
        recording = recordFrames;
        currentFrame = 0;
        replayStartTime = Time.time;
        isReplaying = true;
        gameObject.SetActive(true);
        
        Debug.Log($"Призрак: начинаю воспроизведение с {recording.Length} кадрами");
        
        // Автоуничтожение через 5 секунд (можно заменить на настраиваемое время)
        Destroy(gameObject, 5f);
    }
    
    private void FixedUpdate()
    {
        if (!isReplaying || recording == null || currentFrame >= recording.Length)
        {
            FinishReplay();
            return;
        }
        
        float currentTime = (Time.time - replayStartTime) * replaySpeed;
        
        // Ищем подходящий кадр
        while (currentFrame < recording.Length - 1 && 
               recording[currentFrame + 1].timestamp <= currentTime)
        {
            currentFrame++;
        }
        
        ApplyFrame(recording[currentFrame]);
        
        if (currentFrame >= recording.Length - 1 && 
            currentTime >= recording[recording.Length - 1].timestamp)
        {
            FinishReplay();
        }
    }
    
    private void ApplyFrame(RecordFrame frame)
    {
        if (rb == null) return;
        
        // Позиция (используем MovePosition для плавности)
        rb.MovePosition(frame.position);
        
        // Поворот спрайта
        if (spriteRenderer != null && frame.velocity.x != 0)
        {
            spriteRenderer.flipX = frame.velocity.x < 0;
        }
        
        // Передаём параметры анимации
        if (animator != null)
        {
            // Скорость бега (абсолютное значение)
            animator.SetFloat("Speed", Mathf.Abs(frame.velocity.x));
            // На земле или в воздухе
            animator.SetBool("IsGrounded", frame.isGrounded);
            // Прыжок (можно использовать как триггер, но мы уже имеем isJumping)
            animator.SetBool("IsJumping", frame.isJumping);
        }
    }
    
    private void FinishReplay()
    {
        isReplaying = false;
        gameObject.SetActive(false);
    }
    
    public void ResetGhost()
    {
        recording = null;
        currentFrame = 0;
        isReplaying = false;
        gameObject.SetActive(false);
    }
}