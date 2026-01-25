using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float waitTime = 1f;
    
    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float chaseRange = 8f;
    [SerializeField] private float attackRange = 1.5f;
    
    [Header("Detection")]
    [SerializeField] private float detectionAngle = 90f;
    [SerializeField] private float detectionDistance = 6f;
    [SerializeField] private LayerMask detectionLayer;
    [SerializeField] private LayerMask obstacleLayer;
    
    private Transform player;
    private int currentPatrolIndex = 0;
    private bool isWaiting = false;
    private bool isChasing = false;
    
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    
    [Header("Visual Feedback")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color alertColor = Color.red;
    [SerializeField] private GameObject exclamationMark;
    
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        
        if (patrolPoints.Length > 0)
        {
            transform.position = patrolPoints[0].position;
        }
        
        if (exclamationMark != null)
        {
            exclamationMark.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (isChasing)
        {
            ChasePlayer(distanceToPlayer);
        }
        else
        {
            Patrol();
            CheckForPlayer(distanceToPlayer);
        }
        
        UpdateAnimation();
    }
    
    private void Patrol()
    {
        if (patrolPoints.Length == 0 || isWaiting) return;
        
        Transform targetPoint = patrolPoints[currentPatrolIndex];
        Vector2 direction = (targetPoint.position - transform.position).normalized;
        
        // Движение к точке
        transform.position = Vector2.MoveTowards(
            transform.position, 
            targetPoint.position, 
            patrolSpeed * Time.deltaTime
        );
        
        // Поворот спрайта
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
        
        // Проверка достижения точки
        if (Vector2.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            StartCoroutine(WaitAtPoint());
        }
    }
    
    private IEnumerator WaitAtPoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);
        
        // Переход к следующей точке
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        isWaiting = false;
    }
    
    private void CheckForPlayer(float distanceToPlayer)
    {
        if (distanceToPlayer <= detectionDistance && CanSeePlayer())
        {
            StartChasing();
        }
    }
    
    private bool CanSeePlayer()
    {
        if (player == null) return false;
        
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        float angleToPlayer = Vector2.Angle(transform.right * (spriteRenderer.flipX ? -1 : 1), directionToPlayer);
        
        // Проверка угла обзора
        if (angleToPlayer > detectionAngle / 2) return false;
        
        // Raycast для проверки препятствий
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position, 
            directionToPlayer, 
            detectionDistance, 
            obstacleLayer
        );
        
        // Визуализация луча в редакторе
        Debug.DrawRay(transform.position, directionToPlayer * detectionDistance, Color.yellow);
        
        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            return true;
        }
        
        return false;
    }
    
    private void StartChasing()
    {
        isChasing = true;
        isWaiting = false;
        
        // Визуальная обратная связь
        if (spriteRenderer != null)
        {
            spriteRenderer.color = alertColor;
        }
        
        if (exclamationMark != null)
        {
            exclamationMark.SetActive(true);
            StartCoroutine(HideExclamationMark());
        }
        
        // Отправляем событие обнаружения
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnPlayerDetected?.Invoke();
        }
        
        Debug.Log("Player detected! Chasing started.");
    }
    
    private void ChasePlayer(float distanceToPlayer)
    {
        if (player == null) return;
        
        Vector2 direction = (player.position - transform.position).normalized;
        
        // Движение к игроку
        transform.position = Vector2.MoveTowards(
            transform.position, 
            player.position, 
            chaseSpeed * Time.deltaTime
        );
        
        // Поворот спрайта
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
        
        // Если игрок слишком далеко, прекращаем преследование
        if (distanceToPlayer > chaseRange)
        {
            StopChasing();
        }
    }
    
    private void StopChasing()
    {
        isChasing = false;
        
        // Возвращаем нормальный цвет
        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }
        
        Debug.Log("Player lost. Returning to patrol.");
    }
    
    private void UpdateAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("IsMoving", !isWaiting);
            animator.SetBool("IsChasing", isChasing);
        }
    }
    
    private IEnumerator HideExclamationMark()
    {
        yield return new WaitForSeconds(1f);
        if (exclamationMark != null)
        {
            exclamationMark.SetActive(false);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Если враг касается игрока
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player caught!");
            
            // Здесь можно добавить логику поражения
            // Например, перезапуск уровня
            if (GameManager.Instance != null)
            {
                // GameManager.Instance.GameOver();
            }
        }
    }
    
    // Визуализация в редакторе
    private void OnDrawGizmosSelected()
    {
        // Отображение угла обзора
        Gizmos.color = Color.yellow;
        Vector2 direction = spriteRenderer != null && spriteRenderer.flipX ? -transform.right : transform.right;
        
        Vector2 leftRay = Quaternion.Euler(0, 0, detectionAngle / 2) * direction;
        Vector2 rightRay = Quaternion.Euler(0, 0, -detectionAngle / 2) * direction;
        
        Gizmos.DrawRay(transform.position, leftRay * detectionDistance);
        Gizmos.DrawRay(transform.position, rightRay * detectionDistance);
        
        // Линия к игроку
        if (player != null && isChasing)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }
        
        // Патрульные точки
        Gizmos.color = Color.blue;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] != null)
            {
                Gizmos.DrawSphere(patrolPoints[i].position, 0.2f);
                if (i < patrolPoints.Length - 1 && patrolPoints[i + 1] != null)
                {
                    Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
                }
            }
        }
    }
}