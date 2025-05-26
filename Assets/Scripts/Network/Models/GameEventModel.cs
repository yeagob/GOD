using System;
using System.Collections.Generic;
using Network.Repositories;

namespace Network.Models
{
    public class GameEventModel : IGameEventModel
    {
        private readonly IGameEventRepository _gameEventRepository;

        public GameEventModel(IGameEventRepository gameEventRepository)
        {
            _gameEventRepository = gameEventRepository;
        }

        public void CreateGameEvent(string playerId, string matchId, int eventType, int targetTile, Action<GameEventData> callback = null)
        {
            string eventId = _gameEventRepository.GenerateGameEventId();
            GameEventData gameEventData = new GameEventData(eventId, playerId, matchId, eventType, targetTile);
            
            _gameEventRepository.CreateGameEvent(gameEventData, success => {
                if (success)
                {
                    callback?.Invoke(gameEventData);
                }
                else
                {
                    callback?.Invoke(default);
                }
            });
        }

        public void GetEventsByMatch(string matchId, Action<List<GameEventData>> callback)
        {
            _gameEventRepository.GetGameEventsByMatchId(matchId, callback);
        }

        public void GetEventsByPlayer(string playerId, Action<List<GameEventData>> callback)
        {
            _gameEventRepository.GetGameEventsByPlayerId(playerId, callback);
        }

        public void ListenForMatchEvents(string matchId, Action<List<GameEventData>> callback)
        {
            _gameEventRepository.ListenForMatchEvents(matchId, callback);
        }

        public void StopListeningForMatchEvents(string matchId)
        {
            _gameEventRepository.StopListeningForMatchEvents(matchId);
        }
    }
}