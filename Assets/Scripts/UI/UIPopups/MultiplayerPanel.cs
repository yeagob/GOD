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
            }
        }

        private void InitializeNetworkServices()
        {
            try
            {
                _playerMatchPresenter = NetworkInstaller.Resolve<IPlayerMatchPresenter>();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to resolve PlayerMatchPresenter: {ex.Message}");
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
            if (!string.IsNullOrEmpty(playerName.Trim()))
            {
                CreatePlayerInFirebase(playerName.Trim());
            }
        }

        private void CreatePlayerInFirebase(string playerName)
        {
            string matchId = _urlParameterHandler.GetMatchParameter();
            
            if (string.IsNullOrEmpty(matchId))
            {
                Debug.LogError("No match ID found in URL parameters");
                return;
            }

            if (_playerMatchPresenter == null)
            {
                Debug.LogError("PlayerMatchPresenter not available");
                return;
            }

            PlayerMatchData playerMatchData = new PlayerMatchData(
                System.Guid.NewGuid().ToString(),
                playerName,
                matchId,
                0
            );

            _playerMatchPresenter.JoinMatch(playerMatchData, OnPlayerCreated);
        }

        private void OnPlayerCreated(bool success)
        {
            if (success)
            {
                Debug.Log("Player successfully created in Firebase");
            }
            else
            {
                Debug.LogError("Failed to create player in Firebase");
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
