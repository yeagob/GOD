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

        public void JoinMatch(string matchId, string playerName, Action<PlayerMatchData> callback = null)
        {
            _matchModel.GetMatch(matchId, match => {
                if (string.IsNullOrEmpty(match._id) || match._state != MatchPresenter.MATCH_STATE_WAITING)
                {
                    callback?.Invoke(default);
                    return;
                }
                
                _playerMatchModel.CreatePlayerMatch(playerName, matchId, callback);
            });
        }

        public void JoinMatch(PlayerMatchData playerMatchData, Action<bool> callback = null)
        {
            _matchModel.GetMatch(playerMatchData._matchId, match => {
                if (string.IsNullOrEmpty(match._id))
                {
                    callback?.Invoke(false);
                    return;
                }
                
                _playerMatchModel.CreatePlayerMatch(playerMatchData._name, playerMatchData._matchId, result => {
                    callback?.Invoke(!string.IsNullOrEmpty(result._id));
                });
            });
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
