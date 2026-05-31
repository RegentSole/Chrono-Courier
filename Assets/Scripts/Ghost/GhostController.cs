using UnityEngine;
using System.Collections;

public class GhostController : MonoBehaviour
{
    [Header("Ghost Settings")]
    [SerializeField] private Color ghostColor = new Color(1f, 1f, 1f, 0.6f);
    [SerializeField] private float replaySpeed = 1f;
    
    [Header("Visual Effects")]
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private RecordFrame[] recording;
    private int currentFrame = 0;
    private float replayStartTime;
    private bool isReplaying = false;
    private bool isFadingOut = false;
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = ghostColor;
            spriteRenderer.sortingOrder = -1;
        }
    }
    
    public void StartReplay(RecordFrame[] recordFrames, Vector2 startPosition)
    {
        recording = recordFrames;
        currentFrame = 0;
        replayStartTime = Time.time;
        isReplaying = true;
        transform.position = startPosition;
        gameObject.SetActive(true);
        
        // Запускаем появление
        StartCoroutine(FadeIn());
        
        // Автоматическое исчезновение после окончания записи + небольшой запас
        float replayDuration = recording.Length > 0 ? recording[recording.Length - 1].timestamp : 0f;
        float totalTime = replayDuration + fadeOutDuration;
        StartCoroutine(ScheduleFadeOut(totalTime));
    }
    
    private IEnumerator FadeIn()
    {
        if (spriteRenderer == null) yield break;
        
        Color startColor = ghostColor;
        startColor.a = 0f;
        spriteRenderer.color = startColor;
        
        float time = 0f;
        while (time < fadeInDuration)
        {
            float t = fadeCurve.Evaluate(time / fadeInDuration);
            Color c = spriteRenderer.color;
            c.a = Mathf.Lerp(0f, ghostColor.a, t);
            spriteRenderer.color = c;
            time += Time.deltaTime;
            yield return null;
        }
        
        Color finalColor = spriteRenderer.color;
        finalColor.a = ghostColor.a;
        spriteRenderer.color = finalColor;
    }
    
    private IEnumerator ScheduleFadeOut(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!isFadingOut)
        {
            StartCoroutine(FadeOutAndDestroy());
        }
    }
    
    private IEnumerator FadeOutAndDestroy()
    {
        if (isFadingOut) yield break;
        isFadingOut = true;
        
        if (spriteRenderer == null)
        {
            Destroy(gameObject);
            yield break;
        }
        
        float startAlpha = spriteRenderer.color.a;
        float time = 0f;
        while (time < fadeOutDuration)
        {
            float t = fadeCurve.Evaluate(time / fadeOutDuration);
            Color c = spriteRenderer.color;
            c.a = Mathf.Lerp(startAlpha, 0f, t);
            spriteRenderer.color = c;
            time += Time.deltaTime;
            yield return null;
        }
        
        Destroy(gameObject);
    }
    
    private void FixedUpdate()
    {
        if (!isReplaying || recording == null || currentFrame >= recording.Length)
        {
            if (!isFadingOut) StartCoroutine(FadeOutAndDestroy());
            return;
        }
        
        float currentTime = (Time.time - replayStartTime) * replaySpeed;
        
        while (currentFrame < recording.Length - 1 && 
               recording[currentFrame + 1].timestamp <= currentTime)
        {
            currentFrame++;
        }
        
        ApplyFrame(recording[currentFrame]);
        
        if (currentFrame >= recording.Length - 1 && 
            currentTime >= recording[recording.Length - 1].timestamp)
        {
            StartCoroutine(FadeOutAndDestroy());
        }
    }
    
    private void ApplyFrame(RecordFrame frame)
    {
        if (rb == null) return;
        
        // Позиция
        rb.MovePosition(frame.position);
        
        // Масштаб и поворот (для кувырка)
        transform.localScale = frame.localScale;
        transform.localRotation = frame.localRotation;
        
        if (spriteRenderer != null && frame.velocity.x != 0)
        {
            spriteRenderer.flipX = frame.velocity.x < 0;
        }
        
        // Анимации (если нужно)
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(frame.velocity.x));
            animator.SetBool("IsGrounded", frame.isGrounded);
            animator.SetBool("IsJumping", frame.isJumping);
        }
    }
}