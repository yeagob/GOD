using System.Collections.Generic;
using UnityEngine;
using Network;

namespace UI.UIPopups
{
    public interface IPlayerPanelManager
    {
        void SetupLocalPlayerPanel(PlayerPanel playerPanel, string playerName);
        void UpdatePlayerPanels(List<PlayerMatchData> playersData);
        void SetLocalPlayerId(string playerId);
        void ClearAllPlayers();
    }

    public class PlayerPanelManager : IPlayerPanelManager
    {
        private readonly Transform _playerPanelsParent;
        private readonly PlayerPanel _localPlayerPanel;
        private readonly GameObject _playerPanelPrefab;
        
        private readonly Dictionary<string, PlayerPanel> _remotePanels = new();
        private string _localPlayerId;

        public PlayerPanelManager(Transform playerPanelsParent, PlayerPanel localPlayerPanel, GameObject playerPanelPrefab)
        {
            _playerPanelsParent = playerPanelsParent;
            _localPlayerPanel = localPlayerPanel;
            _playerPanelPrefab = playerPanelPrefab;
        }

        public void SetupLocalPlayerPanel(PlayerPanel playerPanel, string playerName)
        {
            if (playerPanel != null)
            {
                Color playerColor = PlayerColorGenerator.GetRandomPlayerColor();
                playerPanel.SetupAsLocalPlayer(playerName, playerColor);
            }
        }

        public void UpdatePlayerPanels(List<PlayerMatchData> playersData)
        {
            if (playersData == null || playersData.Count == 0)
            {
                return;
            }

            List<string> currentPlayerIds = new List<string>();

            foreach (PlayerMatchData playerData in playersData)
            {
                currentPlayerIds.Add(playerData._id);

                if (playerData._id == _localPlayerId)
                {
                    UpdateLocalPlayer(playerData);
                }
                else
                {
                    UpdateRemotePlayer(playerData);
                }
            }

            RemoveDisconnectedPlayers(currentPlayerIds);
        }

        public void SetLocalPlayerId(string playerId)
        {
            _localPlayerId = playerId;
        }

        public void ClearAllPlayers()
        {
            foreach (var panel in _remotePanels.Values)
            {
                if (panel != null && panel.gameObject != null)
                {
                    Object.Destroy(panel.gameObject);
                }
            }
            _remotePanels.Clear();
        }

        private void UpdateLocalPlayer(PlayerMatchData playerData)
        {
            if (_localPlayerPanel != null && !string.IsNullOrEmpty(playerData._color))
            {
                Color playerColor = PlayerColorGenerator.HexToColor(playerData._color);
                _localPlayerPanel.UpdatePlayerInfo(playerData._name, playerColor);
            }
        }

        private void UpdateRemotePlayer(PlayerMatchData playerData)
        {
            if (!_remotePanels.ContainsKey(playerData._id))
            {
                CreateRemotePlayerPanel(playerData);
            }
            else
            {
                UpdateExistingRemotePlayer(playerData);
            }
        }

        private void CreateRemotePlayerPanel(PlayerMatchData playerData)
        {
            if (_playerPanelPrefab == null || _playerPanelsParent == null)
            {
                Debug.LogError("PlayerPanelPrefab or PlayerPanelsParent is null");
                return;
            }

            GameObject newPanelObj = Object.Instantiate(_playerPanelPrefab, _playerPanelsParent);
            PlayerPanel newPanel = newPanelObj.GetComponent<PlayerPanel>();
            
            if (newPanel != null)
            {
                Color playerColor = string.IsNullOrEmpty(playerData._color) 
                    ? PlayerColorGenerator.GetRandomPlayerColor() 
                    : PlayerColorGenerator.HexToColor(playerData._color);
                    
                newPanel.SetupAsRemotePlayer(playerData._name, playerColor);
                _remotePanels[playerData._id] = newPanel;
            }
        }

        private void UpdateExistingRemotePlayer(PlayerMatchData playerData)
        {
            if (_remotePanels.TryGetValue(playerData._id, out PlayerPanel panel) && panel != null)
            {
                Color playerColor = string.IsNullOrEmpty(playerData._color) 
                    ? PlayerColorGenerator.GetRandomPlayerColor() 
                    : PlayerColorGenerator.HexToColor(playerData._color);
                    
                panel.UpdatePlayerInfo(playerData._name, playerColor);
            }
        }

        private void RemoveDisconnectedPlayers(List<string> currentPlayerIds)
        {
            List<string> playersToRemove = new List<string>();
            
            foreach (string playerId in _remotePanels.Keys)
            {
                if (!currentPlayerIds.Contains(playerId))
                {
                    playersToRemove.Add(playerId);
                }
            }

            foreach (string playerId in playersToRemove)
            {
                if (_remotePanels.TryGetValue(playerId, out PlayerPanel panel) && panel != null)
                {
                    Object.Destroy(panel.gameObject);
                }
                _remotePanels.Remove(playerId);
            }
        }
    }
}