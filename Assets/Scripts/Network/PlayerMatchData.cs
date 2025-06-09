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
        public string _color;

        public PlayerMatchData(string id, string name, string matchId, int tile, string color = "")
        {
            _id = id;
            _name = name;
            _matchId = matchId;
            _tile = tile;
            _color = color;
        }
    }
}