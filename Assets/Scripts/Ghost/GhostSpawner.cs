using UnityEngine;

public class GhostSpawner : MonoBehaviour
{
    [SerializeField] private GameObject ghostPrefab;
    
    // УБИРАЕМ подписку на события, так как теперь прямой вызов
    private void Start()
    {
        // Больше не подписываемся, просто проверяем наличие префаба
        if (ghostPrefab == null)
        {
            Debug.LogError("Ghost prefab не назначен в GhostSpawner!");
        }
        else
        {
            Debug.Log("GhostSpawner готов к работе");
        }
    }
    
    // Метод ДОЛЖЕН быть public
    public void SpawnGhost(RecordFrame[] recording, Vector2 position)
    {
        if (recording == null || recording.Length == 0)
        {
            Debug.LogWarning("Пустая запись для призрака");
            return;
        }
        
        if (ghostPrefab == null)
        {
            Debug.LogError("Ghost prefab не назначен!");
            return;
        }
        
        // Создаем призрака
        GameObject ghost = Instantiate(ghostPrefab, position, Quaternion.identity);
        GhostController ghostController = ghost.GetComponent<GhostController>();
        
        if (ghostController != null)
        {
            ghostController.StartReplay(recording);
        }
        else
        {
            Debug.LogError("Ghost prefab не имеет GhostController компонента!");
        }
        
        Debug.Log($"Призрак создан с {recording.Length} кадрами");
    }
}