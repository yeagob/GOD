using System;
using System.Collections.Generic;
using Network.Models;

namespace Network.Presenters
{
    public class GameEventPresenter : IGameEventPresenter
    {
        private readonly IGameEventModel _gameEventModel;
        private readonly IPlayerMatchModel _playerMatchModel;
        private readonly IMatchModel _matchModel;
        
        // Tipos de evento
        public const int EVENT_TYPE_MOVE = 0;
        public const int EVENT_TYPE_JUMP = 1;
        public const int EVENT_TYPE_QUESTION = 2;
        public const int EVENT_TYPE_SCORE_CHANGE = 3;
        public const int EVENT_TYPE_REVIEW = 4;

        public GameEventPresenter(
            IGameEventModel gameEventModel,
            IPlayerMatchModel playerMatchModel,
            IMatchModel matchModel)
        {
            _gameEventModel = gameEventModel;
            _playerMatchModel = playerMatchModel;
            _matchModel = matchModel;
        }

        public void RecordMovement(string playerId, string matchId, int targetTile, Action<GameEventData> callback = null)
        {
            _gameEventModel.CreateGameEvent(playerId, matchId, EVENT_TYPE_MOVE, targetTile, callback);
        }

        public void RecordJump(string playerId, string matchId, int targetTile, Action<GameEventData> callback = null)
        {
            _gameEventModel.CreateGameEvent(playerId, matchId, EVENT_TYPE_JUMP, targetTile, callback);
        }

        public void RecordQuestion(string playerId, string matchId, int targetTile, Action<GameEventData> callback = null)
        {
            _gameEventModel.CreateGameEvent(playerId, matchId, EVENT_TYPE_QUESTION, targetTile, callback);
        }

        public void RecordScoreChange(string playerId, string matchId, int targetTile, Action<GameEventData> callback = null)
        {
            _gameEventModel.CreateGameEvent(playerId, matchId, EVENT_TYPE_SCORE_CHANGE, targetTile, callback);
        }

        public void RecordReview(string playerId, string matchId, int targetTile, Action<GameEventData> callback = null)
        {
            _gameEventModel.CreateGameEvent(playerId, matchId, EVENT_TYPE_REVIEW, targetTile, callback);
        }

        public void GetMatchTimeline(string matchId, Action<List<GameEventData>> callback)
        {
            _gameEventModel.GetEventsByMatch(matchId, callback);
        }

        public void GetPlayerTimeline(string playerId, Action<List<GameEventData>> callback)
        {
            _gameEventModel.GetEventsByPlayer(playerId, callback);
        }

        public void ListenForMatchEvents(string matchId, Action<List<GameEventData>> callback)
        {
            _gameEventModel.ListenForMatchEvents(matchId, callback);
        }

        public void StopListeningForMatchEvents(string matchId)
        {
            _gameEventModel.StopListeningForMatchEvents(matchId);
        }
    }
}