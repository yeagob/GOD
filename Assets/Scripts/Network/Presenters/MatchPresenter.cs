using System;
using System.Collections.Generic;
using Network.Infrastructure;
using Network.Models;

namespace Network.Presenters
{
    public class MatchPresenter : IMatchPresenter
    {
        private readonly IMatchModel _matchModel;
        private readonly IPlayerMatchModel _playerMatchModel;
        private readonly IGameEventModel _gameEventModel;
        
        public const int MATCH_STATE_WAITING = 0;
        public const int MATCH_STATE_PLAYING = 1;
        public const int MATCH_STATE_FINISHED = 2;

        public MatchPresenter(
            IMatchModel matchModel, 
            IPlayerMatchModel playerMatchModel, 
            IGameEventModel gameEventModel)
        {
            _matchModel = matchModel;
            _playerMatchModel = playerMatchModel;
            _gameEventModel = gameEventModel;
        }

        public void CreateMatch(MatchData matchData, Action<bool> callback = null)
        {
            _matchModel.CreateMatch(matchData._id, matchData._url, (MatchState)matchData._state, result => {
                callback?.Invoke(!string.IsNullOrEmpty(result._id));
            });
        }

        public void GetMatch(string matchId, Action<MatchData> callback)
        {
            _matchModel.GetMatch(matchId, callback);
        }

        public void CreateNewMatch(string url, Action<MatchData> callback = null)
        {
            _matchModel.CreateMatch(url, MATCH_STATE_WAITING, callback);
        }

        public void JoinMatch(string matchId, string playerName, Action<PlayerMatchData> callback = null)
        {
            _matchModel.GetMatch(matchId, match => {
                if (string.IsNullOrEmpty(match._id) || match._state != MATCH_STATE_WAITING)
                {
                    callback?.Invoke(default);
                    return;
                }
                
                _playerMatchModel.CreatePlayerMatch(playerName, matchId, callback);
            });
        }

        public void StartMatch(string matchId, Action<bool> callback = null)
        {
            _matchModel.UpdateMatchState(matchId, MatchState.PlayGame, callback);
        }

        public void EndMatch(string matchId, Action<bool> callback = null)
        {
            _matchModel.UpdateMatchState(matchId, MatchState.EndGame, callback);
        }

        public void GetCurrentMatchState(string matchId, Action<MatchData, List<PlayerMatchData>, List<GameEventData>> callback)
        {
            _matchModel.GetMatch(matchId, match => {
                if (string.IsNullOrEmpty(match._id))
                {
                    callback?.Invoke(default, null, null);
                    return;
                }
                
                _playerMatchModel.GetPlayersByMatch(matchId, players => {
                    _gameEventModel.GetEventsByMatch(matchId, events => {
                        callback?.Invoke(match, players, events);
                    });
                });
            });
        }

        public void ListenForMatchUpdates(
            string matchId, 
            Action<MatchData> onMatchChanged, 
            Action<List<PlayerMatchData>> onPlayersChanged, 
            Action<List<GameEventData>> onEventsChanged)
        {
            _matchModel.ListenForMatchChanges(matchId, onMatchChanged);
            _playerMatchModel.ListenForMatchPlayersChanges(matchId, onPlayersChanged);
            _gameEventModel.ListenForMatchEvents(matchId, onEventsChanged);
        }

        public void StopListeningForMatchUpdates(string matchId)
        {
            _matchModel.StopListeningForMatch(matchId);
            _playerMatchModel.StopListeningForMatchPlayers(matchId);
            _gameEventModel.StopListeningForMatchEvents(matchId);
        }
    }
}
