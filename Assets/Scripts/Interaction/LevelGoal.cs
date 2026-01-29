using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelGoalTrigger : MonoBehaviour
{
    [SerializeField] private string levelName = "Уровень 1";
    [SerializeField] private string nextLevelName = "Level2";
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Игрок достиг цели уровня!");
            
            // Сохраняем прогресс
            int currentLevel = SceneManager.GetActiveScene().buildIndex;
            PlayerPrefs.SetInt("LastCompletedLevel", currentLevel);
            PlayerPrefs.Save();
            
            // Показываем UI завершения уровня
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLevelComplete(levelName);
            }
            
            // Автопереход на следующий уровень через 3 секунды
            Invoke("LoadNextLevel", 3f);
        }
    }
    
    private void LoadNextLevel()
    {
        if (!string.IsNullOrEmpty(nextLevelName))
        {
            SceneManager.LoadScene(nextLevelName);
        }
        else
        {
            // Если следующего уровня нет, перезагружаем текущий
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
    
    // Визуализация в редакторе
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawCube(transform.position, GetComponent<BoxCollider2D>().size);
    }
}