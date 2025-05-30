namespace Network.Models
{
    public struct CurrentMatch
    {
        public string Id { get; private set; }
        public bool IsHost { get; private set; }
        public bool IsInMatch { get; private set; }

        public CurrentMatch(string matchId, bool isHost)
        {
            Id = matchId;
            IsHost = isHost;
            IsInMatch = !string.IsNullOrEmpty(matchId);
        }

        public static CurrentMatch CreateHostState(string matchId)
        {
            return new CurrentMatch(matchId, true);
        }

        public static CurrentMatch CreateClientState(string matchId)
        {
            return new CurrentMatch(matchId, false);
        }

        public static CurrentMatch CreateEmptyState()
        {
            return new CurrentMatch(string.Empty, false);
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(Id);
        }
    }
}