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

        private void Awake()
        {
            _urlParameterHandler = new URLParameterHandler();
            InitializeNetworkServices();
        }

        private void OnEnable()
        {
            if (_playerPanel != null)
            {
                StartCoroutine(FocusPlayerInput());
                SetupPlayerInput();
                HandleMatchFlow();
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

        private void HandleMatchFlow()
        {
            _currentMatchId = _urlParameterHandler.GetMatchParameter();
            
            if (!string.IsNullOrEmpty(_currentMatchId))
            {
                JoinExistingMatch();
            }
            else
            {
                CreateNewMatch();
            }
        }

        private void CreateNewMatch()
        {
            if (_matchPresenter == null) return;

            MatchData newMatch = new MatchData(
                System.Guid.NewGuid().ToString(),
                System.DateTime.UtcNow,
                System.DateTime.UtcNow,
                MatchStatus.Waiting,
                4,
                "duck_game"
            );

            _matchPresenter.CreateMatch(newMatch, OnMatchCreated);
        }

        private void JoinExistingMatch()
        {
            if (_matchPresenter == null) return;
            
            _matchPresenter.GetMatch(_currentMatchId, OnMatchRetrieved);
        }

        private void OnMatchCreated(bool success)
        {
            if (success)
            {
                Debug.Log("Match created successfully");
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
                Debug.LogError("Failed to retrieve match data");
            }
        }

        private void SetupPlayerInput()
        {
            if (_playerPanel?.NameInputField != null)
            {
                _playerPanel.NameInputField.onSubmit.AddListener(OnPlayerNameSubmit);
                SetDefaultPlayerName();
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
                _playerPanel.NameInputField.text = "Jugador 1";
            }
        }

        private void OnPlayerNameSubmit(string playerName)
        {
            if (!string.IsNullOrEmpty(playerName.Trim()) && !string.IsNullOrEmpty(_currentMatchId))
            {
                CreatePlayerInMatch(playerName.Trim());
            }
        }

        private void CreatePlayerInMatch(string playerName)
        {
            if (_playerMatchPresenter == null)
            {
                Debug.LogError("PlayerMatchPresenter not available");
                return;
            }

            PlayerMatchData playerMatchData = new PlayerMatchData(
                System.Guid.NewGuid().ToString(),
                playerName,
                _currentMatchId,
                0
            );

            _playerMatchPresenter.JoinMatch(playerMatchData, OnPlayerCreated);
        }

        private void OnPlayerCreated(bool success)
        {
            if (success)
            {
                Debug.Log("Player successfully joined match");
            }
            else
            {
                Debug.LogError("Failed to join match");
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
