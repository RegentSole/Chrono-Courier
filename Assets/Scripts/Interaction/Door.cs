using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private bool isLocked = false;
    [SerializeField] private string requiredKey = ""; // Для будущего расширения
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private Vector2 openOffset = new Vector2(0, 2f);
    
    [Header("References")]
    [SerializeField] private SpriteRenderer doorSprite;
    [SerializeField] private Collider2D doorCollider;
    
    private Vector2 closedPosition;
    private Vector2 openPosition;
    private bool isOpen = false;
    private bool isMoving = false;
    
    private void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + openOffset;
        
        if (doorCollider == null)
        {
            doorCollider = GetComponent<Collider2D>();
        }
    }
    
    public void Open()
    {
        if (isLocked || isOpen || isMoving) return;
        
        StartCoroutine(MoveDoor(openPosition, true));
    }
    
    public void Close()
    {
        if (!isOpen || isMoving) return;
        
        StartCoroutine(MoveDoor(closedPosition, false));
    }
    
    public void Toggle()
    {
        if (isOpen) Close();
        else Open();
    }
    
    public void Unlock()
    {
        isLocked = false;
        Debug.Log("Door unlocked");
    }
    
    public void Lock()
    {
        isLocked = true;
        Debug.Log("Door locked");
    }
    
    private System.Collections.IEnumerator MoveDoor(Vector2 targetPosition, bool opening)
    {
        isMoving = true;
        
        while (Vector2.Distance(transform.position, targetPosition) > 0.01f)
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
        isMoving = false;
        
        // Включаем/выключаем коллайдер
        if (doorCollider != null)
        {
            doorCollider.enabled = !isOpen;
        }
        
        Debug.Log($"Door is now {(isOpen ? "open" : "closed")}");
    }
}