using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 5f;
    
    private bool hasDetectedPlayer = false;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !hasDetectedPlayer)
        {
            PlayerDetected();
        }
    }
    
    private void PlayerDetected()
    {
        hasDetectedPlayer = true;
        
        // Вызываем событие обнаружения
        if (EventManager.Instance != null)
        {
            EventManager.Instance.PlayerDetected();
        }
        
        // Через 1 секунду вызываем событие поимки
        Invoke("CallPlayerCaught", 1f);
        
        Debug.Log("EnemyAI: Игрок обнаружен!");
    }
    
    private void CallPlayerCaught()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.PlayerCaught();
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = hasDetectedPlayer ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}