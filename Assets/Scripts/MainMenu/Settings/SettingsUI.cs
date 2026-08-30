using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private GameObject settingsPanel;
    [SerializeField]
    private Slider bgmSlider;
    [SerializeField]
    private Slider sfxSlider;

    private void Start()
    {
        settingsPanel.SetActive(false);
        bgmSlider.value = GameSettings.BgmVolume;
        sfxSlider.value =  GameSettings.SfxVolume;

        bgmSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
    }

    private void OnDestroy()
    {
        bgmSlider.onValueChanged.RemoveListener(OnBgmVolumeChanged);
        sfxSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
    }

    public void OpenSettings()
    {
        bgmSlider.value = GameSettings.BgmVolume;
        sfxSlider.value = GameSettings.SfxVolume;
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        GameSettings.Save();
        settingsPanel.SetActive(false);
    }

    private void OnBgmVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBgmVolume(value);
        }
    }

    private void OnSfxVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSfxVolume(value);
        }
    }
}