using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MultiButtonDoor : MonoBehaviour
{
    [SerializeField] private int[] requiredButtonIndices;
    [SerializeField] private Door targetDoor;
    [SerializeField] private GameObject[] indicatorLights;
    [SerializeField] private Color inactiveColor = Color.red;
    [SerializeField] private Color activeColor = Color.green;

    private bool[] buttonStates;

    private void Start()
    {
        buttonStates = new bool[requiredButtonIndices.Length];
        ResetIndicators();
    }

    public void RegisterButtonPress(int index)
    {
        for (int i = 0; i < requiredButtonIndices.Length; i++)
        {
            if (requiredButtonIndices[i] == index)
            {
                buttonStates[i] = true;
                UpdateIndicator(i, true);
                CheckAllPressed();
                break;
            }
        }
    }

    public void RegisterButtonRelease(int index)
    {
        for (int i = 0; i < requiredButtonIndices.Length; i++)
        {
            if (requiredButtonIndices[i] == index)
            {
                buttonStates[i] = false;
                UpdateIndicator(i, false);
                if (targetDoor != null)
                    targetDoor.Close();
                break;
            }
        }
    }

    private void CheckAllPressed()
    {
        foreach (bool state in buttonStates)
            if (!state) return;
        if (targetDoor != null)
            targetDoor.Open();
    }

    private void UpdateIndicator(int idx, bool isActive)
    {
        if (indicatorLights != null && idx < indicatorLights.Length && indicatorLights[idx] != null)
        {
            // Пытаемся изменить цвет Light2D
            var light = indicatorLights[idx].GetComponent<Light2D>();
            if (light != null)
            {
                light.color = isActive ? activeColor : inactiveColor;
            }
            else
            {
                // Если нет Light2D, пробуем SpriteRenderer
                var sr = indicatorLights[idx].GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = isActive ? activeColor : inactiveColor;
            }
        }
    }

    private void ResetIndicators()
    {
        for (int i = 0; i < requiredButtonIndices.Length; i++)
            UpdateIndicator(i, false);
    }
}