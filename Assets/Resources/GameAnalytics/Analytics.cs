using UnityEngine;
using GameAnalyticsSDK;

public class Analytics : MonoBehaviour
{
    void Start()
    {
        // Инициализация GameAnalytics. Ключи SDK подтянутся автоматически из настроек.
        GameAnalytics.Initialize();
        
        // (Опционально) Включить подробный лог для отладки, потом отключить
        // GameAnalytics.SetEnabledInfoLog(true);
        // GameAnalytics.SetEnabledVerboseLog(true);
    }
}