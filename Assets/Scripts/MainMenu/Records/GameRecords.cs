using UnityEngine;

public static class GameRecords
{
    private const string BestTimeKey = "BestTime";
    private const string TotalClearsKey = "TotalClears";
    private const string TotalRespawnsKey = "TotalRespawns";

    public static float BestTime
    {
        get
        {
            return PlayerPrefs.GetFloat(BestTimeKey, -1f);
        }
    }


    public static int TotalClears
    {
        get
        {
            return PlayerPrefs.GetInt(TotalClearsKey, 0);
        }
    }


    public static int TotalRespawns
    {
        get
        {
            return PlayerPrefs.GetInt(TotalRespawnsKey,0);
        }
    }

    public static void RecordClear(float clearTime)
    {
        float bestTime = BestTime;

        if (bestTime < 0f || clearTime < bestTime)
        {
            PlayerPrefs.SetFloat(BestTimeKey, clearTime);
        }

        PlayerPrefs.SetInt(TotalClearsKey, TotalClears + 1);
        PlayerPrefs.Save();
    }


    public static void RecordRespawn()
    {
        PlayerPrefs.SetInt(TotalRespawnsKey, TotalRespawns + 1);
    }
}