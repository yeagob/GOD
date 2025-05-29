using System;
using System.Collections.Generic;

namespace Network.Repositories
{
    public interface IGameEventRepository
    {
        void CreateGameEvent(GameEventData gameEventData, Action<bool> callback = null);
        void GetGameEventsByMatchId(string matchId, Action<List<GameEventData>> callback);
        void GetGameEventsByPlayerId(string playerId, Action<List<GameEventData>> callback);
        void ListenForMatchEvents(string matchId, Action<List<GameEventData>> callback);
        void StopListeningForMatchEvents(string matchId);
        string GenerateGameEventId();
    }
}