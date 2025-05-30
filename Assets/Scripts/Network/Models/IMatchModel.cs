using System;
using Network.Infrastructure;

namespace Network.Models
{
    public interface IMatchModel
    {
        public LocalMatchState CurrentMatchState { get; }

        public void CreateMatch(string url, MatchState state, Action<MatchData> callback = null);

        public void CreateMatch(string matchId, string url, MatchState state, Action<MatchData> callback = null);

        public void UpdateMatchState(string matchId, MatchState newState, Action<bool> callback = null);

        public void GetMatch(string matchId, Action<MatchData> callback);

        public void ListenForMatchChanges(string matchId, Action<MatchData> callback);

        public void StopListeningForMatch(string matchId);

        public void SetAsHost(MatchData matchData);

        public void SetAsClient(MatchData matchData);

        public void UpdateLocalMatch(MatchData matchData);

        public void ClearMatchState();

        public bool IsCurrentlyInMatch();

        public bool IsHost();
    }
}