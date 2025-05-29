using System;
using System.Collections.Generic;

namespace Network.Models
{
    public interface IMatchModel
    {
        void CreateMatch(string url, int state, Action<MatchData> callback = null);
        void UpdateMatchState(string matchId, int newState, Action<bool> callback = null);
        void GetMatch(string matchId, Action<MatchData> callback);
        void ListenForMatchChanges(string matchId, Action<MatchData> callback);
        void StopListeningForMatch(string matchId);
    }
}