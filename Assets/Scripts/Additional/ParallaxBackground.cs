using UnityEngine;

public class InfiniteParallax : MonoBehaviour
{
    public Camera cam;
    public Transform subject; // Обычно Main Camera
    [Range(0f, 1f)] public float parallaxEffect; // 0 - далеко, 1 - близко
    private float length, startPos;

    void Start()
    {
        startPos = transform.position.x;
        // Получаем ширину спрайта
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void LateUpdate()
    {
        // Вычисляем дистанцию, на которую нужно сместиться
        float distance = (cam.transform.position.x * parallaxEffect);
        
        // Позиция фона с учетом параллакса
        transform.position = new Vector3(startPos + distance, transform.position.y, transform.position.z);

        // Бесконечное повторение
        float temp = (cam.transform.position.x * (1 - parallaxEffect));
        if (temp > startPos + length) startPos += length;
        else if (temp < startPos - length) startPos -= length;
    }
}