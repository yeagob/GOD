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

        public MatchData(string id, string url, MatchState state)
        {
            _id = id;
            _url = url;
            _state = (int)state;
            _createdAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            _maxPlayers = NetworkConstants.DEFAULT_MAX_PLAYERS;
            _gameMode = NetworkConstants.DEFAULT_GAME_MODE;
        }

        public MatchData(string id, string url, MatchState state, int maxPlayers, string gameMode)
        {
            _id = id;
            _url = url;
            _state = (int)state;
            _createdAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            _maxPlayers = maxPlayers;
            _gameMode = gameMode;
        }

        public MatchState State => (MatchState)_state;
        public bool IsWaitingForPlayers => State == MatchState.WaitingForPlayers;
        public bool IsPlayingGame => State == MatchState.PlayGame;
        public bool IsEndGame => State == MatchState.EndGame;
        public bool IsCancelled => State == MatchState.Cancelled;
        public bool IsEmpty => string.IsNullOrEmpty(_id);
    }

    public struct LocalMatchState
    {
        public MatchData CurrentMatch { get; private set; }
        public bool IsHost { get; private set; }
        public bool IsInMatch => !CurrentMatch.IsEmpty;

        public LocalMatchState(MatchData matchData, bool isHost)
        {
            CurrentMatch = matchData;
            IsHost = isHost;
        }

        public static LocalMatchState CreateHostState(MatchData matchData)
        {
            return new LocalMatchState(matchData, true);
        }

        public static LocalMatchState CreateClientState(MatchData matchData)
        {
            return new LocalMatchState(matchData, false);
        }

        public static LocalMatchState CreateEmptyState()
        {
            return new LocalMatchState(default, false);
        }

        public void UpdateMatchData(MatchData newMatchData)
        {
            CurrentMatch = newMatchData;
        }
    }
}