using UnityEngine;
using UnityEngine.Events;

public class Button : Interactable
{
    [Header("Button Settings")]
    [SerializeField] private UnityEvent onButtonPressed;
    [SerializeField] private UnityEvent onButtonReleased;
    [SerializeField] private bool stayPressed = false; // Остается нажатой
    
    private bool wasActivated = false;
    
    protected override void SetActivated(bool activated)
    {
        if (stayPressed && wasActivated) return; // Если кнопка остается нажатой
        
        base.SetActivated(activated);
        
        if (activated && !wasActivated)
        {
            onButtonPressed?.Invoke();
            wasActivated = true;
        }
        else if (!activated && wasActivated)
        {
            onButtonReleased?.Invoke();
            wasActivated = false;
        }
    }
}