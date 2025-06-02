using System;
using System.Collections.Generic;
using UnityEngine;
using Network.Services;

namespace Network.Repositories
{
    public class PlayerMatchRepository : IPlayerMatchRepository
    {
        private readonly IFirebaseService _firebaseService;
        private const string PATH = "player_matches";
        private const string MATCH_INDEX = "match_index";

        public PlayerMatchRepository(IFirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
        }

        public void CreatePlayerMatch(PlayerMatchData playerMatchData, Action<bool> callback = null)
        {
            Debug.Log($"[PlayerMatchRepository] CreatePlayerMatch called - ID: {playerMatchData._id}, Name: {playerMatchData._name}, MatchId: {playerMatchData._matchId}, Color: {playerMatchData._color}");
            
            string path = $"{PATH}/{playerMatchData._id}";
            _firebaseService.SetData(path, playerMatchData, result => {
                Debug.Log($"[PlayerMatchRepository] SetData result for player: {result}");
                
                if (result)
                {
                    string indexPath = $"{MATCH_INDEX}/{playerMatchData._matchId}/{playerMatchData._id}";
                    Debug.Log($"[PlayerMatchRepository] Creating index at: {indexPath}");
                    
                    _firebaseService.SetData(indexPath, true, indexResult => {
                        Debug.Log($"[PlayerMatchRepository] Index creation result: {indexResult}");
                        callback?.Invoke(indexResult);
                    });
                }
                else
                {
                    callback?.Invoke(false);
                }
            });
        }

        public void UpdatePlayerMatch(PlayerMatchData playerMatchData, Action<bool> callback = null)
        {
            Debug.Log($"[PlayerMatchRepository] UpdatePlayerMatch called for: {playerMatchData._id}");
            CreatePlayerMatch(playerMatchData, callback);
        }

        public void GetPlayerMatch(string playerMatchId, Action<PlayerMatchData> callback)
        {
            Debug.Log($"[PlayerMatchRepository] GetPlayerMatch called for: {playerMatchId}");
            string path = $"{PATH}/{playerMatchId}";
            _firebaseService.GetData<PlayerMatchData>(path, playerData => {
                Debug.Log($"[PlayerMatchRepository] GetPlayerMatch result - Player: {playerData._name} (ID: {playerData._id})");
                callback?.Invoke(playerData);
            });
        }

        public void GetPlayerMatchesByMatchId(string matchId, Action<List<PlayerMatchData>> callback)
        {
            Debug.Log($"[PlayerMatchRepository] GetPlayerMatchesByMatchId called for MatchId: {matchId}");
            
            string indexPath = $"{MATCH_INDEX}/{matchId}";
            Debug.Log($"[PlayerMatchRepository] Querying index path: {indexPath}");
            
            _firebaseService.GetData<Dictionary<string, object>>(indexPath, indexData => {
                Debug.Log($"[PlayerMatchRepository] Index query result - Found {indexData?.Count ?? 0} player references");
                
                if (indexData == null || indexData.Count == 0)
                {
                    Debug.LogWarning($"[PlayerMatchRepository] No players found in match index for MatchId: {matchId}");
                    callback?.Invoke(new List<PlayerMatchData>());
                    return;
                }

                List<PlayerMatchData> players = new List<PlayerMatchData>();
                int completedRequests = 0;
                int totalRequests = indexData.Count;

                foreach (string playerId in indexData.Keys)
                {
                    Debug.Log($"[PlayerMatchRepository] Fetching player data for ID: {playerId}");
                    
                    GetPlayerMatch(playerId, playerData => {
                        completedRequests++;
                        
                        if (!string.IsNullOrEmpty(playerData._id))
                        {
                            players.Add(playerData);
                            Debug.Log($"[PlayerMatchRepository] Added player: {playerData._name} (ID: {playerData._id}) - {completedRequests}/{totalRequests}");
                        }
                        else
                        {
                            Debug.LogWarning($"[PlayerMatchRepository] Player data is empty for ID: {playerId}");
                        }

                        if (completedRequests >= totalRequests)
                        {
                            Debug.Log($"[PlayerMatchRepository] All players fetched - Total: {players.Count}");
                            callback?.Invoke(players);
                        }
                    });
                }
            });
        }

        public void ListenForPlayerMatchChanges(string playerMatchId, Action<PlayerMatchData> callback)
        {
            Debug.Log($"[PlayerMatchRepository] ListenForPlayerMatchChanges called for: {playerMatchId}");
            string path = $"{PATH}/{playerMatchId}";
            _firebaseService.ListenForChanges(path, callback);
        }

        public void ListenForMatchPlayerChanges(string matchId, Action<List<PlayerMatchData>> callback)
        {
            Debug.Log($"[PlayerMatchRepository] ListenForMatchPlayerChanges called for MatchId: {matchId}");
            string path = $"{MATCH_INDEX}/{matchId}";
            
            _firebaseService.ListenForChanges<Dictionary<string, object>>(path, indexData => {
                Debug.Log($"[PlayerMatchRepository] Match players listener triggered - Found {indexData?.Count ?? 0} player references");
                
                if (indexData == null || indexData.Count == 0)
                {
                    callback?.Invoke(new List<PlayerMatchData>());
                    return;
                }

                List<PlayerMatchData> players = new List<PlayerMatchData>();
                int completedRequests = 0;
                int totalRequests = indexData.Count;

                foreach (string playerId in indexData.Keys)
                {
                    GetPlayerMatch(playerId, playerData => {
                        completedRequests++;
                        
                        if (!string.IsNullOrEmpty(playerData._id))
                        {
                            players.Add(playerData);
                        }

                        if (completedRequests >= totalRequests)
                        {
                            callback?.Invoke(players);
                        }
                    });
                }
            });
        }

        public void StopListeningForPlayerMatch(string playerMatchId)
        {
            Debug.Log($"[PlayerMatchRepository] StopListeningForPlayerMatch called for: {playerMatchId}");
            string path = $"{PATH}/{playerMatchId}";
            _firebaseService.StopListening(path);
        }

        public void StopListeningForMatchPlayers(string matchId)
        {
            Debug.Log($"[PlayerMatchRepository] StopListeningForMatchPlayers called for: {matchId}");
            string path = $"{MATCH_INDEX}/{matchId}";
            _firebaseService.StopListening(path);
        }

        public string GeneratePlayerMatchId()
        {
            string generatedId = _firebaseService.GenerateId(PATH);
            Debug.Log($"[PlayerMatchRepository] Generated PlayerMatchId: {generatedId}");
            return generatedId;
        }
    }
}