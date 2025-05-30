using System;
using System.Collections;
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
        
        private URLParameterHandler _urlParameterHandler;
        private IPlayerMatchPresenter _playerMatchPresenter;
        private IMatchPresenter _matchPresenter;
        private string _currentMatchId;
        private string _localPlayerId;
        private bool _isClientMode;

        public void Initialize(string matchId = null)
        {
            InitializeURLHandler();
            InitializeNetworkServices();
            
            _currentMatchId = matchId;
            _isClientMode = !string.IsNullOrEmpty(matchId);
            
            Debug.Log($"MultiplayerPanel.Initialize: matchId={matchId}, isClient={_isClientMode}");
            
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
                Debug.Log("MultiplayerPanel: Network services initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to resolve Network presenters: {ex.Message}");
            }
        }

        private void HandleMatchFlow()
        {
            if (_isClientMode)
            {
                Debug.Log($"MultiplayerPanel: Client mode - joining match {_currentMatchId}");
                JoinExistingMatch();
            }
            else
            {
                Debug.Log("MultiplayerPanel: Host mode - creating new match");
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
            Debug.Log($"MultiplayerPanel: Generated new match ID: {_currentMatchId}");
            
            string boardName = _urlParameterHandler.GetBoardParameter();
            string url = $"{Application.absoluteURL.Split('?')[0]}?board={boardName}&match={_currentMatchId}";
            
            Debug.Log($"MultiplayerPanel: Creating match with URL: {url}");
            
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
            
            Debug.Log($"MultiplayerPanel: Attempting to join match: {_currentMatchId}");
            _matchPresenter.GetMatch(_currentMatchId, OnMatchRetrieved);
        }

        private void OnMatchCreated(bool success)
        {
            if (success)
            {
                Debug.Log($"Match created successfully with ID: {_currentMatchId}");
            }
            else
            {
                Debug.LogError("Failed to create match");
            }
        }

        private void OnMatchRetrieved(MatchData matchData)
        {
            if (matchData._id != null)
            {
                Debug.Log($"Joined existing match: {matchData._id}");
                _currentMatchId = matchData._id;
            }
            else
            {
                Debug.LogError("Failed to retrieve match data - received null or invalid match data");
                Debug.LogError($"MatchData details: id={matchData._id}, url={matchData._url}, state={matchData._state}");
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
            Debug.Log($"MultiplayerPanel: Player name submitted: '{playerName}', matchId: '{_currentMatchId}'");
            
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

            Debug.Log($"MultiplayerPanel: Creating player '{playerName}' in match '{_currentMatchId}'");

            PlayerMatchData playerMatchData = new PlayerMatchData(
                "",
                playerName,
                _currentMatchId,
                0
            );

            Debug.Log($"PlayerMatchData created: playerId='{playerMatchData._playerId}', name='{playerMatchData._playerName}', matchId='{playerMatchData._matchId}', score={playerMatchData._score}");

            _localPlayerId = _playerMatchPresenter.JoinMatch(playerMatchData, OnPlayerCreated);
            
            Debug.Log($"JoinMatch called, returned playerId: '{_localPlayerId}'");
        }

        private void OnPlayerCreated(bool success)
        {
            Debug.Log($"MultiplayerPanel: OnPlayerCreated callback - success: {success}");
            
            if (success)
            {
                Debug.Log($"Player successfully joined match with ID: {_localPlayerId}");
                if (_playerPanel?.NameInputField != null)
                {
                    _playerPanel.NameInputField.onSubmit.RemoveListener(OnPlayerNameSubmit);
                }
            }
            else
            {
                Debug.LogError("Failed to join match - check Firebase connection and match existence");
            }
        }

        private void OnDestroy()
        {
            if (_playerPanel?.NameInputField != null)
            {
                _playerPanel.NameInputField.onSubmit.RemoveListener(OnPlayerNameSubmit);
            }
        }
    }
}
