using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private string text = "Нажмите E";
    [SerializeField] private float floatHeight = 1f;
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private Color textColor = Color.white;
    
    private GameObject textObject;
    private TextMeshPro textMesh;
    private Vector3 startPos;
    
    private void Start()
    {
        CreateFloatingText();
    }
    
    private void CreateFloatingText()
    {
        textObject = new GameObject("FloatingText");
        textObject.transform.SetParent(transform);
        textObject.transform.localPosition = Vector3.up * 1.5f;
        
        textMesh = textObject.AddComponent<TextMeshPro>();
        textMesh.text = text;
        textMesh.fontSize = 2;
        textMesh.color = textColor;
        textMesh.alignment = TextAlignmentOptions.Center;
        
        startPos = textObject.transform.localPosition;
        
        // Добавляем Outline
        textMesh.outlineWidth = 0.2f;
        textMesh.outlineColor = Color.black;
    }
    
    private void Update()
    {
        if (textObject != null)
        {
            // Парящая анимация
            float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatHeight * 0.5f;
            textObject.transform.localPosition = startPos + new Vector3(0, yOffset, 0);
            
            // Всегда смотреть на камеру
            textObject.transform.rotation = Quaternion.LookRotation(
                textObject.transform.position - Camera.main.transform.position
            );
        }
    }
    
    public void SetText(string newText)
    {
        text = newText;
        if (textMesh != null)
        {
            textMesh.text = newText;
        }
    }
    
    private void OnDestroy()
    {
        if (textObject != null)
        {
            Destroy(textObject);
        }
    }
}