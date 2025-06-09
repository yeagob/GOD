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
            
            Debug.Log($"[PlayerPanelManager] Initialized - Parent: {_playerPanelsParent?.name}, LocalPanel: {_localPlayerPanel?.name}, Prefab: {_playerPanelPrefab?.name}");
        }

        public void SetupLocalPlayerPanel(PlayerPanel playerPanel, string playerName)
        {
            Debug.Log($"[PlayerPanelManager] SetupLocalPlayerPanel called for: {playerName}");
            
            if (playerPanel != null)
            {
                Color playerColor = PlayerColorGenerator.GetRandomPlayerColor();
                playerPanel.SetupAsLocalPlayer(playerName, playerColor);
                Debug.Log($"[PlayerPanelManager] Local player panel configured with color: {PlayerColorGenerator.ColorToHex(playerColor)}");
            }
            else
            {
                Debug.LogError("[PlayerPanelManager] PlayerPanel is null in SetupLocalPlayerPanel");
            }
        }

        public void UpdatePlayerPanels(List<PlayerMatchData> playersData)
        {
            Debug.Log($"[PlayerPanelManager] UpdatePlayerPanels called with {playersData?.Count ?? 0} players");
            
            if (playersData == null || playersData.Count == 0)
            {
                Debug.LogWarning("[PlayerPanelManager] No players data to update");
                return;
            }

            List<string> currentPlayerIds = new List<string>();

            for (int i = 0; i < playersData.Count; i++)
            {
                PlayerMatchData playerData = playersData[i];
                Debug.Log($"[PlayerPanelManager] Processing player {i}: ID={playerData._id}, Name={playerData._name}, LocalPlayerId={_localPlayerId}");
                
                currentPlayerIds.Add(playerData._id);

                if (playerData._id == _localPlayerId)
                {
                    Debug.Log($"[PlayerPanelManager] Updating local player: {playerData._name}");
                    UpdateLocalPlayer(playerData);
                }
                else
                {
                    Debug.Log($"[PlayerPanelManager] Updating/Creating remote player: {playerData._name}");
                    UpdateRemotePlayer(playerData);
                }
            }

            RemoveDisconnectedPlayers(currentPlayerIds);
        }

        public void SetLocalPlayerId(string playerId)
        {
            _localPlayerId = playerId;
            Debug.Log($"[PlayerPanelManager] Local player ID set to: {_localPlayerId}");
        }

        public void ClearAllPlayers()
        {
            Debug.Log($"[PlayerPanelManager] Clearing {_remotePanels.Count} remote players");
            
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
            Debug.Log($"[PlayerPanelManager] UpdateLocalPlayer called for: {playerData._name}");
            
            if (_localPlayerPanel != null && !string.IsNullOrEmpty(playerData._color))
            {
                Color playerColor = PlayerColorGenerator.HexToColor(playerData._color);
                _localPlayerPanel.UpdatePlayerInfo(playerData._name, playerColor);
                Debug.Log($"[PlayerPanelManager] Local player updated with color: {playerData._color}");
            }
            else
            {
                Debug.LogWarning($"[PlayerPanelManager] Cannot update local player - Panel: {_localPlayerPanel}, Color: {playerData._color}");
            }
        }

        private void UpdateRemotePlayer(PlayerMatchData playerData)
        {
            Debug.Log($"[PlayerPanelManager] UpdateRemotePlayer called for: {playerData._name} (ID: {playerData._id})");
            
            if (!_remotePanels.ContainsKey(playerData._id))
            {
                Debug.Log($"[PlayerPanelManager] Creating new remote player panel for: {playerData._name}");
                CreateRemotePlayerPanel(playerData);
            }
            else
            {
                Debug.Log($"[PlayerPanelManager] Updating existing remote player panel for: {playerData._name}");
                UpdateExistingRemotePlayer(playerData);
            }
        }

        private void CreateRemotePlayerPanel(PlayerMatchData playerData)
        {
            Debug.Log($"[PlayerPanelManager] CreateRemotePlayerPanel for: {playerData._name}");
            
            if (_playerPanelPrefab == null || _playerPanelsParent == null)
            {
                Debug.LogError($"[PlayerPanelManager] Cannot create panel - Prefab: {_playerPanelPrefab}, Parent: {_playerPanelsParent}");
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
                
                Debug.Log($"[PlayerPanelManager] Created remote panel for {playerData._name} with color {playerData._color}. Total remote panels: {_remotePanels.Count}");
            }
            else
            {
                Debug.LogError($"[PlayerPanelManager] Created GameObject does not have PlayerPanel component");
                Object.Destroy(newPanelObj);
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
                Debug.Log($"[PlayerPanelManager] Updated existing remote panel for {playerData._name}");
            }
            else
            {
                Debug.LogWarning($"[PlayerPanelManager] Could not find remote panel for player ID: {playerData._id}");
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

            if (playersToRemove.Count > 0)
            {
                Debug.Log($"[PlayerPanelManager] Removing {playersToRemove.Count} disconnected players");
                
                foreach (string playerId in playersToRemove)
                {
                    if (_remotePanels.TryGetValue(playerId, out PlayerPanel panel) && panel != null)
                    {
                        Debug.Log($"[PlayerPanelManager] Removing panel for disconnected player: {playerId}");
                        Object.Destroy(panel.gameObject);
                    }
                    _remotePanels.Remove(playerId);
                }
            }
        }
    }
}