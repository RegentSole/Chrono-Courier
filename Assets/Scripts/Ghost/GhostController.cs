using UnityEngine;

public class GhostController : MonoBehaviour
{
    [Header("Ghost Settings")]
    [SerializeField] private Color ghostColor = new Color(1f, 1f, 1f, 0.6f);
    [SerializeField] private int maxGhosts = 1;
    [SerializeField] private float replaySpeed = 1f;
    
    private RecordFrame[] recording;
    private int currentFrame = 0;
    private float replayStartTime;
    private bool isReplaying = false;
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        
        // Настраиваем визуал призрака
        if (spriteRenderer != null)
        {
            spriteRenderer.color = ghostColor;
            spriteRenderer.sortingOrder = -1; // Призрак позади игрока
        }
    }
    
    public void StartReplay(RecordFrame[] recordFrames)
    {
        recording = recordFrames;
        currentFrame = 0;
        replayStartTime = Time.time;
        isReplaying = true;
        
        // Активируем объект
        gameObject.SetActive(true);
        
        Debug.Log($"Ghost replay started with {recording.Length} frames");
    }
    
    private void FixedUpdate()
    {
        if (!isReplaying || recording == null || currentFrame >= recording.Length)
        {
            FinishReplay();
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
        
        // Анимации
        if (animator != null)
        {
            animator.SetBool("IsRunning", Mathf.Abs(frame.velocity.x) > 0.1f);
            animator.SetBool("IsGrounded", frame.isGrounded);
            animator.SetBool("IsJumping", frame.isJumping);
        }
        
        // Поворот спрайта
        if (spriteRenderer != null)
        {
            if (frame.velocity.x > 0.1f)
                spriteRenderer.flipX = false;
            else if (frame.velocity.x < -0.1f)
                spriteRenderer.flipX = true;
        }
    }
    
    private void FinishReplay()
    {
        isReplaying = false;
        gameObject.SetActive(false);
        Debug.Log("Ghost replay finished");
    }
    
    public void ResetGhost()
    {
        recording = null;
        currentFrame = 0;
        isReplaying = false;
        gameObject.SetActive(false);
    }
}