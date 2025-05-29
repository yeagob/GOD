using System;
using System.Collections.Generic;

namespace Network.Models
{
    public interface IGameEventModel
    {
        void CreateGameEvent(string playerId, string matchId, int eventType, int targetTile, Action<GameEventData> callback = null);
        void GetEventsByMatch(string matchId, Action<List<GameEventData>> callback);
        void GetEventsByPlayer(string playerId, Action<List<GameEventData>> callback);
        void ListenForMatchEvents(string matchId, Action<List<GameEventData>> callback);
        void StopListeningForMatchEvents(string matchId);
    }
}