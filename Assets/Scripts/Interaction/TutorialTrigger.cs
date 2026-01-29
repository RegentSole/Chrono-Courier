using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [Header("Tutorial Settings")]
    [SerializeField] private string tutorialMessage = "";
    [SerializeField] private float displayTime = 3f;
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private bool destroyAfterTrigger = true;
    
    [Header("Visual Settings")]
    [SerializeField] private bool showVisual = true;
    [SerializeField] private Color gizmoColor = new Color(1f, 1f, 0f, 0.3f);
    
    private bool hasTriggered = false;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggerOnce && hasTriggered) return;
        if (!collision.CompareTag("Player")) return;
        
        hasTriggered = true;
        
        // Показываем сообщение
        if (UIManager.Instance != null && !string.IsNullOrEmpty(tutorialMessage))
        {
       //     UIManager.Instance.ShowTutorial(tutorialMessage, displayTime);
            Debug.Log($"TutorialTrigger: '{tutorialMessage}'");
        }
        
        // Уничтожаем или деактивируем триггер
        if (destroyAfterTrigger)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!showVisual) return;
        
        Gizmos.color = gizmoColor;
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        
        if (collider != null)
        {
            Gizmos.DrawCube(transform.position + (Vector3)collider.offset, collider.size);
        }
    }
}