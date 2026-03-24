using UnityEngine;
// Если эта строка горит красным, значит URP не установлен в Package Manager!
using UnityEngine.Rendering.Universal; 

public class NeonFlicker : MonoBehaviour
{
    [Header("Настройки света")]
    public Light2D lightComponent; // Называем иначе, чтобы не путать с типами
    
    public float minIntensity = 1.0f;
    public float maxIntensity = 2.0f;
    public float speed = 5.0f;

    void Start()
    {
        // Пытаемся найти компонент, если он не привязан
        if (lightComponent == null)
        {
            lightComponent = GetComponent<Light2D>();
        }

        if (lightComponent == null)
        {
            Debug.LogError($"На объекте {gameObject.name} не найден компонент Light2D (URP)!");
        }
    }

    void Update()
    {
        if (lightComponent == null) return;

        // Плавное мерцание через шум Перлина
        float noise = Mathf.PerlinNoise(Time.time * speed, 0f);
        lightComponent.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
    }
}
