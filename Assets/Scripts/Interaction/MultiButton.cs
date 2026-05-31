using UnityEngine;
using UnityEngine.Events;

public class MultiButton : MonoBehaviour
{
    public int buttonIndex;
    public UnityEvent OnButtonPressed;
    public UnityEvent OnButtonReleased;

    private bool isPressed = false;
    private int objectsOnButton = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Ghost"))
        {
            objectsOnButton++;
            if (!isPressed)
            {
                isPressed = true;
                OnButtonPressed?.Invoke();
                AudioManager.Instance?.PlayButtonPress();
                UpdateVisual(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Ghost"))
        {
            objectsOnButton--;
            if (objectsOnButton <= 0)
            {
                isPressed = false;
                OnButtonReleased?.Invoke();
                UpdateVisual(false);
            }
        }
    }

    private void UpdateVisual(bool pressed)
    {
        // меняем цвет спрайта или анимацию
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = pressed ? Color.green : Color.blue;
    }
}