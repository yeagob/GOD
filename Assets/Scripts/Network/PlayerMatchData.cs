namespace Network
{
    public readonly struct PlayerMatchData
    {
        public readonly string _id;
        public readonly string _name;
        public readonly string _matchId;
        public readonly int _score;

        public PlayerMatchData(string id, string name, string matchId, int score)
        {
            _id = id;
            _name = name;
            _matchId = matchId;
            _score = score;
        }
    }
}