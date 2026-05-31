using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    private float footstepTimer;
    private float footstepInterval = 0.4f;
    private bool wasGrounded; // для отслеживания приземления
    private float lastLandTime = -1f;
    private float landCooldown = 0.2f;
    
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private int maxJumps = 2;
    
    [Header("Flip Jump (Second Jump)")]
    [SerializeField] private float flipJumpScale = 0.6f;
    [SerializeField] private float flipJumpDuration = 0.3f;
    [SerializeField] private float flipJumpRotation = 360f;
    [SerializeField] private AnimationCurve flipJumpScaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 0.6f);

    [Header("Wall Jump")]
    [SerializeField] private float wallJumpForce = 12f;
    [SerializeField] private float wallCheckDistance = 0.45f;   // дистанция луча до стены
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallJumpCooldown = 0.2f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    
    private bool isGrounded;
    private float moveInput;
    private bool jumpPressed;
    private int remainingJumps;
    private bool isFlipJumping = false;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    
    private bool isTouchingWall = false;
    private bool isWallJumping = false;
    private float wallJumpTimer = 0f;
    private Vector2 wallNormal;   // направление от стены (влево/вправо)
    
    // Свойства для других скриптов
    public Vector2 Velocity => rb != null ? rb.linearVelocity : Vector2.zero;
    public bool IsGrounded => isGrounded;
    public bool IsJumping { get; private set; }
    public bool IsInteracting { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        
        originalScale = transform.localScale;
        originalRotation = transform.rotation;
        remainingJumps = maxJumps;
    }

    private void Update()
    {
        moveInput = Input.GetAxis("Horizontal");
        
        if (Input.GetButtonDown("Jump") && !isWallJumping)
        {
            if (isGrounded)
            {
                PerformJump();
                remainingJumps = maxJumps - 1;
            }
            else if (isTouchingWall && !isGrounded)
            {
                PerformWallJump();
            }
            else if (remainingJumps > 0)
            {
                PerformFlipJump();
                remainingJumps--;
            }
        }
        
        if (isGrounded && !isFlipJumping && !isWallJumping)
            remainingJumps = maxJumps;
        
        if (Input.GetKeyDown(KeyCode.E))
            IsInteracting = true;
        else
            IsInteracting = false;
        
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(moveInput));
            animator.SetBool("IsGrounded", isGrounded);
        }

        if (isGrounded && Mathf.Abs(moveInput) > 0.1f && !isWallJumping && !isFlipJumping)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                AudioManager.Instance?.PlayFootstep();
                footstepTimer = footstepInterval;
            }
        }
        else
            {
                footstepTimer = 0f;
            }
    }

    private void FixedUpdate()
    {
        CheckGround();
        CheckWallTouch();
        
        // горизонтальное движение
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        
        // предотвращаем залипание: если игрок врезается в стену и пытается идти в неё – срезаем скорость
        if (isTouchingWall && Mathf.Sign(moveInput) == wallNormal.x && Mathf.Abs(moveInput) > 0.1f)
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed * 0.3f, rb.linearVelocity.y);
        }
        
        // поворот спрайта
        if (spriteRenderer != null && moveInput != 0)
            spriteRenderer.flipX = moveInput < 0;
        
        if (isWallJumping)
        {
            wallJumpTimer -= Time.fixedDeltaTime;
            if (wallJumpTimer <= 0f) isWallJumping = false;
        }
        
        if (jumpPressed)
            IsJumping = true;
        else if (isGrounded)
            IsJumping = false;

        if (!wasGrounded && isGrounded && Time.time > lastLandTime + landCooldown)
        {
            AudioManager.Instance?.PlayLand();
            lastLandTime = Time.time;
        }
        wasGrounded = isGrounded;
    }   

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Если игрок в режиме записи – прерываем запись, но не убиваем
            PlayerRecording recording = GetComponent<PlayerRecording>();
            if (recording != null && recording.IsInvincible)
            {
                recording.InterruptRecording();
            }
            else
            {
                // Обычная смерть (вызываем Game Over)
                if (EventManager.Instance != null)
                    EventManager.Instance.PlayerCaught();
            }
        }
    }

    #region Ground Check
    private void CheckGround()
    {
        if (groundCheckPoint == null)
        {
            Debug.LogError("Ground Check Point не назначен!");
            isGrounded = false;
            return;
        }
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
    }
    #endregion

    #region Wall Check (raycast вместо OverlapCircle)
    private void CheckWallTouch()
    {
        // направление взгляда (куда смотрит спрайт)
        Vector2 lookDir = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        // луч от центра персонажа в сторону взгляда
        RaycastHit2D hit = Physics2D.Raycast(transform.position, lookDir, wallCheckDistance, wallLayer);
        
        if (hit.collider != null && !isGrounded)
        {
            // стена обнаружена, причём игрок движется к ней?
            if (Mathf.Sign(moveInput) == lookDir.x && Mathf.Abs(moveInput) > 0.1f)
            {
                isTouchingWall = true;
                wallNormal = -lookDir;   // отталкивание в противоположную сторону
                return;
            }
        }
        
        // дополнительная проверка: если игрок зажат между двух стен – даём возможность оттолкнуться
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, wallCheckDistance, wallLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, wallCheckDistance, wallLayer);
        if (hitLeft.collider != null && hitRight.collider != null)
        {
            isTouchingWall = true;
            wallNormal = Vector2.zero; // особый случай – общая стена, прыжок вверх
        }
        else
        {
            isTouchingWall = false;
        }
    }
    #endregion

    #region Jump Logic
    private void PerformJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        jumpPressed = true;
        if (isFlipJumping) StopFlipJump();
        AudioManager.Instance?.PlayJump();
    }

    private void PerformFlipJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        jumpPressed = true;
        StartFlipJump();
        if (animator != null) animator.SetTrigger("FlipJump");
        AudioManager.Instance?.PlayDoubleJump();
    }

    private void StartFlipJump()
    {
        if (isFlipJumping) return;
        isFlipJumping = true;
        transform.rotation = originalRotation;
        StartCoroutine(FlipJumpCoroutine());
    }

    private IEnumerator FlipJumpCoroutine()
    {
        float elapsed = 0f;
        float startRot = transform.eulerAngles.z;
        float targetRot = startRot + flipJumpRotation;
        
        while (elapsed < flipJumpDuration)
        {
            float t = elapsed / flipJumpDuration;
            float scale = Mathf.Lerp(1f, flipJumpScale, flipJumpScaleCurve.Evaluate(t));
            transform.localScale = new Vector3(originalScale.x * scale, originalScale.y * scale, originalScale.z);
            
            if (flipJumpRotation != 0)
            {
                float angle = Mathf.Lerp(startRot, targetRot, t);
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = originalScale;
        transform.rotation = originalRotation;
        isFlipJumping = false;
    }

    private void StopFlipJump()
    {
        StopAllCoroutines();
        transform.localScale = originalScale;
        transform.rotation = originalRotation;
        isFlipJumping = false;
    }

    private void PerformWallJump()
    {
        isWallJumping = true;
        wallJumpTimer = wallJumpCooldown;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        
        Vector2 jumpDir = wallNormal + Vector2.up;
        if (wallNormal == Vector2.zero) jumpDir = Vector2.up; // прыжок строго вверх
        jumpDir.Normalize();
        rb.AddForce(jumpDir * wallJumpForce, ForceMode2D.Impulse);
        
        remainingJumps = maxJumps;
        if (isFlipJumping) StopFlipJump();
    }
    #endregion

    #region Editor
    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
        Gizmos.color = Color.cyan;
        Vector2 origin = transform.position;
        Vector2 left = Vector2.left * wallCheckDistance;
        Vector2 right = Vector2.right * wallCheckDistance;
        Gizmos.DrawLine(origin, origin + left);
        Gizmos.DrawLine(origin, origin + right);
    }
    #endregion
}