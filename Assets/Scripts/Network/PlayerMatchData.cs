namespace Network
{
    public readonly struct PlayerMatchData
    {
        public readonly string _id;
        public readonly string _name;
        public readonly string _matchId;
        public readonly int _tile;

        public PlayerMatchData(string id, string name, string matchId, int tile)
        {
            _id = id;
            _name = name;
            _matchId = matchId;
            _tile = tile;
        }
    }
}