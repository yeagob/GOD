using System.Collections;
using TMPro;
using UnityEngine;

namespace UI.UIPopups
{
    public class MultiplayerPanel : MonoBehaviour
    {
        [SerializeField] private PlayerPanel _playerPanel;
        
        private URLParameterHandler _urlParameterHandler;
        private bool _isInMatchFlow;

        private void Awake()
        {
            _urlParameterHandler = new URLParameterHandler();
        }

        private void Start()
        {
            CheckMatchFlow();
        }

        private void OnEnable()
        {
            if (_isInMatchFlow && _playerPanel != null)
            {
                StartCoroutine(FocusPlayerInput());
            }
        }

        private void CheckMatchFlow()
        {
            _isInMatchFlow = _urlParameterHandler.HasMatchParameter;
            
            if (_isInMatchFlow)
            {
                SetupMatchFlow();
            }
        }

        private void SetupMatchFlow()
        {
            if (_playerPanel?.NameInputField != null)
            {
                _playerPanel.NameInputField.onSubmit.AddListener(OnPlayerNameSubmit);
                StartCoroutine(FocusPlayerInput());
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

            Debug.Log($"Creating player '{playerName}' for match '{matchId}' using Network flow");
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
