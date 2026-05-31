using UnityEngine;
using UnityEngine.Events;

public class Button : Interactable
{
    [Header("Button Settings")]
    [SerializeField] private bool stayPressed = false; // Кнопка остается нажатой навсегда?
    
    [Header("Events")]
    [SerializeField] private UnityEvent onButtonPressed;
    [SerializeField] private UnityEvent onButtonReleased;
    
    private bool isCurrentlyActivated = false;

    protected override void SetActivated(bool activated)
    {
        // Если кнопка уже была нажата И включен режим "залипания" — полностью игнорируем любые изменения
        if (stayPressed && isCurrentlyActivated) return;

        // Если состояние не изменилось (например, нажали нажатую кнопку) — ничего не делаем
        if (activated == isCurrentlyActivated) return;

        base.SetActivated(activated);
        isCurrentlyActivated = activated;

        if (isCurrentlyActivated)
        {
            Debug.Log("Кнопка нажата");
            onButtonPressed?.Invoke();
        }
        else
        {
            Debug.Log("Кнопка отпущена");
            onButtonReleased?.Invoke();
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonPress();
    }
}
