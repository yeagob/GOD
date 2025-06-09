using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Network;
using Network.Infrastructure;
using Network.Presenters;

namespace UI.UIPopups
{
    public class MultiplayerPanel : MonoBehaviour
    {
        [SerializeField] private PlayerPanel _playerPanel;
        [SerializeField] private GameObject _playerPanelPrefab;
        [SerializeField] private Transform _playerPanelsParent;
        
        private URLParameterHandler _urlParameterHandler;
        private IPlayerMatchPresenter _playerMatchPresenter;
        private IMatchPresenter _matchPresenter;
        private IPlayerPanelManager _playerPanelManager;
        
        private string _currentMatchId;
        private string _localPlayerId;
        private bool _isClientMode;
        private bool _isListeningForPlayers;

        public void Initialize(string matchId = null)
        {
            Debug.Log($"[MultiplayerPanel] Initialize called with matchId: {matchId}");
            
            InitializeURLHandler();
            InitializeNetworkServices();
            InitializePlayerPanelManager();
            
            _currentMatchId = matchId;
            _isClientMode = !string.IsNullOrEmpty(matchId);
            
            Debug.Log($"[MultiplayerPanel] After initialization - MatchId: {_currentMatchId}, IsClientMode: {_isClientMode}");
            
            if (_playerPanel != null)
            {
                StartCoroutine(FocusPlayerInput());
                SetupPlayerInput();
                HandleMatchFlow();
            }
            else
            {
                Debug.LogError("[MultiplayerPanel] PlayerPanel is null during initialization");
            }
        }

        private void InitializeURLHandler()
        {
            _urlParameterHandler = new URLParameterHandler();
            
            if (_urlParameterHandler.IsMultiplayerMode && string.IsNullOrEmpty(_currentMatchId))
            {
                _currentMatchId = _urlParameterHandler.GetMatchParameter();
                _isClientMode = !string.IsNullOrEmpty(_currentMatchId);
                Debug.Log($"[MultiplayerPanel] URL Handler found MatchId: {_currentMatchId}");
            }
        }

        private void InitializeNetworkServices()
        {
            try
            {
                _playerMatchPresenter = NetworkInstaller.Resolve<IPlayerMatchPresenter>();
                _matchPresenter = NetworkInstaller.Resolve<IMatchPresenter>();
                Debug.Log("[MultiplayerPanel] Network services initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MultiplayerPanel] Failed to resolve Network presenters: {ex.Message}");
                Debug.LogError($"[MultiplayerPanel] Stack trace: {ex.StackTrace}");
            }
        }

        private void InitializePlayerPanelManager()
        {
            if (_playerPanelsParent == null)
            {
                _playerPanelsParent = _playerPanel?.transform.parent;
                Debug.Log($"[MultiplayerPanel] PlayerPanelsParent auto-assigned: {_playerPanelsParent?.name}");
            }

            if (_playerPanelPrefab == null)
            {
                _playerPanelPrefab = _playerPanel?.gameObject;
                Debug.Log($"[MultiplayerPanel] PlayerPanelPrefab auto-assigned: {_playerPanelPrefab?.name}");
            }

            if (_playerPanelsParent != null && _playerPanel != null && _playerPanelPrefab != null)
            {
                _playerPanelManager = new PlayerPanelManager(_playerPanelsParent, _playerPanel, _playerPanelPrefab);
                Debug.Log("[MultiplayerPanel] PlayerPanelManager initialized successfully");
            }
            else
            {
                Debug.LogError($"[MultiplayerPanel] PlayerPanelManager initialization failed - Parent: {_playerPanelsParent}, Panel: {_playerPanel}, Prefab: {_playerPanelPrefab}");
            }
        }

        private void HandleMatchFlow()
        {
            Debug.Log($"[MultiplayerPanel] HandleMatchFlow - IsClientMode: {_isClientMode}");
            
            if (_isClientMode)
            {
                Debug.Log("[MultiplayerPanel] Joining existing match...");
                JoinExistingMatch();
            }
            else
            {
                Debug.Log("[MultiplayerPanel] Creating new match as host...");
                CreateNewMatch();
            }
        }

        private void CreateNewMatch()
        {
            if (_matchPresenter == null)
            {
                Debug.LogError("[MultiplayerPanel] MatchPresenter is null, cannot create match");
                return;
            }

            _currentMatchId = System.Guid.NewGuid().ToString();
            Debug.Log($"[MultiplayerPanel] Generated new MatchId: {_currentMatchId}");
            
            string boardName = _urlParameterHandler.GetBoardParameter();
            string url = $"{Application.absoluteURL.Split('?')[0]}?board={boardName}&match={_currentMatchId}";
            
            Debug.Log($"[MultiplayerPanel] Creating match with URL: {url}");
            
            MatchData newMatch = new MatchData(
                _currentMatchId,
                url,
                MatchState.WaitingForPlayers
            );

            _matchPresenter.CreateMatch(newMatch, OnMatchCreated);
        }

        private void JoinExistingMatch()
        {
            if (_matchPresenter == null)
            {
                Debug.LogError("[MultiplayerPanel] MatchPresenter is null, cannot join match");
                return;
            }

            Debug.Log($"[MultiplayerPanel] Attempting to join match: {_currentMatchId}");
            _matchPresenter.GetMatch(_currentMatchId, OnMatchRetrieved);
        }

        private void OnMatchCreated(bool success)
        {
            Debug.Log($"[MultiplayerPanel] OnMatchCreated - Success: {success}");
            if (!success)
            {
                Debug.LogError("[MultiplayerPanel] Failed to create match");
            }
        }

        private void OnMatchRetrieved(MatchData matchData)
        {
            Debug.Log($"[MultiplayerPanel] OnMatchRetrieved called");
            Debug.Log($"[MultiplayerPanel] MatchData - ID: {matchData._id}, URL: {matchData._url}, State: {matchData._state}");
            
            if (matchData._id != null)
            {
                _currentMatchId = matchData._id;
                Debug.Log($"[MultiplayerPanel] Match retrieved successfully, loading existing players...");
                LoadExistingPlayersInMatch();
            }
            else
            {
                Debug.LogError("[MultiplayerPanel] Failed to retrieve match data - received null or invalid match data");
                Debug.LogError($"[MultiplayerPanel] MatchData details: url={matchData._url}, state={matchData._state}");
            }
        }

        private void LoadExistingPlayersInMatch()
        {
            Debug.Log($"[MultiplayerPanel] LoadExistingPlayersInMatch called for MatchId: {_currentMatchId}");
            
            if (_playerMatchPresenter == null)
            {
                Debug.LogError("[MultiplayerPanel] PlayerMatchPresenter is null, cannot load existing players");
                return;
            }

            if (string.IsNullOrEmpty(_currentMatchId))
            {
                Debug.LogError("[MultiplayerPanel] CurrentMatchId is null or empty, cannot load existing players");
                return;
            }

            Debug.Log("[MultiplayerPanel] Calling GetMatchPlayers...");
            _playerMatchPresenter.GetMatchPlayers(_currentMatchId, OnExistingPlayersLoaded);
        }

        private void OnExistingPlayersLoaded(List<PlayerMatchData> existingPlayers)
        {
            Debug.Log($"[MultiplayerPanel] OnExistingPlayersLoaded called");
            
            if (existingPlayers == null)
            {
                Debug.LogWarning("[MultiplayerPanel] Received null players list");
                return;
            }

            Debug.Log($"[MultiplayerPanel] Received {existingPlayers.Count} existing players");
            
            if (existingPlayers.Count > 0)
            {
                for (int i = 0; i < existingPlayers.Count; i++)
                {
                    var player = existingPlayers[i];
                    Debug.Log($"[MultiplayerPanel] Player {i}: ID={player._id}, Name={player._name}, Color={player._color}, MatchId={player._matchId}");
                }
                
                if (_playerPanelManager != null)
                {
                    Debug.Log("[MultiplayerPanel] Updating player panels with existing players...");
                    _playerPanelManager.UpdatePlayerPanels(existingPlayers);
                }
                else
                {
                    Debug.LogError("[MultiplayerPanel] PlayerPanelManager is null, cannot update panels");
                }
            }
            else
            {
                Debug.Log("[MultiplayerPanel] No existing players found in match or empty list received");
            }
        }

        private void SetupPlayerInput()
        {
            if (_playerPanel?.NameInputField != null)
            {
                _playerPanel.NameInputField.onSubmit.AddListener(OnPlayerNameSubmit);
                SetDefaultPlayerName();
                Debug.Log("[MultiplayerPanel] Player input setup completed");
            }
            else
            {
                Debug.LogError("[MultiplayerPanel] PlayerPanel or NameInputField is null");
            }
        }

        private IEnumerator FocusPlayerInput()
        {
            yield return new WaitForEndOfFrame();
            
            if (_playerPanel?.NameInputField != null)
            {
                _playerPanel.NameInputField.Select();
                _playerPanel.NameInputField.ActivateInputField();
                Debug.Log("[MultiplayerPanel] Player input focused");
            }
        }

        private void SetDefaultPlayerName()
        {
            if (_playerPanel?.NameInputField != null && string.IsNullOrEmpty(_playerPanel.NameInputField.text))
            {
                _playerPanel.NameInputField.text = "Introduce tu nombre";
            }
        }

        private void OnPlayerNameSubmit(string playerName)
        {
            Debug.Log($"[MultiplayerPanel] OnPlayerNameSubmit called with name: {playerName}");
            
            if (!string.IsNullOrEmpty(playerName.Trim()) && !string.IsNullOrEmpty(_currentMatchId))
            {
                Debug.Log("[MultiplayerPanel] Creating player in match...");
                CreatePlayerInMatch(playerName.Trim());
            }
            else
            {
                Debug.LogError($"[MultiplayerPanel] Invalid input - playerName: '{playerName}', matchId: '{_currentMatchId}'");
            }
        }

        private void CreatePlayerInMatch(string playerName)
        {
            if (_playerMatchPresenter == null)
            {
                Debug.LogError("[MultiplayerPanel] PlayerMatchPresenter not available");
                return;
            }

            Color playerColor = PlayerColorGenerator.GetRandomPlayerColor();
            string colorHex = PlayerColorGenerator.ColorToHex(playerColor);

            Debug.Log($"[MultiplayerPanel] Creating player - Name: {playerName}, Color: {colorHex}, MatchId: {_currentMatchId}");

            PlayerMatchData playerMatchData = new PlayerMatchData(
                "",
                playerName,
                _currentMatchId,
                0,
                colorHex
            );

            string returnedId = _playerMatchPresenter.JoinMatch(playerMatchData, OnPlayerCreated);
            
            Debug.Log($"[MultiplayerPanel] JoinMatch returned ID: {returnedId}");
            
            if (!string.IsNullOrEmpty(returnedId))
            {
                _localPlayerId = returnedId;
                _playerPanelManager?.SetLocalPlayerId(_localPlayerId);
                Debug.Log($"[MultiplayerPanel] Local player ID set: {_localPlayerId}");
            }
        }

        private void OnPlayerCreated(bool success)
        {
            Debug.Log($"[MultiplayerPanel] OnPlayerCreated - Success: {success}");
            
            if (success)
            {
                if (_playerPanel?.NameInputField != null)
                {
                    _playerPanel.NameInputField.onSubmit.RemoveListener(OnPlayerNameSubmit);
                }

                SetupLocalPlayerPanel();
                StartListeningForPlayers();
            }
            else
            {
                Debug.LogError("[MultiplayerPanel] Failed to join match - check Firebase connection and match existence");
                Debug.LogError("[MultiplayerPanel] Possible causes: Match doesn't exist, Firebase rules, network issues");
            }
        }

        private void SetupLocalPlayerPanel()
        {
            if (_playerPanelManager != null && _playerPanel != null)
            {
                string playerName = _playerPanel.NameInputField?.text ?? "Player";
                _playerPanelManager.SetupLocalPlayerPanel(_playerPanel, playerName);
                Debug.Log($"[MultiplayerPanel] Local player panel setup for: {playerName}");
            }
        }

        private void StartListeningForPlayers()
        {
            if (_playerMatchPresenter != null && !string.IsNullOrEmpty(_currentMatchId) && !_isListeningForPlayers)
            {
                _isListeningForPlayers = true;
                _playerMatchPresenter.ListenForMatchPlayersUpdates(_currentMatchId, OnPlayersUpdated);
                Debug.Log($"[MultiplayerPanel] Started listening for player updates on match: {_currentMatchId}");
            }
        }

        private void OnPlayersUpdated(List<PlayerMatchData> playersData)
        {
            Debug.Log($"[MultiplayerPanel] OnPlayersUpdated called with {playersData?.Count ?? 0} players");
            
            if (_playerPanelManager != null)
            {
                _playerPanelManager.UpdatePlayerPanels(playersData);
            }
        }

        private void StopListeningForPlayers()
        {
            if (_playerMatchPresenter != null && !string.IsNullOrEmpty(_currentMatchId) && _isListeningForPlayers)
            {
                _isListeningForPlayers = false;
                _playerMatchPresenter.StopListeningForMatchPlayers(_currentMatchId);
                Debug.Log($"[MultiplayerPanel] Stopped listening for player updates on match: {_currentMatchId}");
            }
        }

        private void OnDestroy()
        {
            Debug.Log("[MultiplayerPanel] OnDestroy called");
            StopListeningForPlayers();
            
            if (_playerPanel?.NameInputField != null)
            {
                _playerPanel.NameInputField.onSubmit.RemoveListener(OnPlayerNameSubmit);
            }

            _playerPanelManager?.ClearAllPlayers();
        }

        private void OnDisable()
        {
            Debug.Log("[MultiplayerPanel] OnDisable called");
            StopListeningForPlayers();
        }
    }
}