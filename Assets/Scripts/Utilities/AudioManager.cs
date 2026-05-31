using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip levelMusic;
    [SerializeField] private AudioClip chaseMusic;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.5f;
    private AudioSource musicSource;
    private AudioSource secondaryMusicSource;

    [Header("SFX")]
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private AudioClip enemyDetected;
    [SerializeField] private AudioClip enemyLostTarget;
    [SerializeField] private AudioClip buttonPress;
    [SerializeField] private AudioClip pressurePlatePress;
    [SerializeField] private AudioClip levelComplete;
    [SerializeField] private AudioClip gameOver;
    [SerializeField] private AudioClip uiClick;
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 0.7f;
    private AudioSource sfxSource;

    [Header("Volume Multipliers")]
    [SerializeField] private float levelCompleteVolume = 0.4f;
    [SerializeField] private float gameOverVolume = 0.4f;

    [Header("Player Sounds")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip doubleJumpSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioClip recordingStartSound;
    [SerializeField] private AudioClip recordingStopSound;
    [SerializeField] private AudioClip ghostReplaySound;

    private bool isChasing = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
            LoadVolumes(); // Загружаем сохранённые настройки
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        secondaryMusicSource = gameObject.AddComponent<AudioSource>();
        secondaryMusicSource.loop = true;
        secondaryMusicSource.volume = 0f;
        secondaryMusicSource.playOnAwake = false;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void LoadVolumes()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.7f);
        if (musicSource) musicSource.volume = musicVolume;
        if (sfxSource) sfxSource.volume = sfxVolume;
        Debug.Log($"Loaded volumes: Music={musicVolume}, SFX={sfxVolume}");
    }

    private void SaveVolumes()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
        Debug.Log($"Saved volumes: Music={musicVolume}, SFX={sfxVolume}");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            isChasing = false;
            PlayMusic(menuMusic);
        }
        else if (scene.name.Contains("Level"))
        {
            isChasing = false;
            PlayMusic(levelMusic);
        }
    }

    #region Music Control
    public void PlayMusic(AudioClip clip, float fadeTime = 0.5f)
    {
        if (clip == null)
        {
            Debug.LogWarning("Попытка воспроизвести null музыку.");
            return;
        }
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        if (fadeTime > 0)
            StartCoroutine(CrossfadeMusic(clip, fadeTime));
        else
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }

    public void StartChaseMusic()
    {
        if (isChasing) return;
        if (chaseMusic == null) return;
        isChasing = true;
        PlayMusic(chaseMusic, 0.3f);
    }

    public void StopChaseMusic()
    {
        if (!isChasing) return;
        isChasing = false;
        PlayMusic(levelMusic ?? menuMusic, 0.3f);
    }

    public void ResetToLevelMusic()
    {
        if (levelMusic == null) return;
        isChasing = false;
        musicSource.Stop();
        musicSource.clip = levelMusic;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    private IEnumerator CrossfadeMusic(AudioClip newClip, float duration)
    {
        secondaryMusicSource.clip = newClip;
        secondaryMusicSource.volume = 0f;
        secondaryMusicSource.Play();

        float timer = 0f;
        while (timer < duration)
        {
            float t = timer / duration;
            musicSource.volume = Mathf.Lerp(musicVolume, 0f, t);
            secondaryMusicSource.volume = Mathf.Lerp(0f, musicVolume, t);
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.volume = musicVolume;
        musicSource.Play();
        secondaryMusicSource.Stop();
        secondaryMusicSource.volume = 0f;
    }

    public void StopMusic() => musicSource.Stop();

    public void SetMusicVolume(float vol)
    {
        musicVolume = Mathf.Clamp01(vol);
        if (musicSource) musicSource.volume = musicVolume;
        SaveVolumes();
    }

    public float GetMusicVolume() => musicVolume;
    #endregion

    #region SFX
    public void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (clip != null)
            sfxSource.PlayOneShot(clip, volume * sfxVolume);
    }

    public void PlayFootstep()
    {
        if (footstepClips == null || footstepClips.Length == 0) return;
        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
        PlaySound(clip, 0.5f);
    }

    public void PlayEnemyDetected() => PlaySound(enemyDetected);
    public void PlayEnemyLostTarget() => PlaySound(enemyLostTarget);
    public void PlayButtonPress() => PlaySound(buttonPress);
    public void PlayPressurePlate() => PlaySound(pressurePlatePress);
    public void PlayLevelComplete() => PlaySound(levelComplete, levelCompleteVolume);
    public void PlayGameOver() => PlaySound(gameOver, gameOverVolume);
    public void PlayUIClick() => PlaySound(uiClick, 0.6f);
    public void PlayJump() => PlaySound(jumpSound);
    public void PlayDoubleJump() => PlaySound(doubleJumpSound);
    public void PlayLand() => PlaySound(landSound);
    public void PlayRecordingStart() => PlaySound(recordingStartSound);
    public void PlayRecordingStop() => PlaySound(recordingStopSound);
    public void PlayGhostReplay() => PlaySound(ghostReplaySound);

    public void SetSfxVolume(float vol)
    {
        sfxVolume = Mathf.Clamp01(vol);
        if (sfxSource) sfxSource.volume = sfxVolume;
        SaveVolumes();
    }

    public float GetSfxVolume() => sfxVolume;
    #endregion

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}