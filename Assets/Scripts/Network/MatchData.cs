using System;

namespace Network
{
    [Serializable]
    public struct MatchData
    {
        public string _id;
        public string _url;
        public int _state;
        public string _createdAt;
        public int _maxPlayers;
        public string _gameMode;

        public MatchData(string id, string url, int state)
        {
            _id = id;
            _url = url;
            _state = state;
            _createdAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            _maxPlayers = 4;
            _gameMode = "classic";
        }

        public MatchData(string id, string url, int state, int maxPlayers, string gameMode)
        {
            _id = id;
            _url = url;
            _state = state;
            _createdAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            _maxPlayers = maxPlayers;
            _gameMode = gameMode;
        }
    }
}
