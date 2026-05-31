using UnityEngine;
using UnityEngine.UI;
using GameAnalyticsSDK;

public class AnalyticsDemo : MonoBehaviour
{
    // Указываем тип явно через UnityEngine.UI, чтобы избежать конфликтов
    public UnityEngine.UI.Button buttonGameStart;
    public UnityEngine.UI.Button buttonLevelStart;
    public UnityEngine.UI.Button buttonWeaponEquip;
    public UnityEngine.UI.Button buttonEnemyDefeat;
    public UnityEngine.UI.Button buttonCoinCollect;

    void Start()
    {
        GameAnalytics.Initialize();

        if (buttonGameStart != null)
            buttonGameStart.onClick.AddListener(OnGameStartClicked);
        if (buttonLevelStart != null)
            buttonLevelStart.onClick.AddListener(OnLevelStartClicked);
        if (buttonWeaponEquip != null)
            buttonWeaponEquip.onClick.AddListener(OnWeaponEquipClicked);
        if (buttonEnemyDefeat != null)
            buttonEnemyDefeat.onClick.AddListener(OnEnemyDefeatClicked);
        if (buttonCoinCollect != null)
            buttonCoinCollect.onClick.AddListener(OnCoinCollectClicked);
    }

    void OnGameStartClicked()
    {
        GameAnalytics.NewDesignEvent("game:start");
        Debug.Log("[Analytics] Отправлено событие: game:start");
    }

    void OnLevelStartClicked()
    {
        GameAnalytics.NewDesignEvent("level:start:1");
        Debug.Log("[Analytics] Отправлено событие: level:start:1");
    }

    void OnWeaponEquipClicked()
    {
        GameAnalytics.NewDesignEvent("weapon:equip:sword");
        Debug.Log("[Analytics] Отправлено событие: weapon:equip:sword");
    }

    void OnEnemyDefeatClicked()
    {
        GameAnalytics.NewDesignEvent("enemy:defeat:dragon");
        Debug.Log("[Analytics] Отправлено событие: enemy:defeat:dragon");
    }

    void OnCoinCollectClicked()
    {
        int coinsAmount = Random.Range(1, 10);
        GameAnalytics.NewDesignEvent("resource:collect:coin", coinsAmount);
        Debug.Log($"[Analytics] Отправлено событие: resource:collect:coin со значением {coinsAmount}");
    }
}
