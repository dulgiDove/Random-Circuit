using UnityEngine;

public static class GameSettings
{
    private const string BgmVolumeKey = "BgmVolume";
    private const string SfxVolumeKey = "SfxVolume";

    public static float BgmVolume
    {
        get
        {
            return PlayerPrefs.GetFloat(BgmVolumeKey, 1f);
        }

        set
        {
            PlayerPrefs.SetFloat(BgmVolumeKey, Mathf.Clamp01(value));
        }
    }

    public static float SfxVolume
    {
        get
        {
            return PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
        }

        set
        {
            PlayerPrefs.SetFloat(SfxVolumeKey, Mathf.Clamp01(value));
        }
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }
}