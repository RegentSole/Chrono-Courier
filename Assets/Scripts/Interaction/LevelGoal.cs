using UnityEngine;

public class LevelGoal : MonoBehaviour
{
    [SerializeField] private string nextLevelName = "";
    [SerializeField] private ParticleSystem confettiEffect;
    
    private bool isActivated = false;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isActivated)
        {
            isActivated = true;
            
            // Эффект
            if (confettiEffect != null)
            {
                confettiEffect.Play();
            }
            
            // Звук
            // AudioSource.PlayClipAtPoint(winSound, transform.position);
            
            // Завершение уровня
            CompleteLevel();
        }
    }
    
    private void CompleteLevel()
    {
        Debug.Log("Level completed!");
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLevelComplete();
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CompleteLevel();
        }
        
        // Автопереход на следующий уровень через 3 секунды
        Invoke("LoadNextLevel", 3f);
    }
    
    private void LoadNextLevel()
    {
        if (!string.IsNullOrEmpty(nextLevelName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextLevelName);
        }
        else
        {
            // Загрузка следующей сцены по индексу
            int nextSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1;
            if (nextSceneIndex < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneIndex);
            }
        }
    }
}