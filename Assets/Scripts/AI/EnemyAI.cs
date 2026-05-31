using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float waitTime = 1f;
    [SerializeField] private bool isStationary = false;

    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float detectionAngle = 90f;
    [SerializeField] private float backDetectionRange = 2.5f;
    [SerializeField] private float backDetectionAngle = 120f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float chaseRange = 8f;
    [SerializeField] private float lostTargetTime = 1f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color alertColor = Color.red;
    [SerializeField] private GameObject exclamationMark;
    [SerializeField] private bool showVisionVisual = false;
    [SerializeField] private Color visionColor = new Color(1f, 1f, 0f, 0.2f);
    [SerializeField] private Color backVisionColor = new Color(1f, 0.5f, 0f, 0.15f);
    [SerializeField] private int visionSegments = 30;
    [SerializeField] private int visionSortingOrder = 100;
    [SerializeField] private bool defaultFacingRight = true; // true = спрайт смотрит вправо по умолчанию
    [SerializeField] private bool spriteFacesLeft = false; // true если спрайт изначально смотрит влево

    // Объекты для визуализации секторов
    private GameObject visionFrontObject;
    private MeshFilter visionFrontFilter;
    private MeshRenderer visionFrontRenderer;
    private GameObject visionBackObject;
    private MeshFilter visionBackFilter;
    private MeshRenderer visionBackRenderer;

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

    private Vector2 avoidDirection = Vector2.zero;
    private float avoidTimer = 0f;
    private Coroutine hideExclamationCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        if (spriteRenderer != null) spriteRenderer.color = normalColor;
        if (exclamationMark != null) exclamationMark.SetActive(false);
    }

    private void Start()
    {
        if (patrolPoints.Length > 0 && !isStationary)
            transform.position = patrolPoints[0].position;

        if (showVisionVisual && isStationary)
            CreateVisionVisual();
    }

    private void Update()
    {
        if (isStationary)
        {
            StationaryUpdate();
            return;
        }

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
    }

    #region Стационарный режим
    private void StationaryUpdate()
    {
        UpdateVisionVisual();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject ghost = GameObject.FindGameObjectWithTag("Ghost");
        Transform target = null;

        if (player != null && CanSeeTarget(player.transform))
            target = player.transform;
        else if (ghost != null && ghost.activeInHierarchy && CanSeeTarget(ghost.transform))
            target = ghost.transform;

        if (target != null)
        {
            if (currentState != AIState.Chase)
            {
                currentState = AIState.Chase;
                SetVisualsAlert(true);
                EventManager.Instance?.PlayerDetected();
            }
            Vector2 dir = (target.position - transform.position).normalized;
            if (spriteRenderer != null && dir.x != 0)
                spriteRenderer.flipX = dir.x < 0;
        }
        else
        {
            if (currentState != AIState.Patrol)
            {
                currentState = AIState.Patrol;
                SetVisualsAlert(false);
            }
        }
    }
    #endregion

    #region Патрулирование и движение (подвижный)
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
            SetVisualsAlert(false);
        }
    }

    private Transform GetNearestPatrolPoint()
    {
        Transform nearest = patrolPoints[0];
        float nearestDist = Vector2.Distance(transform.position, nearest.position);
        for (int i = 1; i < patrolPoints.Length; i++)
        {
            float d = Vector2.Distance(transform.position, patrolPoints[i].position);
            if (d < nearestDist)
            {
                nearest = patrolPoints[i];
                nearestDist = d;
            }
        }
        return nearest;
    }

    private void MoveTowards(Vector2 targetPos, float speed)
    {
        if (rb == null) return;
        Vector2 direction = (targetPos - (Vector2)transform.position).normalized;
        
        // Проверка препятствий и обход (как было)
        float checkDist = 0.5f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, checkDist, obstacleLayer);
        if (hit.collider != null)
        {
            TryAvoidObstacle(targetPos);
            return;
        }
        if (avoidTimer > 0 && avoidDirection != Vector2.zero)
        {
            rb.velocity = avoidDirection * speed;
            avoidTimer -= Time.deltaTime;
            return;
        }
        rb.velocity = direction * speed;
        
        // Поворот спрайта в зависимости от направления движения и настройки по умолчанию
        if (spriteRenderer != null && direction.x != 0)
        {
            if (defaultFacingRight)
                spriteRenderer.flipX = direction.x < 0;   // движение влево – переворот
            else
                spriteRenderer.flipX = direction.x > 0;   // если по умолчанию смотрит влево, то движение вправо требует переворота
        }
    }

    private void TryAvoidObstacle(Vector2 target)
    {
        if (avoidTimer > 0 && avoidDirection != Vector2.zero) return;
        Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        float checkDist = 0.7f;
        Vector2 curDir = (target - (Vector2)transform.position).normalized;
        foreach (Vector2 d in dirs)
        {
            if (Vector2.Dot(d, curDir) < -0.5f) continue;
            if (Physics2D.Raycast(transform.position, d, checkDist, obstacleLayer).collider == null)
            {
                avoidDirection = d;
                avoidTimer = 0.8f;
                return;
            }
        }
        avoidDirection = Vector2.zero;
        avoidTimer = 0f;
        rb.velocity = Vector2.zero;
    }
    #endregion

    #region Обнаружение и преследование
    private void CheckForTargets()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject ghost = GameObject.FindGameObjectWithTag("Ghost");
        if (player != null && CanSeeTarget(player.transform))
            StartChasing(player.transform);
        else if (ghost != null && ghost.activeInHierarchy && CanSeeTarget(ghost.transform))
            StartChasing(ghost.transform);
    }

    private bool CanSeeTarget(Transform target)
    {
        if (target == null) return false;
        Vector2 toTarget = target.position - transform.position;
        float distance = toTarget.magnitude;
        if (distance > detectionRange && distance > backDetectionRange) return false;

        Vector2 forward = GetForwardDirection();
        float angle = Vector2.Angle(forward, toTarget);

        if (angle <= detectionAngle * 0.5f && distance <= detectionRange)
            return CheckLineOfSight(toTarget, distance);

        float backAngle = Vector2.Angle(-forward, toTarget);
        if (backAngle <= backDetectionAngle * 0.5f && distance <= backDetectionRange)
            return CheckLineOfSight(toTarget, distance);

        return false;
    }

    private bool CheckLineOfSight(Vector2 direction, float distance)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, distance, obstacleLayer);
        bool hasLine = hit.collider == null || hit.collider.transform == currentTarget;
        Debug.DrawRay(transform.position, direction.normalized * distance, hasLine ? Color.green : Color.red);
        return hasLine;
    }

    private void StartChasing(Transform target)
    {
        if (currentState == AIState.Chase) return;
        currentTarget = target;
        lastKnownPosition = target.position;
        lastSeenTime = Time.time;
        currentState = AIState.Chase;
        SetVisualsAlert(true);
        EventManager.Instance?.PlayerDetected();
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayEnemyDetected();
        AudioManager.Instance?.StartChaseMusic();
    }

    private void Chase()
    {
        if (currentTarget == null)
        {
            ResetToPatrol();
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
            ResetToPatrol();
            return;
        }
        bool notSeen = Time.time - lastSeenTime > lostTargetTime;
        bool atLastKnown = Vector2.Distance(transform.position, lastKnownPosition) < 0.5f;
        if (notSeen && atLastKnown)
        {
            ResetToPatrol();
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayEnemyLostTarget();
        }
    }

    private void ResetToPatrol()
    {
        currentState = AIState.Patrol;
        currentTarget = null;
        rb.velocity = Vector2.zero;
        SetVisualsAlert(false);
        AudioManager.Instance?.StopChaseMusic();
    }
    #endregion

    #region Визуальные эффекты
    private void SetVisualsAlert(bool alert)
    {
        if (spriteRenderer != null)
            spriteRenderer.color = alert ? alertColor : normalColor;

        if (exclamationMark != null)
        {
            if (hideExclamationCoroutine != null)
                StopCoroutine(hideExclamationCoroutine);

            if (alert)
            {
                exclamationMark.SetActive(true);
                if (!isStationary)
                    hideExclamationCoroutine = StartCoroutine(HideExclamationAfterDelay(1.5f));
            }
            else
            {
                exclamationMark.SetActive(false);
            }
        }
    }

    private IEnumerator HideExclamationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (exclamationMark != null && currentState != AIState.Chase)
            exclamationMark.SetActive(false);
    }

    private Vector2 GetForwardDirection() => spriteRenderer.flipX ? Vector2.left : Vector2.right;
    #endregion

    #region Визуализация секторов обзора
    private void CreateVisionVisual()
    {
        if (visionFrontObject == null)
        {
            visionFrontObject = new GameObject("VisionFrontSector");
            visionFrontObject.transform.SetParent(transform);
            visionFrontObject.transform.localPosition = Vector3.zero;
            visionFrontFilter = visionFrontObject.AddComponent<MeshFilter>();
            visionFrontRenderer = visionFrontObject.AddComponent<MeshRenderer>();
            SetupMaterial(visionFrontRenderer, visionColor);
            UpdateVisionMesh(visionFrontFilter, detectionRange, detectionAngle, false);
        }

        if (backDetectionRange > 0 && visionBackObject == null)
        {
            visionBackObject = new GameObject("VisionBackSector");
            visionBackObject.transform.SetParent(transform);
            visionBackObject.transform.localPosition = Vector3.zero;
            visionBackFilter = visionBackObject.AddComponent<MeshFilter>();
            visionBackRenderer = visionBackObject.AddComponent<MeshRenderer>();
            SetupMaterial(visionBackRenderer, backVisionColor);
            UpdateVisionMesh(visionBackFilter, backDetectionRange, backDetectionAngle, true);
        }
    }

    private void SetupMaterial(MeshRenderer renderer, Color color)
    {
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.renderQueue = 3000;
        renderer.material = mat;
        renderer.sortingOrder = visionSortingOrder;
    }

    private void UpdateVisionMesh(MeshFilter filter, float range, float angle, bool isBack)
    {
        if (filter == null) return;
        float angleRad = angle * Mathf.Deg2Rad;
        float startAngle = -angleRad * 0.5f;
        float step = angleRad / visionSegments;

        List<Vector3> verts = new List<Vector3> { Vector3.zero };
        for (int i = 0; i <= visionSegments; i++)
        {
            float ang = startAngle + i * step;
            Vector3 dir = new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0);
            if (!isBack && spriteRenderer != null && spriteRenderer.flipX)
                dir.x = -dir.x;
            verts.Add(dir * range);
        }

        List<int> tris = new List<int>();
        for (int i = 1; i <= visionSegments; i++)
        {
            tris.Add(0);
            tris.Add(i);
            tris.Add(i + 1);
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        filter.mesh = mesh;
    }

    private void UpdateVisionVisual()
    {
        if (!showVisionVisual || !isStationary)
        {
            if (visionFrontObject != null) visionFrontObject.SetActive(false);
            if (visionBackObject != null) visionBackObject.SetActive(false);
            return;
        }

        if (visionFrontObject == null) CreateVisionVisual();
        else visionFrontObject.SetActive(true);

        if (backDetectionRange > 0 && visionBackObject == null) CreateVisionVisual();
        else if (backDetectionRange > 0 && visionBackObject != null) visionBackObject.SetActive(true);
        else if (backDetectionRange <= 0 && visionBackObject != null) visionBackObject.SetActive(false);
    }
    #endregion

    #region События
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!isStationary && col.gameObject.CompareTag("Player"))
            PlayerCaught();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!isStationary && col.CompareTag("Player") && currentState != AIState.Chase)
            StartChasing(col.transform);
    }

    private void PlayerCaught()
    {
        Debug.Log($"{name}: Игрок пойман!");
        EventManager.Instance?.PlayerCaught();
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayGameOver();
    }
    #endregion
}