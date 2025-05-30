namespace Network.Models
{
    public struct MatchState
    {
        public string CurrentMatchId { get; private set; }
        public bool IsHost { get; private set; }
        public bool IsInMatch { get; private set; }

        public MatchState(string matchId, bool isHost)
        {
            CurrentMatchId = matchId;
            IsHost = isHost;
            IsInMatch = !string.IsNullOrEmpty(matchId);
        }

        public static MatchState CreateHostState(string matchId)
        {
            return new MatchState(matchId, true);
        }

        public static MatchState CreateClientState(string matchId)
        {
            return new MatchState(matchId, false);
        }

        public static MatchState CreateEmptyState()
        {
            return new MatchState(string.Empty, false);
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(CurrentMatchId);
        }
    }
}