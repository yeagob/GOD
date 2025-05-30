using System;
using System.Collections.Generic;
using Network.Models;

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
        

        private void HandleMatchJoin(MatchData match, string matchId, string playerName,
            Action<bool> callback, ref string playerMatchId)
        {
            if (string.IsNullOrEmpty(match._id) )
            {
                callback?.Invoke(default);
                return;
            }

            playerMatchId = _playerMatchModel.CreatePlayerMatch(playerName, matchId, callback);
        }

        public string JoinMatch(PlayerMatchData playerMatchData, Action<bool> callback = null)
        {
            string playerMatchId = null;

            _matchModel.GetMatch(playerMatchData._matchId,
                match => HandleMatchJoin(match, playerMatchData._matchId, playerMatchData._name, callback, ref playerMatchId));

            return playerMatchId;
        }

        public void UpdateScore(string playerMatchId, int scoreChange, Action<bool> callback = null)
        {
            _playerMatchModel.GetPlayerMatch(playerMatchId, player => {
                if (string.IsNullOrEmpty(player._id))
                {
                    callback?.Invoke(false);
                    return;
                }
                
                int newScore = player._tile + scoreChange;
                _playerMatchModel.UpdatePlayerScore(playerMatchId, newScore, callback);
            });
        }

        public void GetPlayerInfo(string playerMatchId, Action<PlayerMatchData> callback)
        {
            _playerMatchModel.GetPlayerMatch(playerMatchId, callback);
        }

        public void GetMatchPlayers(string matchId, Action<List<PlayerMatchData>> callback)
        {
            _playerMatchModel.GetPlayersByMatch(matchId, callback);
        }

        public void ListenForPlayerUpdates(string playerMatchId, Action<PlayerMatchData> callback)
        {
            _playerMatchModel.ListenForPlayerMatchChanges(playerMatchId, callback);
        }

        public void ListenForMatchPlayersUpdates(string matchId, Action<List<PlayerMatchData>> callback)
        {
            _playerMatchModel.ListenForMatchPlayersChanges(matchId, callback);
        }

        public void StopListeningForPlayer(string playerMatchId)
        {
            _playerMatchModel.StopListeningForPlayerMatch(playerMatchId);
        }

        public void StopListeningForMatchPlayers(string matchId)
        {
            _playerMatchModel.StopListeningForMatchPlayers(matchId);
        }
    }
}
