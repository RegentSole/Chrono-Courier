using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject levelSelectPanel;
    
    [Header("Buttons")]
    [SerializeField] private GameObject levelButtonPrefab;
    [SerializeField] private Transform levelButtonsContainer;
    
    [Header("Level Names")]
    [SerializeField] private string[] levelNames = { "Level1", "Level2" }; // Здесь укажите имена ваших сцен
    
    private void Start()
    {
        // Показываем главное меню, скрываем выбор уровней
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        
        // Создаём кнопки уровней
        CreateLevelButtons();
    }
    
    private void CreateLevelButtons()
    {
        if (levelButtonPrefab == null || levelButtonsContainer == null) return;
        
        // Очищаем контейнер от старых кнопок
        foreach (Transform child in levelButtonsContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Создаём кнопку для каждого уровня
        for (int i = 0; i < levelNames.Length; i++)
        {
            int levelIndex = i; // захватываем индекс для замыкания
            GameObject buttonObj = Instantiate(levelButtonPrefab, levelButtonsContainer);
            
            // Настраиваем текст на кнопке
            TMP_Text buttonText = buttonObj.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = $"Уровень {levelIndex + 1}";
            }
            
            // Назначаем обработчик нажатия
            UnityEngine.UI.Button button = buttonObj.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => LoadLevel(levelIndex));
            }
        }
    }
    
    // Вызывается по кнопке "Играть"
    public void OnPlayButton()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(true);
    }
    
    // Вызывается по кнопке "Назад" на панели выбора уровня
    public void OnBackButton()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
    }
    
    // Вызывается по кнопке "Выход"
    public void OnQuitButton()
    {
        Debug.Log("Выход из игры");
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    
    private void LoadLevel(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < levelNames.Length)
        {
            SceneManager.LoadScene(levelNames[levelIndex]);
        }
        else
        {
            Debug.LogError($"Уровень с индексом {levelIndex} не найден!");
        }
    }
}