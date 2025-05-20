using System;
using System.Collections.Generic;

namespace Network.Repositories
{
    public interface IMatchRepository
    {
        void CreateMatch(MatchData matchData, Action<bool> callback = null);
        void UpdateMatch(MatchData matchData, Action<bool> callback = null);
        void GetMatch(string matchId, Action<MatchData> callback);
        void ListenForMatchChanges(string matchId, Action<MatchData> callback);
        void StopListeningForMatch(string matchId);
        string GenerateMatchId();
    }
}