using UnityEngine;

public class ParallaxInfinite : MonoBehaviour 

{
    private float length, startPos;
    public GameObject cam; // Ссылка на камеру
    public float parallaxEffect; // Скорость (от 0 до 1)

    void Start() {
        startPos = transform.position.x;
        // Берем ширину спрайта
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void LateUpdate() {
        // На сколько передвинулся фон относительно камеры
        float temp = (cam.transform.position.x * (1 - parallaxEffect));
        // На сколько передвинулся фон физически
        float dist = (cam.transform.position.x * parallaxEffect);

        transform.position = new Vector3(startPos + dist, transform.position.y, transform.position.z);

        // Магия бесконечности:
        if (temp > startPos + length) startPos += length;
        else if (temp < startPos - length) startPos -= length;
    }
}

