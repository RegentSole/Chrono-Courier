using UnityEngine;
using GameAnalyticsSDK; // Не забудь добавить пространство имен

public class PlayerActions : MonoBehaviour
{
    // Пример 1: Запуск игры (вызови в меню)
    public void LogGameStart()
    {
        GameAnalytics.NewDesignEvent("game:start");
        Debug.Log("Событие game:start отправлено");
    }

    // Пример 2: Начало уровня (вызови при загрузке уровня)
    public void LogLevelStart(int levelNumber)
    {
        string eventName = $"level:start:{levelNumber}";
        GameAnalytics.NewDesignEvent(eventName);
        Debug.Log($"Событие {eventName} отправлено");
    }

    // Пример 3: Экипировка оружия (вызови при выборе меча)
    public void LogWeaponEquip(string weaponName)
    {
        GameAnalytics.NewDesignEvent($"weapon:equip:{weaponName}");
        Debug.Log($"Событие weapon:equip:{weaponName} отправлено");
    }

    // Пример 4: Победа над врагом (вызови при смерти врага)
    public void LogEnemyDefeat(string enemyType)
    {
        GameAnalytics.NewDesignEvent($"enemy:defeat:{enemyType}");
        Debug.Log($"Событие enemy:defeat:{enemyType} отправлено");
    }

    // Пример 5: Сбор ресурса с числовым значением (вызови при подборе монеты)
    public void LogCoinCollected(int coinAmount)
    {
        GameAnalytics.NewDesignEvent("resource:collect:coin", coinAmount);
        Debug.Log($"Событие resource:collect:coin со значением {coinAmount} отправлено");
    }
}