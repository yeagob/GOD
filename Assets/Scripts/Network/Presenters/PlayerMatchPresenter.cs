using System;
using System.Collections.Generic;
using Network.Models;
using UnityEngine;

namespace Network.Presenters
{
    public class PlayerMatchPresenter : IPlayerMatchPresenter
    {
        private readonly IPlayerMatchModel _playerMatchModel;
        private readonly IMatchModel _matchModel;

        public PlayerMatchPresenter(
            IPlayerMatchModel playerMatchModel,
            IMatchModel matchModel)
        {
            _playerMatchModel = playerMatchModel;
            _matchModel = matchModel;
        }
        

        private void HandleMatchJoin(MatchData match, PlayerMatchData playerMatchData,
            Action<bool> callback, ref string playerMatchId)
        {
            Debug.Log($"[PlayerMatchPresenter] HandleMatchJoin - MatchID: {match._id}, PlayerName: {playerMatchData._name}");
            
            if (string.IsNullOrEmpty(match._id))
            {
                Debug.LogError("[PlayerMatchPresenter] Match ID is null or empty in HandleMatchJoin");
                callback?.Invoke(default);
                return;
            }

            Debug.Log($"[PlayerMatchPresenter] Creating player match with data: Name={playerMatchData._name}, Color={playerMatchData._color}");
            playerMatchId = _playerMatchModel.CreatePlayerMatch(playerMatchData, callback);
            Debug.Log($"[PlayerMatchPresenter] CreatePlayerMatch returned ID: {playerMatchId}");
        }

        public string JoinMatch(PlayerMatchData playerMatchData, Action<bool> callback = null)
        {
            Debug.Log($"[PlayerMatchPresenter] JoinMatch called - PlayerName: {playerMatchData._name}, MatchID: {playerMatchData._matchId}");
            
            string playerMatchId = null;

            _matchModel.GetMatch(playerMatchData._matchId,
                match => {
                    Debug.Log($"[PlayerMatchPresenter] GetMatch callback - Retrieved match ID: {match._id}");
                    HandleMatchJoin(match, playerMatchData, callback, ref playerMatchId);
                });

            return playerMatchId;
        }

        public void UpdateScore(string playerMatchId, int scoreChange, Action<bool> callback = null)
        {
            Debug.Log($"[PlayerMatchPresenter] UpdateScore called - PlayerID: {playerMatchId}, ScoreChange: {scoreChange}");
            
            _playerMatchModel.GetPlayerMatch(playerMatchId, player => {
                if (string.IsNullOrEmpty(player._id))
                {
                    Debug.LogError($"[PlayerMatchPresenter] Player not found for ID: {playerMatchId}");
                    callback?.Invoke(false);
                    return;
                }
                
                int newScore = player._tile + scoreChange;
                Debug.Log($"[PlayerMatchPresenter] Updating score from {player._tile} to {newScore}");
                _playerMatchModel.UpdatePlayerScore(playerMatchId, newScore, callback);
            });
        }

        public void GetPlayerInfo(string playerMatchId, Action<PlayerMatchData> callback)
        {
            Debug.Log($"[PlayerMatchPresenter] GetPlayerInfo called for ID: {playerMatchId}");
            _playerMatchModel.GetPlayerMatch(playerMatchId, callback);
        }

        public void GetMatchPlayers(string matchId, Action<List<PlayerMatchData>> callback)
        {
            Debug.Log($"[PlayerMatchPresenter] GetMatchPlayers called for MatchID: {matchId}");
            _playerMatchModel.GetPlayersByMatch(matchId, players => {
                Debug.Log($"[PlayerMatchPresenter] GetPlayersByMatch returned {players?.Count ?? 0} players");
                
                if (players != null && players.Count > 0)
                {
                    for (int i = 0; i < players.Count; i++)
                    {
                        var player = players[i];
                        Debug.Log($"[PlayerMatchPresenter] Player {i}: ID={player._id}, Name={player._name}, Color={player._color}");
                    }
                }
                
                callback?.Invoke(players);
            });
        }

        public void ListenForPlayerUpdates(string playerMatchId, Action<PlayerMatchData> callback)
        {
            Debug.Log($"[PlayerMatchPresenter] ListenForPlayerUpdates called for PlayerID: {playerMatchId}");
            _playerMatchModel.ListenForPlayerMatchChanges(playerMatchId, callback);
        }

        public void ListenForMatchPlayersUpdates(string matchId, Action<List<PlayerMatchData>> callback)
        {
            Debug.Log($"[PlayerMatchPresenter] ListenForMatchPlayersUpdates called for MatchID: {matchId}");
            _playerMatchModel.ListenForMatchPlayersChanges(matchId, callback);
        }

        public void StopListeningForPlayer(string playerMatchId)
        {
            Debug.Log($"[PlayerMatchPresenter] StopListeningForPlayer called for PlayerID: {playerMatchId}");
            _playerMatchModel.StopListeningForPlayerMatch(playerMatchId);
        }

        public void StopListeningForMatchPlayers(string matchId)
        {
            Debug.Log($"[PlayerMatchPresenter] StopListeningForMatchPlayers called for MatchID: {matchId}");
            _playerMatchModel.StopListeningForMatchPlayers(matchId);
        }
    }
}