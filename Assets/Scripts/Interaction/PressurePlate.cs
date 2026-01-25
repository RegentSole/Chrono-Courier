using UnityEngine;
using UnityEngine.Events;

public class PressurePlate : Interactable
{
    [Header("Pressure Plate Settings")]
    [SerializeField] private bool requireWeight = true; // Нужен ли вес для активации
    [SerializeField] private int requiredWeight = 1; // Сколько объектов нужно
    [SerializeField] private UnityEvent onPlatePressed;
    [SerializeField] private UnityEvent onPlateReleased;
    
    private int objectsOnPlate = 0;
    
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsValidObject(collision)) return;
        
        objectsOnPlate++;
        
        if (!requireWeight || objectsOnPlate >= requiredWeight)
        {
            base.OnTriggerEnter2D(collision);
        }
    }
    
    protected override void OnTriggerExit2D(Collider2D collision)
    {
        if (!IsValidObject(collision)) return;
        
        objectsOnPlate = Mathf.Max(0, objectsOnPlate - 1);
        
        if (!requireWeight || objectsOnPlate < requiredWeight)
        {
            base.OnTriggerExit2D(collision);
        }
    }
    
    private bool IsValidObject(Collider2D collision)
    {
        return (collision.CompareTag("Player") && canBeActivatedByPlayer) ||
               (collision.CompareTag("Ghost") && canBeActivatedByGhost) ||
               collision.CompareTag("MovableObject"); // Для расширения
    }
    
    protected override void SetActivated(bool activated)
    {
        base.SetActivated(activated);
        
        if (activated)
        {
            onPlatePressed?.Invoke();
        }
        else
        {
            onPlateReleased?.Invoke();
        }
    }
}