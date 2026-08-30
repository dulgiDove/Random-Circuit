using TMPro;
using UnityEngine;

public class RecordsUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private GameObject recordsPanel;
    [SerializeField]
    private TMP_Text bestTimeText;
    [SerializeField]
    private TMP_Text totalClearsText;
    [SerializeField]
    private TMP_Text totalRespawnsText;

    private void Start()
    {
        recordsPanel.SetActive(false);
    }

    public void OpenRecords()
    {
        UpdateRecords();
        recordsPanel.SetActive(true);
    }

    public void CloseRecords()
    {
        recordsPanel.SetActive(false);
    }

    private void UpdateRecords()
    {
        float bestTime = GameRecords.BestTime;

        if (bestTime < 0f)
        {
            bestTimeText.text =
                "--:--.--";
        }
        else
        {
            bestTimeText.text =
                FormatTime(bestTime);
        }

        totalClearsText.text = GameRecords.TotalClears.ToString();
        totalRespawnsText.text = GameRecords.TotalRespawns.ToString();
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int milliseconds = Mathf.FloorToInt((time * 100f) % 100f);

        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
    }
}