/*using UnityEngine;
using UnityEngine.Rendering.Universal; // Правильное пространство имён

public class NeonFlicker : MonoBehaviour
{
    public Light2D light2D;
    public float minIntensity = 1.5f;
    public float maxIntensity = 2.5f;
    public float speed = 5f;

    void Start()
    {
        // Если не назначено в инспекторе, пробуем получить компонент
        if (light2D == null)
            light2D = GetComponent<Light2D>();
            
        if (light2D == null)
            Debug.LogError("Light2D component not found on " + gameObject.name);
    }

    void Update()
    {
        if (light2D == null) return;
        
        // Используем Perlin Noise для плавного мерцания
        float noise = Mathf.PerlinNoise(Time.time * speed, 0f);
        light2D.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
    }
}*/