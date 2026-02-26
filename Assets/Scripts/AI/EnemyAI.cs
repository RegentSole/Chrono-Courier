using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float waitTime = 1f;
    
    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float detectionAngle = 90f; // Угол обзора в градусах
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float chaseRange = 8f; // Максимальная дистанция преследования
    [SerializeField] private LayerMask obstacleLayer; // Слой для препятствий
    
    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color alertColor = Color.red;
    [SerializeField] private GameObject exclamationMark;
    
    private enum AIState
    {
        Patrol,
        Chase,
        Return
    }
    
    private AIState currentState = AIState.Patrol;
    private int currentPatrolIndex = 0;
    private bool isWaiting = false;
    private Transform currentTarget = null;
    
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Vector2 lastKnownPosition;
    private bool hasLineOfSight = false;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }
        
        if (exclamationMark != null)
        {
            exclamationMark.SetActive(false);
        }
    }
    
    private void Start()
    {
        if (patrolPoints.Length > 0)
        {
            transform.position = patrolPoints[0].position;
        }
    }
    
    private void Update()
    {
        switch (currentState)
        {
            case AIState.Patrol:
                Patrol();
                CheckForTargets();
                break;
                
            case AIState.Chase:
                Chase();
                CheckIfTargetLost();
                break;
                
            case AIState.Return:
                ReturnToPatrol();
                break;
        }
        
        UpdateVisuals();
    }
    
    #region Патрулирование
    private void Patrol()
    {
        if (patrolPoints.Length == 0 || isWaiting) return;
        
        Transform targetPoint = patrolPoints[currentPatrolIndex];
        
        // Движение к точке патрулирования
        MoveTowards(targetPoint.position, patrolSpeed);
        
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
    
    private void ReturnToPatrol()
    {
        if (patrolPoints.Length == 0) return;
        
        // Возвращаемся к ближайшей точке патрулирования
        Transform nearestPoint = GetNearestPatrolPoint();
        MoveTowards(nearestPoint.position, patrolSpeed);
        
        // Если достигли точки, возвращаемся к патрулированию
        if (Vector2.Distance(transform.position, nearestPoint.position) < 0.1f)
        {
            currentPatrolIndex = System.Array.IndexOf(patrolPoints, nearestPoint);
            currentState = AIState.Patrol;
        }
    }
    
    private Transform GetNearestPatrolPoint()
    {
        Transform nearest = patrolPoints[0];
        float nearestDistance = Vector2.Distance(transform.position, nearest.position);
        
        for (int i = 1; i < patrolPoints.Length; i++)
        {
            float distance = Vector2.Distance(transform.position, patrolPoints[i].position);
            if (distance < nearestDistance)
            {
                nearest = patrolPoints[i];
                nearestDistance = distance;
            }
        }
        
        return nearest;
    }
    #endregion
    
    #region Обнаружение и преследование
    private void CheckForTargets()
    {
        // Ищем игрока
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject ghost = GameObject.FindGameObjectWithTag("Ghost");
        
        // Проверяем игрока в первую очередь
        if (player != null && CanSeeTarget(player.transform))
        {
            StartChasing(player.transform);
            return;
        }
        
        // Затем проверяем призрака
        if (ghost != null && ghost.activeInHierarchy && CanSeeTarget(ghost.transform))
        {
            StartChasing(ghost.transform);
            return;
        }
    }
    
    private bool CanSeeTarget(Transform target)
    {
        if (target == null) return false;
        
        Vector2 directionToTarget = (target.position - transform.position);
        float distanceToTarget = directionToTarget.magnitude;
        
        // Проверка дистанции
        if (distanceToTarget > detectionRange) return false;
        
        // Проверка угла обзора
        float angleToTarget = Vector2.Angle(GetForwardDirection(), directionToTarget);
        if (angleToTarget > detectionAngle / 2f) return false;
        
        // Проверка линии видимости (нет препятствий)
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position, 
            directionToTarget.normalized, 
            distanceToTarget, 
            obstacleLayer
        );
        
        hasLineOfSight = hit.collider == null || hit.collider.transform == target;
        
        // Визуализация луча в редакторе
        Debug.DrawRay(transform.position, directionToTarget.normalized * detectionRange, 
                     hasLineOfSight ? Color.green : Color.red);
        
        return hasLineOfSight;
    }
    
    private void StartChasing(Transform target)
    {
        currentTarget = target;
        lastKnownPosition = target.position;
        currentState = AIState.Chase;
        
        // Визуальная обратная связь
        if (spriteRenderer != null)
        {
            spriteRenderer.color = alertColor;
        }
        
        if (exclamationMark != null)
        {
            exclamationMark.SetActive(true);
            StartCoroutine(HideExclamationMark(1.5f));
        }
        
        // Вызываем событие обнаружения
        if (EventManager.Instance != null)
        {
            EventManager.Instance.PlayerDetected();
        }
        
        Debug.Log($"{gameObject.name}: Начинаю преследовать {target.name}!");
    }
    
    private void Chase()
    {
        if (currentTarget == null)
        {
            currentState = AIState.Return;
            return;
        }
        
        // Проверяем, видим ли цель сейчас
        if (CanSeeTarget(currentTarget))
        {
            lastKnownPosition = currentTarget.position;
            MoveTowards(lastKnownPosition, chaseSpeed);
        }
        else
        {
            // Двигаемся к последнему известному положению
            MoveTowards(lastKnownPosition, chaseSpeed);
        }
    }
    
    private void CheckIfTargetLost()
    {
        if (currentTarget == null)
        {
            currentState = AIState.Return;
            return;
        }
        
        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);
        
        // Если цель слишком далеко или не видна какое-то время, теряем ее
        if (distanceToTarget > chaseRange || (!hasLineOfSight && Vector2.Distance(transform.position, lastKnownPosition) < 0.5f))
        {
            Debug.Log($"{gameObject.name}: Потерял цель!");
            currentState = AIState.Return;
            currentTarget = null;
        }
    }
    
    private void MoveTowards(Vector2 targetPosition, float speed)
    {
        if (rb == null) return;
        
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        
        // Движение
        rb.velocity = direction * speed;
        
        // Поворот спрайта в сторону движения
        if (spriteRenderer != null && direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }
    
    private Vector2 GetForwardDirection()
    {
        return spriteRenderer.flipX ? Vector2.left : Vector2.right;
    }
    #endregion
    
    #region Визуальные эффекты
    private void UpdateVisuals()
    {
        // Можно добавить анимации в зависимости от состояния
    }
    
    private IEnumerator HideExclamationMark(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (exclamationMark != null)
        {
            exclamationMark.SetActive(false);
        }
    }
    #endregion
    
    #region События
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerCaught();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Если игрок вошел в триггер, начинаем преследование
            if (currentState != AIState.Chase)
            {
                StartChasing(collision.transform);
            }
        }
    }
    
    private void PlayerCaught()
    {
        Debug.Log($"{gameObject.name}: Игрок пойман!");
        
        // Вызываем событие поимки
        if (EventManager.Instance != null)
        {
            EventManager.Instance.PlayerCaught();
        }
    }
    #endregion
    
    #region Визуализация в редакторе
    private void OnDrawGizmosSelected()
    {
        // Отображение радиуса обнаружения
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Отображение угла обзора
        Vector2 forward = spriteRenderer != null && spriteRenderer.flipX ? Vector2.left : Vector2.right;
        Vector2 leftBoundary = Quaternion.Euler(0, 0, detectionAngle / 2) * forward;
        Vector2 rightBoundary = Quaternion.Euler(0, 0, -detectionAngle / 2) * forward;
        
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawRay(transform.position, leftBoundary * detectionRange);
        Gizmos.DrawRay(transform.position, rightBoundary * detectionRange);
        Gizmos.DrawLine(
            transform.position + (Vector3)(leftBoundary * detectionRange),
            transform.position + (Vector3)(rightBoundary * detectionRange)
        );
        
        // Отображение радиуса преследования
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        
        // Отображение пути патрулирования
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
                if (i == patrolPoints.Length - 1 && patrolPoints[0] != null)
                {
                    Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[0].position);
                }
            }
        }
        
        // Отображение текущей цели
        if (currentTarget != null && currentState == AIState.Chase)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
    
    private void OnDrawGizmos()
    {
        // Текущее состояние цветом
        switch (currentState)
        {
            case AIState.Patrol:
                Gizmos.color = Color.blue;
                break;
            case AIState.Chase:
                Gizmos.color = Color.red;
                break;
            case AIState.Return:
                Gizmos.color = Color.yellow;
                break;
        }
        
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
    #endregion
}