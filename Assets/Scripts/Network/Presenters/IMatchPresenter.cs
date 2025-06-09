using System;
using System.Collections.Generic;

namespace Network.Presenters
{
    public interface IMatchPresenter
    {
        void CreateMatch(MatchData matchData, Action<bool> callback = null);
        void GetMatch(string matchId, Action<MatchData> callback);
        void CreateNewMatch(string url, Action<MatchData> callback = null);
        void JoinMatch(string matchId, string playerName, Action<PlayerMatchData> callback = null);
        void StartMatch(string matchId, Action<bool> callback = null);
        void EndMatch(string matchId, Action<bool> callback = null);
        void GetCurrentMatchState(string matchId, Action<MatchData, List<PlayerMatchData>, List<GameEventData>> callback);
        void ListenForMatchUpdates(string matchId, Action<MatchData> onMatchChanged, Action<List<PlayerMatchData>> onPlayersChanged, Action<List<GameEventData>> onEventsChanged);
        void StopListeningForMatchUpdates(string matchId);
    }
}
