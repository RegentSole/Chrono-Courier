using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private bool isLocked = false;
    [SerializeField] private float openSpeed = 5f; // Увеличил скорость для отзывчивости
    [SerializeField] private Vector2 openOffset = new Vector2(0, 2f);
    
    [Header("References")]
    [SerializeField] private Collider2D doorCollider;
    
    private Vector2 closedPosition;
    private Vector2 openPosition;
    private bool isOpen = false;
    
    private Coroutine movementCoroutine; // Ссылка на запущенную корутину

    private void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + openOffset;
        
        if (doorCollider == null) doorCollider = GetComponent<Collider2D>();
    }
    
    public void Open()
    {
        if (isLocked) return;
        // Если уже открыта и не движется — ничего не делаем
        if (isOpen && movementCoroutine == null) return; 

        StartDoorMovement(openPosition, true);
    }
    
    public void Close()
    {
        // Если уже закрыта и не движется — ничего не делаем
        if (!isOpen && movementCoroutine == null) return;

        StartDoorMovement(closedPosition, false);
    }

    private void StartDoorMovement(Vector2 target, bool opening)
    {
        // Если дверь уже куда-то двигалась — останавливаем старое движение
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }
        
        // Запускаем новое движение к цели
        movementCoroutine = StartCoroutine(MoveDoor(target, opening));
    }
    
    private System.Collections.IEnumerator MoveDoor(Vector2 targetPosition, bool opening)
    {
        // Коллайдер отключаем сразу, если начали открывать, 
        // и включаем сразу, если начали закрывать (чтобы нельзя было пройти)
        //if (doorCollider != null)
        //{
        //    doorCollider.enabled = !opening; 
        //}

        while (Vector2.Distance(transform.position, targetPosition) > 0.001f)
        {
            transform.position = Vector2.MoveTowards(
                transform.position, 
                targetPosition, 
                openSpeed * Time.deltaTime
            );
            yield return null;
        }
        
        transform.position = targetPosition;
        isOpen = opening;
        movementCoroutine = null; // Движение закончено
    }

    // Остальные методы (Lock/Unlock/Toggle) можно оставить без изменений
    public void Toggle() { if (isOpen) Close(); else Open(); }
    public void Unlock() => isLocked = false;
    public void Lock() => isLocked = true;
}
