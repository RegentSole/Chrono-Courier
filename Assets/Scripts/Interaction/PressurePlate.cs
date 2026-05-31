using UnityEngine;
using UnityEngine.Events;

public class PressurePlate : Interactable 
{
    [Header("Pressure Plate Settings")]
    [SerializeField] private bool requireMass = true; 
    [SerializeField] private float requiredMass = 10f;
    
    [Header("Events")]
    [SerializeField] private UnityEvent onPlatePressed;
    [SerializeField] private UnityEvent onPlateReleased;

    private float currentMassOnPlate = 0f;
    private bool isCurrentlyPressed = false;

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsValidObject(collision)) return;

        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            currentMassOnPlate += rb.mass;
            UpdatePlateState();
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        if (!IsValidObject(collision)) return;

        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            currentMassOnPlate = Mathf.Max(0, currentMassOnPlate - rb.mass);
            UpdatePlateState();
        }
    }

    private void UpdatePlateState()
    {
        bool shouldBePressed = !requireMass || currentMassOnPlate >= requiredMass;

        if (shouldBePressed != isCurrentlyPressed)
        {
            isCurrentlyPressed = shouldBePressed;
            SetActivated(isCurrentlyPressed);
        }
    }

    protected override void SetActivated(bool activated)
    {
        // Вызываем базовый метод (он может менять спрайт плиты или звук)
        base.SetActivated(activated);

        if (activated)
        {
            onPlatePressed?.Invoke();
        }
        else
        {
            // Мгновенный вызов события закрытия
            onPlateReleased?.Invoke();
            Debug.Log("Плита отпущена: сигнал на закрытие отправлен мгновенно");
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayPressurePlate();
 //           OnPlatePressed?.Invoke();
    }

    private bool IsValidObject(Collider2D collision)
    {
        return (collision.CompareTag("Player") && canBeActivatedByPlayer) || 
               (collision.CompareTag("Ghost") && canBeActivatedByGhost) || 
               collision.CompareTag("MovableObject");
    }
}
