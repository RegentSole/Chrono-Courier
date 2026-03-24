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
    [SerializeField] private float detectionAngle = 90f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float chaseRange = 8f;
    [SerializeField] private float lostTargetTime = 1f;
    [SerializeField] private LayerMask obstacleLayer;
    
    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color alertColor = Color.red;
    [SerializeField] private GameObject exclamationMark;
    
    private enum AIState { Patrol, Chase, Return }
    private AIState currentState = AIState.Patrol;
    private int currentPatrolIndex = 0;
    private bool isWaiting = false;
    private Transform currentTarget = null;
    private float lastSeenTime = 0f;
    
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Vector2 lastKnownPosition;
    private bool hasLineOfSight = false;
    
    // Для плавного обхода
    private Vector2 avoidDirection = Vector2.zero;
    private float avoidTimer = 1f;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        
        if (rb != null)
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        if (spriteRenderer != null)
            spriteRenderer.color = normalColor;
        
        if (exclamationMark != null)
            exclamationMark.SetActive(false);
    }
    
    private void Start()
    {
        if (patrolPoints.Length > 0)
            transform.position = patrolPoints[0].position;
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
        MoveTowards(targetPoint.position, patrolSpeed);
        
        if (Vector2.Distance(transform.position, targetPoint.position) < 0.2f)
            StartCoroutine(WaitAtPoint());
    }
    
    private IEnumerator WaitAtPoint()
    {
        isWaiting = true;
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(waitTime);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        isWaiting = false;
    }
    
    private void ReturnToPatrol()
{
    if (patrolPoints.Length == 0) return;
    
    Transform nearestPoint = GetNearestPatrolPoint();
    MoveTowards(nearestPoint.position, patrolSpeed);
    
    if (Vector2.Distance(transform.position, nearestPoint.position) < 0.2f)
    {
        rb.velocity = Vector2.zero;
        currentPatrolIndex = System.Array.IndexOf(patrolPoints, nearestPoint);
        currentState = AIState.Patrol;
        if (spriteRenderer != null) spriteRenderer.color = normalColor; // возвращаем цвет
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
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject ghost = GameObject.FindGameObjectWithTag("Ghost");
        
        if (player != null && CanSeeTarget(player.transform))
        {
            StartChasing(player.transform);
            return;
        }
        
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
        if (distanceToTarget > detectionRange) return false;
        
        float angleToTarget = Vector2.Angle(GetForwardDirection(), directionToTarget);
        if (angleToTarget > detectionAngle / 2f) return false;
        
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position, 
            directionToTarget.normalized, 
            distanceToTarget, 
            obstacleLayer);
        
        hasLineOfSight = hit.collider == null || hit.collider.transform == target;
        Debug.DrawRay(transform.position, directionToTarget.normalized * detectionRange, 
                     hasLineOfSight ? Color.green : Color.red);
        
        return hasLineOfSight;
    }
    
    private void StartChasing(Transform target)
    {
        currentTarget = target;
        lastKnownPosition = target.position;
        lastSeenTime = Time.time;
        currentState = AIState.Chase;
        
        if (spriteRenderer != null)
            spriteRenderer.color = alertColor;
        
        if (exclamationMark != null)
        {
            exclamationMark.SetActive(true);
            StartCoroutine(HideExclamationMark(1.5f));
        }
        
        if (EventManager.Instance != null)
            EventManager.Instance.PlayerDetected();
        
        Debug.Log($"{gameObject.name}: Начинаю преследовать {target.name}!");
    }
    
    private void Chase()
    {
        if (currentTarget == null)
    {
        currentState = AIState.Return;
        rb.velocity = Vector2.zero;
        if (spriteRenderer != null) spriteRenderer.color = normalColor;
        return;
    }
        
        if (CanSeeTarget(currentTarget))
        {
            lastKnownPosition = currentTarget.position;
            lastSeenTime = Time.time;
        }
        
        MoveTowards(lastKnownPosition, chaseSpeed);
    }
    
    private void CheckIfTargetLost()
{
    if (currentTarget == null)
    {
        currentState = AIState.Return;
        if (spriteRenderer != null) spriteRenderer.color = normalColor;
        return;
    }
    
    bool notSeenLong = Time.time - lastSeenTime > lostTargetTime;
    bool atLastKnown = Vector2.Distance(transform.position, lastKnownPosition) < 0.5f;
    
    if (notSeenLong && atLastKnown)
    {
        Debug.Log($"{gameObject.name}: Потерял цель!");
        currentState = AIState.Return;
        currentTarget = null;
        rb.velocity = Vector2.zero;
        if (spriteRenderer != null) spriteRenderer.color = normalColor; // сброс цвета
    }
}
    
    private void MoveTowards(Vector2 targetPosition, float speed)
{
    if (rb == null) return;
    
    Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
    
    // Проверяем, есть ли препятствие прямо перед носом
    float checkDistance = 0.5f;
    RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, checkDistance, obstacleLayer);
    
    if (hit.collider != null)
    {
        // Препятствие близко — пытаемся обойти
        TryAvoidObstacle(targetPosition);
        return;
    }
    
    // Если обходной таймер активен, продолжаем движение в выбранном направлении
    if (avoidTimer > 0 && avoidDirection != Vector2.zero)
    {
        rb.velocity = avoidDirection * speed;
        avoidTimer -= Time.deltaTime;
        return;
    }
    
    // Нет препятствия и нет активного обхода — двигаемся к цели
    rb.velocity = direction * speed;
    
    if (spriteRenderer != null && direction.x != 0)
        spriteRenderer.flipX = direction.x < 0;
}

private void TryAvoidObstacle(Vector2 target)
{
    // Если уже в процессе обхода, не меняем направление
    if (avoidTimer > 0 && avoidDirection != Vector2.zero)
        return;
    
    Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
    float checkDistance = 0.7f;
    
    foreach (Vector2 dir in directions)
    {
        // Не проверяем направление назад (противоположное движению), чтобы не разворачиваться
        Vector2 currentDirection = (target - (Vector2)transform.position).normalized;
        if (Vector2.Dot(dir, currentDirection) < -0.5f)
            continue;
        
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, checkDistance, obstacleLayer);
        if (hit.collider == null)
        {
            avoidDirection = dir;
            avoidTimer = 0.8f;
            Debug.Log($"{gameObject.name}: обхожу препятствие, направление {dir}");
            return;
        }
    }
    
    // Если все направления заблокированы, стоим
    avoidDirection = Vector2.zero;
    avoidTimer = 0f;
    rb.velocity = Vector2.zero;
}
    
    private Vector2 GetForwardDirection()
    {
        return spriteRenderer.flipX ? Vector2.left : Vector2.right;
    }
    #endregion
    
    #region Визуальные эффекты
    private void UpdateVisuals() { }
    
    private IEnumerator HideExclamationMark(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (exclamationMark != null)
            exclamationMark.SetActive(false);
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
        if (collision.CompareTag("Player") && currentState != AIState.Chase)
        {
            StartChasing(collision.transform);
        }
    }
    
    private void PlayerCaught()
    {
        Debug.Log($"{gameObject.name}: Игрок пойман!");
        if (EventManager.Instance != null)
            EventManager.Instance.PlayerCaught();
    }
    #endregion
}