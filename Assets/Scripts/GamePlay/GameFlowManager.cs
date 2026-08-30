using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField]
    private PlayerMovement playerMovement;

    [Header("Start UI")]
    [SerializeField]
    private GameObject startOverlay;
    [SerializeField]
    private TMP_Text countdownText;

    [Header("Countdown")]
    [SerializeField]
    private float numberDuration = 1f;
    [SerializeField]
    private float goDuration = 0.5f;

    [Header("Clear UI")]
    [SerializeField]
    private GameObject clearOverlay;
    [SerializeField]
    private TMP_Text clearTimeText;
    [SerializeField]
    private GameObject resultGroup;
    [SerializeField]
    private GameObject mainMenuButton;
    [SerializeField]
    private float resultDelay = 0.8f;

    private bool gameStarted;
    public bool GameStarted => gameStarted;

    private float elapsedTime;
    public float ElapsedTime => elapsedTime;

    private bool gameFinished;


    private void Start()
    {
        clearOverlay.SetActive(false);
        StartCoroutine(StartGameRoutine());
    }

    private void Update()
    {
        if (!gameStarted || gameFinished)
        {
            return;
        }

        elapsedTime += Time.deltaTime;
    }


    private IEnumerator StartGameRoutine()
    {
        gameStarted = false;
        playerMovement.SetControlsLocked(true);
        startOverlay.SetActive(true);

        countdownText.text = "3";
        yield return new WaitForSeconds(numberDuration);

        countdownText.text = "2";
        yield return new WaitForSeconds(numberDuration);

        countdownText.text = "1";
        yield return new WaitForSeconds(numberDuration);

        countdownText.text = "GO!";
        elapsedTime = 0f;
        gameFinished = false;
        playerMovement.SetControlsLocked(false);
        gameStarted = true;
        yield return new WaitForSeconds(goDuration);
        startOverlay.SetActive(false);
    }

    private IEnumerator FinishGameRoutine()
    {
        clearOverlay.SetActive(true);
        resultGroup.SetActive(false);
        mainMenuButton.SetActive(false);
        yield return new WaitForSeconds(resultDelay);

        clearTimeText.text = GetFormattedTime();
        resultGroup.SetActive(true);
        mainMenuButton.SetActive(true);
    }

    public void FinishGame()
    {
        if (!gameStarted || gameFinished)
        {
            return;
        }

        gameFinished = true;
        GameRecords.RecordClear(elapsedTime);
        playerMovement.SetControlsLocked(true);
        StartCoroutine(FinishGameRoutine());
    }

    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        int milliseconds = Mathf.FloorToInt((elapsedTime * 100f) % 100f);

        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
    }

    public void RetryGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}