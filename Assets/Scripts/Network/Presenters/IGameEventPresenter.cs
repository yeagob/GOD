using System;
using System.Collections.Generic;

namespace Network.Presenters
{
    public interface IGameEventPresenter
    {
        void RecordMovement(string playerId, string matchId, int targetTile, Action<GameEventData> callback = null);
        void RecordJump(string playerId, string matchId, int targetTile, Action<GameEventData> callback = null);
        void RecordQuestion(string playerId, string matchId, int targetTile, Action<GameEventData> callback = null);
        void RecordScoreChange(string playerId, string matchId, int targetTile, Action<GameEventData> callback = null);
        void RecordReview(string playerId, string matchId, int targetTile, Action<GameEventData> callback = null);
        void GetMatchTimeline(string matchId, Action<List<GameEventData>> callback);
        void GetPlayerTimeline(string playerId, Action<List<GameEventData>> callback);
        void ListenForMatchEvents(string matchId, Action<List<GameEventData>> callback);
        void StopListeningForMatchEvents(string matchId);
    }
}