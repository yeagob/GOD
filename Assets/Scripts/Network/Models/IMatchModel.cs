using System;
using System.Collections.Generic;

namespace Network.Models
{
    public interface IMatchModel
    {
        MatchState CurrentMatchState { get; }
        
        void CreateMatch(string url, int state, Action<MatchData> callback = null);
        void UpdateMatchState(string matchId, int newState, Action<bool> callback = null);
        void GetMatch(string matchId, Action<MatchData> callback);
        void ListenForMatchChanges(string matchId, Action<MatchData> callback);
        void StopListeningForMatch(string matchId);
        
        void SetAsHost(string matchId);
        void SetAsClient(string matchId);
        void ClearMatchState();
        bool IsCurrentlyInMatch();
        bool IsHost();
    }
}