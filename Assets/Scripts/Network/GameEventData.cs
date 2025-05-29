namespace Network
{
    
    public readonly struct GameEventData
    {
        public readonly string _id;
        public readonly string _playerId;
        public readonly string _matchId;
        public readonly int _eventType;
        public readonly int _targetTile;

        public GameEventData(string id, string playerId, string matchId, int eventType, int targetTile)
        {
            _id = id;
            _playerId = playerId;
            _matchId = matchId;
            _eventType = eventType;
            _targetTile = targetTile;
        }
    }
}