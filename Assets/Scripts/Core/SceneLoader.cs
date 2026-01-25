using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }
    
    [SerializeField] private CanvasGroup loadingScreen;
    [SerializeField] private float fadeTime = 0.5f;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }
    
    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(LoadSceneAsync(sceneIndex));
    }
    
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // Показываем экран загрузки
        if (loadingScreen != null)
        {
            loadingScreen.gameObject.SetActive(true);
            yield return StartCoroutine(FadeCanvas(loadingScreen, 0f, 1f, fadeTime));
        }
        
        // Загружаем сцену асинхронно
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;
        
        while (!operation.isDone)
        {
            // Ждем загрузки
            if (operation.progress >= 0.9f)
            {
                operation.allowSceneActivation = true;
            }
            
            yield return null;
        }
        
        // Скрываем экран загрузки
        if (loadingScreen != null)
        {
            yield return StartCoroutine(FadeCanvas(loadingScreen, 1f, 0f, fadeTime));
            loadingScreen.gameObject.SetActive(false);
        }
    }
    
    private IEnumerator LoadSceneAsync(int sceneIndex)
    {
        yield return LoadSceneAsync(SceneManager.GetSceneByBuildIndex(sceneIndex).name);
    }
    
    private IEnumerator FadeCanvas(CanvasGroup canvas, float startAlpha, float endAlpha, float duration)
    {
        float time = 0f;
        
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            canvas.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }
        
        canvas.alpha = endAlpha;
    }
}