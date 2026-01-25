using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer = 1; // Default layer
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded;
    private float moveInput;
    private bool jumpPressed;
    
    // Свойства для доступа из других скриптов
    public Vector2 Velocity => rb != null ? rb.linearVelocity : Vector2.zero;
    public bool IsGrounded => isGrounded;
    public bool IsJumping { get; private set; }
    public bool IsInteracting { get; private set; }
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    private void Update()
    {
        // Получаем ввод
        moveInput = Input.GetAxis("Horizontal");
        
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpPressed = true;
        }
        
        // Тест для взаимодействия (кнопка E)
        if (Input.GetKeyDown(KeyCode.E))
        {
            IsInteracting = true;
        }
        else
        {
            IsInteracting = false;
        }
    }
    
    private void FixedUpdate()
    {
        // Проверка земли
        CheckGround();
        
        // Движение по горизонтали
        if (rb != null)
        {
            Vector2 velocity = rb.linearVelocity;
            velocity.x = moveInput * moveSpeed;
            rb.linearVelocity = velocity;
            
            // Прыжок
            if (jumpPressed)
            {
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                IsJumping = true;
                jumpPressed = false;
            }
            else if (isGrounded)
            {
                IsJumping = false;
            }
        }
        
        // Поворот спрайта
        if (spriteRenderer != null)
        {
            if (moveInput > 0.1f)
                spriteRenderer.flipX = false;
            else if (moveInput < -0.1f)
                spriteRenderer.flipX = true;
        }
    }
    
    private void CheckGround()
    {
        Vector2 position = transform.position;
        position.y -= 0.5f; // Немного ниже центра
        
        isGrounded = Physics2D.OverlapCircle(position, groundCheckRadius, groundLayer);
    }
    
    // Визуализация в редакторе
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector2 position = transform.position;
        position.y -= 0.5f;
        Gizmos.DrawWireSphere(position, groundCheckRadius);
    }
}