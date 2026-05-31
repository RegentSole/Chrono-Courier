using UnityEngine;

public class GameOverTrigger : MonoBehaviour
{
    [SerializeField] private string gameOverReason = "You fell into the abyss!";
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Игрок погиб: " + gameOverReason);
            
            // Показываем UI поражения
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowGameOver(gameOverReason);
            }
        }
    }
    
    // Визуализация в редакторе
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawCube(transform.position, GetComponent<BoxCollider2D>().size);
    }
}