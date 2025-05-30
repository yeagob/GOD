namespace Network.Infrastructure
{
    public static class NetworkConstants
    {
        public const string FIREBASE_DATABASE_URL = "https://game-of-duck-multiplayer-default-rtdb.europe-west1.firebasedatabase.app/";
        
        public const string MATCHES_PATH = "matches";
        public const string PLAYER_MATCHES_PATH = "player_matches";
        public const string GAME_EVENTS_PATH = "game_events";
        
        public const int DEFAULT_MAX_PLAYERS = 4;
        public const string DEFAULT_GAME_MODE = "classic";
        
        public const float FIREBASE_POLLING_INTERVAL = 2.0f;
        public const float NETWORK_INITIALIZATION_DELAY = 0.5f;
    }

    public enum MatchState
    {
        WaitingForPlayers = 0,
        PlayGame = 1,
        EndGame = 2,
        Cancelled = 3
    }
}