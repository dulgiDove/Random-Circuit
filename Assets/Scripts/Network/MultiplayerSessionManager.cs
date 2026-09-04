using System;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Threading.Tasks;

public class MultiplayerSessionManager : MonoBehaviour
{
    [Header("Play Panel")]
    [SerializeField]
    private GameObject playPanel;
    [Header("Views")]
    [SerializeField]
    private GameObject modeSelect;
    [SerializeField]
    private GameObject multiSelect;
    [SerializeField]
    private GameObject joinView;
    [SerializeField]
    private GameObject roomView;

    [Header("Join")]
    [SerializeField]
    private TMP_InputField joinCodeInput;

    [Header("Room")]
    [SerializeField]
    private TMP_Text roomCodeText;
    [SerializeField]
    private TMP_Text playerCountText;
    [SerializeField]
    private GameObject hostPlayButton;
    [SerializeField]
    private GameObject waitingText;

    private static ISession currentSession;
    private bool servicesInitialized;

    private bool isStartingGame;

    public static bool IsSinglePlayer { get; private set; }

    private void Awake()
    {
        Debug.Assert(playPanel != null);

        Debug.Assert(modeSelect != null);
        Debug.Assert(multiSelect != null);
        Debug.Assert(joinView != null);
        Debug.Assert(roomView != null);

        Debug.Assert(joinCodeInput != null);

        Debug.Assert(roomCodeText != null);
        Debug.Assert(playerCountText != null);
        Debug.Assert(hostPlayButton != null);
        Debug.Assert(waitingText != null);
    }

    private async void Start()
    {
        playPanel.SetActive(false);

        try
        {
            await InitializeServicesAsync();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private void OnDestroy()
    {
        UnsubscribeSessionEvents();
    }

    private async System.Threading.Tasks.Task InitializeServicesAsync()
    {
        if (servicesInitialized)
            return;

        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        servicesInitialized = true;
    }

    public void ShowModeSelect()
    {
        modeSelect.SetActive(true);
        multiSelect.SetActive(false);
        joinView.SetActive(false);
        roomView.SetActive(false);
    }

    public void ShowMultiSelect()
    {
        modeSelect.SetActive(false);
        multiSelect.SetActive(true);
        joinView.SetActive(false);
        roomView.SetActive(false);
    }

    public void ShowJoinView()
    {
        modeSelect.SetActive(false);
        multiSelect.SetActive(false);
        joinView.SetActive(true);
        roomView.SetActive(false);

        joinCodeInput.text = "";
    }

    private void ShowRoomView()
    {
        modeSelect.SetActive(false);
        multiSelect.SetActive(false);
        joinView.SetActive(false);
        roomView.SetActive(true);

        RefreshRoomUI();
    }

    public async void CreateRoom()
    {
        IsSinglePlayer = false;
           
        if (!servicesInitialized)
            return;

        try
        {
            SessionOptions options = new SessionOptions{ MaxPlayers = 4 };

            currentSession = await MultiplayerService.Instance.CreateSessionAsync(options);

            SubscribeSessionEvents();

            ShowRoomView();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    public async void JoinRoom()
    {
        IsSinglePlayer = false;

        if (!servicesInitialized)
            return;

        string joinCode = joinCodeInput.text.Trim().ToUpperInvariant();

        if (string.IsNullOrEmpty(joinCode))
            return;

        try
        {
            currentSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(joinCode);

            SubscribeSessionEvents();

            ShowRoomView();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private void SubscribeSessionEvents()
    {
        if (currentSession == null)
            return;

        currentSession.PlayerJoined += OnPlayerJoined;
        currentSession.PlayerHasLeft += OnPlayerHasLeft;
        currentSession.RemovedFromSession += OnRemovedFromSession;
    }

    private void UnsubscribeSessionEvents()
    {
        if (currentSession == null)
            return;

        currentSession.PlayerJoined -= OnPlayerJoined;
        currentSession.PlayerHasLeft -= OnPlayerHasLeft;
        currentSession.RemovedFromSession -= OnRemovedFromSession;
    }

    private void OnPlayerJoined(string playerId)
    {
        RefreshRoomUI();
    }

    private void OnPlayerHasLeft(string playerId)
    {
        RefreshRoomUI();
    }

    private void OnRemovedFromSession()
    {
        UnsubscribeSessionEvents();
        currentSession = null;
        ShowMultiSelect();
    }

    private void RefreshRoomUI()
    {
        if (currentSession == null)
            return;

        roomCodeText.text = $"Code : {currentSession.Code}";

        playerCountText.text = $"Players : {currentSession.PlayerCount} / {currentSession.MaxPlayers}";

        bool isHost = currentSession.IsHost;

        hostPlayButton.SetActive(isHost);
        waitingText.SetActive(!isHost);
    }

    public async void LeaveRoom()
    {
        if (currentSession == null)
        {
            ShowMultiSelect();
            return;
        }

        try
        {
            UnsubscribeSessionEvents();

            await LeaveCurrentSessionAsync();

            ShowMultiSelect();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    public void OpenPlayPanel()
    {
        playPanel.SetActive(true);
        ShowModeSelect();
    }

    public void ClosePlayPanel()
    {
        playPanel.SetActive(false);
    }

    public void StartSingleGame()
    {
        IsSinglePlayer = true;

        if (NetworkManager.Singleton == null)
        {
            return;
        }

        if (!NetworkManager.Singleton.StartHost())
        {
            return;
        }

        NetworkManager.Singleton.SceneManager.LoadScene("Gameplay", LoadSceneMode.Single);
    }

    public async void StartMultiplayerGame()
    {
        if (currentSession == null || !currentSession.IsHost || isStartingGame)
            return;

        try
        {
            isStartingGame = true;
            hostPlayButton.SetActive(false);

            IHostSession hostSession = currentSession.AsHost();

            RelayNetworkOptions networkOptions = new RelayNetworkOptions(RelayProtocol.Default);

            await hostSession.Network.StartRelayNetworkAsync(networkOptions);

            if (NetworkManager.Singleton == null)
            {
                isStartingGame = false;
                hostPlayButton.SetActive(true);
                return;
            }

            NetworkManager.Singleton.SceneManager.LoadScene("Gameplay", LoadSceneMode.Single);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);

            isStartingGame = false;

            if (currentSession != null && currentSession.IsHost)
            {
                hostPlayButton.SetActive(true);
            }
        }
    }

    public static async Task LeaveCurrentSessionAsync()
    {
        if (currentSession == null)
            return;

        ISession session = currentSession;

        await session.LeaveAsync();

        if (currentSession == session)
        {
            currentSession = null;
        }
    }

    public static async Task DeleteCurrentSessionAsync()
    {
        if (currentSession == null)
            return;

        ISession session = currentSession;

        if (!session.IsHost)
            return;

        await session.AsHost().DeleteAsync();

        if (currentSession == session)
        {
            currentSession = null;
        }
    }
}