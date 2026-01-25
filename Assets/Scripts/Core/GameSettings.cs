using UnityEngine;

// Если вы создаете через меню, раскомментируйте следующую строку:
[CreateAssetMenu(fileName = "GameSettings", menuName = "Chrono Courier/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Game Settings")]
    [Tooltip("Общее количество уровней в игре")]
    public int totalLevels = 4;
    
    [Tooltip("Время до перезапуска уровня после смерти (секунды)")]
    public float restartDelay = 2f;
    
    [Header("UI Settings")]
    [Tooltip("Время анимации UI (секунды)")]
    public float uiFadeTime = 0.3f;
    
    [Header("Recording Settings")]
    [Tooltip("Максимальная длительность записи")]
    public float maxRecordDuration = 5f;
    
    [Header("Ghost Settings")]
    [Tooltip("Максимальное количество призраков одновременно")]
    public int maxGhosts = 1;
}