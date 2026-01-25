using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [SerializeField] private string tutorialMessage = "";
    [SerializeField] private float displayTime = 5f;
    [SerializeField] private bool triggerOnce = true;
    
    private bool triggered = false;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && (!triggerOnce || !triggered))
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowTutorial(tutorialMessage);
            }
            
            triggered = true;
            
            // Автоматическое уничтожение триггера
            if (triggerOnce)
            {
                Destroy(gameObject);
            }
        }
    }
}