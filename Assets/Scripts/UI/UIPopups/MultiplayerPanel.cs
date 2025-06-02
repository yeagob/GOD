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
            InitializeURLHandler();
            InitializeNetworkServices();
            InitializePlayerPanelManager();
            
            _currentMatchId = matchId;
            _isClientMode = !string.IsNullOrEmpty(matchId);
            
            if (_playerPanel != null)
            {
                StartCoroutine(FocusPlayerInput());
                SetupPlayerInput();
                HandleMatchFlow();
            }
        }

        private void InitializeURLHandler()
        {
            _urlParameterHandler = new URLParameterHandler();
            
            if (_urlParameterHandler.IsMultiplayerMode && string.IsNullOrEmpty(_currentMatchId))
            {
                _currentMatchId = _urlParameterHandler.GetMatchParameter();
                _isClientMode = !string.IsNullOrEmpty(_currentMatchId);
            }
        }

        private void InitializeNetworkServices()
        {
            try
            {
                _playerMatchPresenter = NetworkInstaller.Resolve<IPlayerMatchPresenter>();
                _matchPresenter = NetworkInstaller.Resolve<IMatchPresenter>();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to resolve Network presenters: {ex.Message}");
            }
        }

        private void InitializePlayerPanelManager()
        {
            if (_playerPanelsParent == null)
            {
                _playerPanelsParent = _playerPanel?.transform.parent;
            }

            if (_playerPanelPrefab == null)
            {
                _playerPanelPrefab = _playerPanel?.gameObject;
            }

            if (_playerPanelsParent != null && _playerPanel != null && _playerPanelPrefab != null)
            {
                _playerPanelManager = new PlayerPanelManager(_playerPanelsParent, _playerPanel, _playerPanelPrefab);
            }
            else
            {
                Debug.LogError("PlayerPanelManager initialization failed - missing required references");
            }
        }

        private void HandleMatchFlow()
        {
            if (_isClientMode)
            {
                JoinExistingMatch();
            }
            else
            {
                Debug.Log($"Debug Multiplayer: Set as host. Creating match.");
                CreateNewMatch();
            }
        }

        private void CreateNewMatch()
        {
            if (_matchPresenter == null)
            {
                Debug.LogError("MatchPresenter is null, cannot create match");
                return;
            }

            _currentMatchId = System.Guid.NewGuid().ToString();
            
            string boardName = _urlParameterHandler.GetBoardParameter();
            string url = $"{Application.absoluteURL.Split('?')[0]}?board={boardName}&match={_currentMatchId}";
            
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
                Debug.LogError("MatchPresenter is null, cannot join match");
                return;
            }
            
            _matchPresenter.GetMatch(_currentMatchId, OnMatchRetrieved);
        }

        private void OnMatchCreated(bool success)
        {
            if (!success)
            {
                Debug.LogError("Failed to create match");
            }
        }

        private void OnMatchRetrieved(MatchData matchData)
        {
            if (matchData._id != null)
            {
                _currentMatchId = matchData._id;
            }
            else
            {
                Debug.LogError("Failed to retrieve match data - received null or invalid match data");
                Debug.LogError($"MatchData details: url={matchData._url}, state={matchData._state}");
            }
        }

        private void SetupPlayerInput()
        {
            if (_playerPanel?.NameInputField != null)
            {
                _playerPanel.NameInputField.onSubmit.AddListener(OnPlayerNameSubmit);
                SetDefaultPlayerName();
            }
            else
            {
                Debug.LogError("PlayerPanel or NameInputField is null");
            }
        }

        private IEnumerator FocusPlayerInput()
        {
            yield return new WaitForEndOfFrame();
            
            if (_playerPanel?.NameInputField != null)
            {
                _playerPanel.NameInputField.Select();
                _playerPanel.NameInputField.ActivateInputField();
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
            if (!string.IsNullOrEmpty(playerName.Trim()) && !string.IsNullOrEmpty(_currentMatchId))
            {
                CreatePlayerInMatch(playerName.Trim());
            }
            else
            {
                Debug.LogError($"Invalid input - playerName: '{playerName}', matchId: '{_currentMatchId}'");
            }
        }

        private void CreatePlayerInMatch(string playerName)
        {
            if (_playerMatchPresenter == null)
            {
                Debug.LogError("PlayerMatchPresenter not available");
                return;
            }

            Color playerColor = PlayerColorGenerator.GetRandomPlayerColor();
            string colorHex = PlayerColorGenerator.ColorToHex(playerColor);

            PlayerMatchData playerMatchData = new PlayerMatchData(
                "",
                playerName,
                _currentMatchId,
                0,
                colorHex
            );

            string returnedId = _playerMatchPresenter.JoinMatch(playerMatchData, OnPlayerCreated);
            
            if (!string.IsNullOrEmpty(returnedId))
            {
                _localPlayerId = returnedId;
                _playerPanelManager?.SetLocalPlayerId(_localPlayerId);
            }
        }

        private void OnPlayerCreated(bool success)
        {
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
                Debug.LogError("Failed to join match - check Firebase connection and match existence");
                Debug.LogError("Possible causes: Match doesn't exist, Firebase rules, network issues");
            }
        }

        private void SetupLocalPlayerPanel()
        {
            if (_playerPanelManager != null && _playerPanel != null)
            {
                string playerName = _playerPanel.NameInputField?.text ?? "Player";
                _playerPanelManager.SetupLocalPlayerPanel(_playerPanel, playerName);
            }
        }

        private void StartListeningForPlayers()
        {
            if (_playerMatchPresenter != null && !string.IsNullOrEmpty(_currentMatchId) && !_isListeningForPlayers)
            {
                _isListeningForPlayers = true;
                _playerMatchPresenter.ListenForMatchPlayersUpdates(_currentMatchId, OnPlayersUpdated);
            }
        }

        private void OnPlayersUpdated(List<PlayerMatchData> playersData)
        {
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
            }
        }

        private void OnDestroy()
        {
            StopListeningForPlayers();
            
            if (_playerPanel?.NameInputField != null)
            {
                _playerPanel.NameInputField.onSubmit.RemoveListener(OnPlayerNameSubmit);
            }

            _playerPanelManager?.ClearAllPlayers();
        }

        private void OnDisable()
        {
            StopListeningForPlayers();
        }
    }
}