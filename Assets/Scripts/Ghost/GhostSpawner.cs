using UnityEngine;

public class GhostSpawner : MonoBehaviour
{
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private Transform ghostParent;
    
    private GhostController currentGhost;
    
    private void OnEnable()
    {
        // Подписываемся на событие завершения записи
        var playerRecording = FindObjectOfType<PlayerRecording>();
        if (playerRecording != null)
        {
            playerRecording.OnRecordingComplete += SpawnGhost;
        }
    }
    
    private void OnDisable()
    {
        var playerRecording = FindObjectOfType<PlayerRecording>();
        if (playerRecording != null)
        {
            playerRecording.OnRecordingComplete -= SpawnGhost;
        }
    }
    
    private void SpawnGhost(RecordFrame[] recording)
    {
        if (recording == null || recording.Length == 0) return;
        
        // Удаляем старый призрак
        if (currentGhost != null)
        {
            currentGhost.ResetGhost();
            Destroy(currentGhost.gameObject);
        }
        
        // Создаем нового призрака
        GameObject ghostObj = Instantiate(ghostPrefab, ghostParent);
        ghostObj.transform.position = recording[0].position;
        
        currentGhost = ghostObj.GetComponent<GhostController>();
        if (currentGhost != null)
        {
            currentGhost.StartReplay(recording);
        }
        
        Debug.Log("Ghost spawned!");
    }
}