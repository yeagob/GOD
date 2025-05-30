using System;

namespace Network
{
    [Serializable]
    public struct PlayerMatchData
    {
        public string _id;
        public string _name;
        public string _matchId;
        public int _tile;

        public PlayerMatchData(string id, string name, string matchId, int tile)
        {
            _id = id;
            _name = name;
            _matchId = matchId;
            _tile = tile;
        }
    }
}