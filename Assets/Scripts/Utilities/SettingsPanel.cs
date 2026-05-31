using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    //[SerializeField] private GameObject panel; // сама панель настроек

    private void Start()
    {
        // Подписываемся на события изменения слайдеров
        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
        
        // При открытии панели обновляем значения слайдеров из AudioManager
        //panel.SetActive(false);
    }

    private void OnEnable()
    {
        // Обновляем значения слайдеров при каждом показе панели
        if (AudioManager.Instance != null)
        {
            musicSlider.value = AudioManager.Instance.GetMusicVolume();
            sfxSlider.value = AudioManager.Instance.GetSfxVolume();
        }
    }

    private void OnMusicVolumeChanged(float value)
    {
        AudioManager.Instance?.SetMusicVolume(value);
    }

    private void OnSfxVolumeChanged(float value)
    {
        AudioManager.Instance?.SetSfxVolume(value);
    }

    /*public void TogglePanel()
    {
        panel.SetActive(!panel.activeSelf);
        if (panel.activeSelf) OnEnable(); // при открытии обновляем
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
    }*/
}