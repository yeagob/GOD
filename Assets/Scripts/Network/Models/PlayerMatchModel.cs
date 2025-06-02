using System;
using System.Collections.Generic;
using Network.Repositories;
using UnityEngine;

namespace Network.Models
{
    public class PlayerMatchModel : IPlayerMatchModel
    {
        private readonly IPlayerMatchRepository _playerMatchRepository;

        public PlayerMatchModel(IPlayerMatchRepository playerMatchRepository)
        {
            _playerMatchRepository = playerMatchRepository;
        }

        public string CreatePlayerMatch(string name, string matchId, Action<PlayerMatchData> callback = null)
        {
            throw new NotImplementedException();
        }

        public string CreatePlayerMatch(string name, string matchId, Action<bool> callback = null)
        {
            Debug.Log($"[PlayerMatchModel] CreatePlayerMatch (basic) called - Name: {name}, MatchId: {matchId}");
            
            string playerMatchId = _playerMatchRepository.GeneratePlayerMatchId();
            PlayerMatchData playerMatchData = new PlayerMatchData(playerMatchId, name, matchId, 0);
            
            Debug.Log($"[PlayerMatchModel] Generated PlayerMatchId: {playerMatchId}");
            
            _playerMatchRepository.CreatePlayerMatch(playerMatchData, success => {
                Debug.Log($"[PlayerMatchModel] CreatePlayerMatch repository callback - Success: {success}");
                if (success)
                {
                    callback?.Invoke(true);
                }
                else
                {
                    callback?.Invoke(false);
                }
            });

            return playerMatchId;
        }

        public string CreatePlayerMatch(PlayerMatchData playerMatchData, Action<bool> callback = null)
        {
            Debug.Log($"[PlayerMatchModel] CreatePlayerMatch (full data) called - Name: {playerMatchData._name}, MatchId: {playerMatchData._matchId}, Color: {playerMatchData._color}");
            
            string playerMatchId = _playerMatchRepository.GeneratePlayerMatchId();
            
            PlayerMatchData completePlayerData = new PlayerMatchData(
                playerMatchId,
                playerMatchData._name,
                playerMatchData._matchId,
                playerMatchData._tile,
                playerMatchData._color
            );
            
            Debug.Log($"[PlayerMatchModel] Generated PlayerMatchId: {playerMatchId} with complete data including color: {playerMatchData._color}");
            
            _playerMatchRepository.CreatePlayerMatch(completePlayerData, success => {
                Debug.Log($"[PlayerMatchModel] CreatePlayerMatch (full data) repository callback - Success: {success}");
                if (success)
                {
                    callback?.Invoke(true);
                }
                else
                {
                    callback?.Invoke(false);
                }
            });

            return playerMatchId;
        }

        public void UpdatePlayerScore(string playerMatchId, int newScore, Action<bool> callback = null)
        {
            Debug.Log($"[PlayerMatchModel] UpdatePlayerScore called - PlayerMatchId: {playerMatchId}, NewScore: {newScore}");
            
            _playerMatchRepository.GetPlayerMatch(playerMatchId, existingPlayer => {
                Debug.Log($"[PlayerMatchModel] GetPlayerMatch callback - Found player: {existingPlayer._name} (ID: {existingPlayer._id})");
                
                if (string.IsNullOrEmpty(existingPlayer._id))
                {
                    Debug.LogError($"[PlayerMatchModel] Player not found for ID: {playerMatchId}");
                    callback?.Invoke(false);
                    return;
                }
                
                PlayerMatchData updatedPlayer = new PlayerMatchData(
                    existingPlayer._id,
                    existingPlayer._name,
                    existingPlayer._matchId,
                    newScore,
                    existingPlayer._color
                );
                
                Debug.Log($"[PlayerMatchModel] Updating player score from {existingPlayer._tile} to {newScore}, preserving color: {existingPlayer._color}");
                _playerMatchRepository.UpdatePlayerMatch(updatedPlayer, callback);
            });
        }

        public void GetPlayerMatch(string playerMatchId, Action<PlayerMatchData> callback)
        {
            Debug.Log($"[PlayerMatchModel] GetPlayerMatch called for ID: {playerMatchId}");
            _playerMatchRepository.GetPlayerMatch(playerMatchId, playerData => {
                Debug.Log($"[PlayerMatchModel] GetPlayerMatch callback - Player: {playerData._name} (ID: {playerData._id}), Color: {playerData._color}");
                callback?.Invoke(playerData);
            });
        }

        public void GetPlayersByMatch(string matchId, Action<List<PlayerMatchData>> callback)
        {
            Debug.Log($"[PlayerMatchModel] GetPlayersByMatch called for MatchId: {matchId}");
            
            _playerMatchRepository.GetPlayerMatchesByMatchId(matchId, players => {
                Debug.Log($"[PlayerMatchModel] GetPlayerMatchesByMatchId repository callback - Received {players?.Count ?? 0} players");
                
                if (players != null && players.Count > 0)
                {
                    Debug.Log($"[PlayerMatchModel] Players found in match {matchId}:");
                    for (int i = 0; i < players.Count; i++)
                    {
                        var player = players[i];
                        Debug.Log($"[PlayerMatchModel]   Player {i}: ID={player._id}, Name={player._name}, Color={player._color}, MatchId={player._matchId}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[PlayerMatchModel] No players found for match {matchId} or received null/empty list");
                }
                
                callback?.Invoke(players);
            });
        }

        public void ListenForPlayerMatchChanges(string playerMatchId, Action<PlayerMatchData> callback)
        {
            Debug.Log($"[PlayerMatchModel] ListenForPlayerMatchChanges called for PlayerMatchId: {playerMatchId}");
            _playerMatchRepository.ListenForPlayerMatchChanges(playerMatchId, callback);
        }

        public void ListenForMatchPlayersChanges(string matchId, Action<List<PlayerMatchData>> callback)
        {
            Debug.Log($"[PlayerMatchModel] ListenForMatchPlayersChanges called for MatchId: {matchId}");
            _playerMatchRepository.ListenForMatchPlayerChanges(matchId, callback);
        }

        public void StopListeningForPlayerMatch(string playerMatchId)
        {
            Debug.Log($"[PlayerMatchModel] StopListeningForPlayerMatch called for PlayerMatchId: {playerMatchId}");
            _playerMatchRepository.StopListeningForPlayerMatch(playerMatchId);
        }

        public void StopListeningForMatchPlayers(string matchId)
        {
            Debug.Log($"[PlayerMatchModel] StopListeningForMatchPlayers called for MatchId: {matchId}");
            _playerMatchRepository.StopListeningForMatchPlayers(matchId);
        }
    }
}