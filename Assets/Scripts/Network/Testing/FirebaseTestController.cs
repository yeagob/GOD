using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FirebaseTestController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button testConnectionButton;
    [SerializeField] private Button createMatchButton;
    [SerializeField] private Button joinMatchButton;
    [SerializeField] private Button movePlayerButton;
    [SerializeField] private Button rollDiceButton;
    [SerializeField] private Button endMatchButton;
    [SerializeField] private TMP_InputField matchIdInput;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text logText;
    
    [Header("Test Configuration")]
    [SerializeField] private string testPlayerId = "test_player_1";
    [SerializeField] private string testBoardType = "classic";
    [SerializeField] private int maxPlayers = 4;
    
    private NetworkInstaller networkInstaller;
    private FirebaseService firebaseService;
    private MatchPresenter matchPresenter;
    private PlayerMatchPresenter playerMatchPresenter;
    private GameEventPresenter gameEventPresenter;
    
    private string currentMatchId;
    private int currentPlayerPosition = 0;
    private int currentTurn = 0;
    
    private void Start()
    {
        InitializeComponents();
        SetupUI();
        StartCoroutine(WaitForFirebaseInitialization());
    }
    
    private void InitializeComponents()
    {
        networkInstaller = FindObjectOfType<NetworkInstaller>();
        if (networkInstaller == null)
        {
            var installerGO = new GameObject("NetworkInstaller");
            networkInstaller = installerGO.AddComponent<NetworkInstaller>();
        }
        
        firebaseService = networkInstaller.GetFirebaseService();
        matchPresenter = networkInstaller.GetMatchPresenter();
        playerMatchPresenter = networkInstaller.GetPlayerMatchPresenter();
        gameEventPresenter = networkInstaller.GetGameEventPresenter();
    }
    
    private void SetupUI()
    {
        if (testConnectionButton != null)
        {
            testConnectionButton.onClick.AddListener(TestConnection);
        }
        
        if (createMatchButton != null)
        {
            createMatchButton.onClick.AddListener(CreateMatch);
        }
        
        if (joinMatchButton != null)
        {
            joinMatchButton.onClick.AddListener(JoinMatch);
        }
        
        if (movePlayerButton != null)
        {
            movePlayerButton.onClick.AddListener(MovePlayer);
        }
        
        if (rollDiceButton != null)
        {
            rollDiceButton.onClick.AddListener(RollDice);
        }
        
        if (endMatchButton != null)
        {
            endMatchButton.onClick.AddListener(EndMatch);
        }
        
        SetButtonsInteractable(false);
        UpdateStatus("Initializing Firebase...");
    }
    
    private void SetupEventListeners()
    {
        if (firebaseService != null)
        {
            firebaseService.OnInitializationComplete += OnFirebaseInitialized;
            firebaseService.OnError += OnFirebaseError;
        }
        
        if (matchPresenter != null)
        {
            matchPresenter.OnMatchCreated += OnMatchCreated;
            matchPresenter.OnMatchLoaded += OnMatchLoaded;
            matchPresenter.OnMatchStarted += OnMatchStarted;
            matchPresenter.OnMatchEnded += OnMatchEnded;
            matchPresenter.OnError += OnError;
        }
        
        if (playerMatchPresenter != null)
        {
            playerMatchPresenter.OnPlayerJoined += OnPlayerJoined;
            playerMatchPresenter.OnPlayerMoved += OnPlayerMoved;
            playerMatchPresenter.OnScoreUpdated += OnScoreUpdated;
            playerMatchPresenter.OnPlayerLeft += OnPlayerLeft;
            playerMatchPresenter.OnError += OnError;
        }
        
        if (gameEventPresenter != null)
        {
            gameEventPresenter.OnEventLogged += OnEventLogged;
            gameEventPresenter.OnPlayerMoveLogged += OnPlayerMoveLogged;
            gameEventPresenter.OnDiceRollLogged += OnDiceRollLogged;
            gameEventPresenter.OnSpecialTileLogged += OnSpecialTileLogged;
            gameEventPresenter.OnError += OnError;
        }
    }
    
    private IEnumerator WaitForFirebaseInitialization()
    {
        yield return new WaitUntil(() => firebaseService != null);
        SetupEventListeners();
        
        yield return new WaitUntil(() => firebaseService.IsInitialized);
        
        UpdateStatus("Firebase initialized successfully!");
        SetButtonsInteractable(true);
    }
    
    private async void TestConnection()
    {
        UpdateStatus("Testing connection...");
        
        try
        {
            var success = await firebaseService.TestConnection();
            if (success)
            {
                UpdateStatus("Connection test successful!");
                AddLog("✓ Firebase connection test passed");
            }
            else
            {
                UpdateStatus("Connection test failed");
                AddLog("✗ Firebase connection test failed");
            }
        }
        catch (Exception ex)
        {
            UpdateStatus($"Connection test error: {ex.Message}");
            AddLog($"✗ Connection test error: {ex.Message}");
        }
    }
    
    private async void CreateMatch()
    {
        UpdateStatus("Creating match...");
        AddLog("Creating new match...");
        
        try
        {
            await matchPresenter.CreateNewMatch(testBoardType, maxPlayers);
        }
        catch (Exception ex)
        {
            UpdateStatus($"Create match error: {ex.Message}");
            AddLog($"✗ Create match error: {ex.Message}");
        }
    }
    
    private async void JoinMatch()
    {
        if (string.IsNullOrEmpty(currentMatchId))
        {
            if (matchIdInput != null && !string.IsNullOrEmpty(matchIdInput.text))
            {
                currentMatchId = matchIdInput.text;
            }
            else
            {
                UpdateStatus("No match ID available");
                return;
            }
        }
        
        var playerName = playerNameInput != null && !string.IsNullOrEmpty(playerNameInput.text) 
            ? playerNameInput.text 
            : "Test Player";
        
        UpdateStatus("Joining match...");
        AddLog($"Joining match {currentMatchId} as {playerName}...");
        
        try
        {
            await matchPresenter.JoinMatch(currentMatchId, testPlayerId, playerName);
        }
        catch (Exception ex)
        {
            UpdateStatus($"Join match error: {ex.Message}");
            AddLog($"✗ Join match error: {ex.Message}");
        }
    }
    
    private async void MovePlayer()
    {
        if (string.IsNullOrEmpty(currentMatchId))
        {
            UpdateStatus("No active match");
            return;
        }
        
        var newPosition = currentPlayerPosition + UnityEngine.Random.Range(1, 7);
        
        UpdateStatus($"Moving player to position {newPosition}...");
        AddLog($"Moving player from {currentPlayerPosition} to {newPosition}...");
        
        try
        {
            await gameEventPresenter.LogPlayerMove(currentMatchId, testPlayerId, currentPlayerPosition, newPosition, currentTurn);
            currentPlayerPosition = newPosition;
            currentTurn++;
        }
        catch (Exception ex)
        {
            UpdateStatus($"Move player error: {ex.Message}");
            AddLog($"✗ Move player error: {ex.Message}");
        }
    }
    
    private async void RollDice()
    {
        if (string.IsNullOrEmpty(currentMatchId))
        {
            UpdateStatus("No active match");
            return;
        }
        
        var diceValue = UnityEngine.Random.Range(1, 7);
        
        UpdateStatus($"Rolling dice: {diceValue}");
        AddLog($"Dice rolled: {diceValue}");
        
        try
        {
            await gameEventPresenter.LogDiceRoll(currentMatchId, testPlayerId, diceValue, currentTurn);
            currentTurn++;
        }
        catch (Exception ex)
        {
            UpdateStatus($"Roll dice error: {ex.Message}");
            AddLog($"✗ Roll dice error: {ex.Message}");
        }
    }
    
    private async void EndMatch()
    {
        if (string.IsNullOrEmpty(currentMatchId))
        {
            UpdateStatus("No active match");
            return;
        }
        
        UpdateStatus("Ending match...");
        AddLog($"Ending match {currentMatchId}...");
        
        try
        {
            await matchPresenter.EndMatch(currentMatchId, testPlayerId);
        }
        catch (Exception ex)
        {
            UpdateStatus($"End match error: {ex.Message}");
            AddLog($"✗ End match error: {ex.Message}");
        }
    }
    
    #region Event Handlers
    
    private void OnFirebaseInitialized(bool success)
    {
        if (success)
        {
            UpdateStatus("Firebase ready for testing!");
            AddLog("✓ Firebase initialized successfully");
        }
        else
        {
            UpdateStatus("Firebase initialization failed");
            AddLog("✗ Firebase initialization failed");
        }
    }
    
    private void OnFirebaseError(string error)
    {
        UpdateStatus($"Firebase error: {error}");
        AddLog($"✗ Firebase error: {error}");
    }
    
    private void OnMatchCreated(MatchData matchData)
    {
        currentMatchId = matchData.MatchId;
        UpdateStatus($"Match created: {currentMatchId}");
        AddLog($"✓ Match created successfully: {currentMatchId}");
        
        if (matchIdInput != null)
        {
            matchIdInput.text = currentMatchId;
        }
    }
    
    private void OnMatchLoaded(MatchData matchData)
    {
        UpdateStatus($"Match loaded: {matchData.MatchId}");
        AddLog($"✓ Match loaded: {matchData.MatchId} - Status: {matchData.Status}");
    }
    
    private void OnMatchStarted(string matchId)
    {
        UpdateStatus($"Match started: {matchId}");
        AddLog($"✓ Match started: {matchId}");
    }
    
    private void OnMatchEnded(string matchId)
    {
        UpdateStatus($"Match ended: {matchId}");
        AddLog($"✓ Match ended: {matchId}");
        
        currentMatchId = null;
        currentPlayerPosition = 0;
        currentTurn = 0;
    }
    
    private void OnPlayerJoined(PlayerMatchData playerData)
    {
        UpdateStatus($"Player joined: {playerData.PlayerName}");
        AddLog($"✓ Player joined: {playerData.PlayerName} in match {playerData.MatchId}");
    }
    
    private void OnPlayerMoved(string playerId, int position)
    {
        UpdateStatus($"Player moved to position: {position}");
        AddLog($"✓ Player {playerId} moved to position {position}");
    }
    
    private void OnScoreUpdated(string playerId, int score)
    {
        AddLog($"✓ Player {playerId} score updated to {score}");
    }
    
    private void OnPlayerLeft(string playerId)
    {
        AddLog($"✓ Player {playerId} left the match");
    }
    
    private void OnEventLogged(GameEventData eventData)
    {
        AddLog($"✓ Event logged: {eventData.EventType} - {eventData.EventData}");
    }
    
    private void OnPlayerMoveLogged(string playerId, int from, int to)
    {
        UpdateStatus($"Move logged: {playerId} from {from} to {to}");
        AddLog($"✓ Move logged: Player {playerId} moved from {from} to {to}");
    }
    
    private void OnDiceRollLogged(string playerId, int value)
    {
        UpdateStatus($"Dice roll logged: {playerId} rolled {value}");
        AddLog($"✓ Dice roll logged: Player {playerId} rolled {value}");
    }
    
    private void OnSpecialTileLogged(string playerId, string tileType, string effect)
    {
        AddLog($"✓ Special tile logged: Player {playerId} hit {tileType} with effect {effect}");
    }
    
    private void OnError(string error)
    {
        UpdateStatus($"Error: {error}");
        AddLog($"✗ Error: {error}");
    }
    
    #endregion
    
    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = $"Status: {message}";
        }
        
        Debug.Log($"[FirebaseTest] {message}");
    }
    
    private void AddLog(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var logMessage = $"[{timestamp}] {message}";
        
        if (logText != null)
        {
            logText.text += logMessage + "\n";
        }
        
        Debug.Log($"[FirebaseTest] {logMessage}");
    }
    
    private void SetButtonsInteractable(bool interactable)
    {
        if (testConnectionButton != null) testConnectionButton.interactable = interactable;
        if (createMatchButton != null) createMatchButton.interactable = interactable;
        if (joinMatchButton != null) joinMatchButton.interactable = interactable;
        if (movePlayerButton != null) movePlayerButton.interactable = interactable;
        if (rollDiceButton != null) rollDiceButton.interactable = interactable;
        if (endMatchButton != null) endMatchButton.interactable = interactable;
    }
    
    private void OnDestroy()
    {
        if (firebaseService != null)
        {
            firebaseService.OnInitializationComplete -= OnFirebaseInitialized;
            firebaseService.OnError -= OnFirebaseError;
        }
        
        if (matchPresenter != null)
        {
            matchPresenter.OnMatchCreated -= OnMatchCreated;
            matchPresenter.OnMatchLoaded -= OnMatchLoaded;
            matchPresenter.OnMatchStarted -= OnMatchStarted;
            matchPresenter.OnMatchEnded -= OnMatchEnded;
            matchPresenter.OnError -= OnError;
        }
        
        if (playerMatchPresenter != null)
        {
            playerMatchPresenter.OnPlayerJoined -= OnPlayerJoined;
            playerMatchPresenter.OnPlayerMoved -= OnPlayerMoved;
            playerMatchPresenter.OnScoreUpdated -= OnScoreUpdated;
            playerMatchPresenter.OnPlayerLeft -= OnPlayerLeft;
            playerMatchPresenter.OnError -= OnError;
        }
        
        if (gameEventPresenter != null)
        {
            gameEventPresenter.OnEventLogged -= OnEventLogged;
            gameEventPresenter.OnPlayerMoveLogged -= OnPlayerMoveLogged;
            gameEventPresenter.OnDiceRollLogged -= OnDiceRollLogged;
            gameEventPresenter.OnSpecialTileLogged -= OnSpecialTileLogged;
            gameEventPresenter.OnError -= OnError;
        }
    }
}
