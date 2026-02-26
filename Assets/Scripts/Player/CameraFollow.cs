using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    [Header("Settings")]
    public Transform target;               // Цель для следования
    public float smoothing = 5f;           // Скорость сглаживания
    public Vector3 offset = new Vector3(0, 0, -10); // Смещение камеры
    
    void Start()
    {
        // Автоматически находим игрока, если цель не задана
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
        
        // Устанавливаем начальную позицию
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }
    
    void FixedUpdate()
    {
        if (target == null) return;
        
        // Плавное перемещение к цели
        Vector3 targetPosition = target.position + offset;
        targetPosition.z = transform.position.z; // Сохраняем Z
        
        transform.position = Vector3.Lerp(
            transform.position, 
            targetPosition, 
            smoothing * Time.deltaTime
        );
    }
    
    // Метод для установки новой цели
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}