using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField]
    private AudioMixer audioMixer;
    [SerializeField]
    private AudioSource bgmSource;
    [SerializeField]
    private AudioSource sfxSource;

    [Header("BGM")]
    [SerializeField]
    private AudioClip mainMenuBgm;
    [SerializeField]
    private AudioClip gameplayBgm;

    [Header("Test SFX")]
    [SerializeField]
    private AudioClip testSfx;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        ApplyVolumes();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        PlayBgmForScene(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayBgmForScene(scene.name);
    }

    private void PlayBgmForScene(string sceneName)
    {
        AudioClip targetClip = null;

        if (sceneName == "MainMenu")
        {
            targetClip = mainMenuBgm;
        }
        else if (sceneName == "Gameplay")
        {
            targetClip = gameplayBgm;
        }

        if (targetClip == null)
        {
            return;
        }

        if (bgmSource.clip == targetClip && bgmSource.isPlaying)
        {
            return;
        }

        bgmSource.clip = targetClip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void ApplyVolumes()
    {
        SetBgmVolume(GameSettings.BgmVolume);
        SetSfxVolume(GameSettings.SfxVolume);
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip == null || sfxSource == null)
        {
            return;
        }

        sfxSource.PlayOneShot(clip);
    }

    public void SetBgmVolume(float volume)
    {
        GameSettings.BgmVolume = volume;
        audioMixer.SetFloat("BGMVolume", VolumeToDecibel(volume));
    }


    public void SetSfxVolume(float volume)
    {
        GameSettings.SfxVolume = volume;
        audioMixer.SetFloat("SFXVolume", VolumeToDecibel(volume));
    }

    private float VolumeToDecibel(float volume)
    {
        if (volume <= 0.0001f)
        {
            return -80f;
        }

        return Mathf.Log10(volume) * 20f;
    }
}