using System;
using Network.Infrastructure;

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
            _maxPlayers = NetworkConstants.DEFAULT_MAX_PLAYERS;
            _gameMode = NetworkConstants.DEFAULT_GAME_MODE;
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

        public bool IsWaiting => _state == NetworkConstants.MATCH_STATE_WAITING;
        public bool IsPlaying => _state == NetworkConstants.MATCH_STATE_PLAYING;
        public bool IsFinished => _state == NetworkConstants.MATCH_STATE_FINISHED;
        public bool IsCancelled => _state == NetworkConstants.MATCH_STATE_CANCELLED;
    }
}
