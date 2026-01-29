using UnityEngine;

public class EnemyGameOver : MonoBehaviour
{
    [SerializeField] private string gameOverReason = "Вас обнаружил охранник!";
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Игрок пойман врагом: " + gameOverReason);
            
            // Показываем UI поражения
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowGameOver(gameOverReason);
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Игрок пойман врагом (триггер): " + gameOverReason);
            
            // Показываем UI поражения
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowGameOver(gameOverReason);
            }
        }
    }
}