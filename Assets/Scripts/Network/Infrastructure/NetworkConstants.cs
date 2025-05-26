namespace Network.Infrastructure
{
    public static class NetworkConstants
    {
        // Estados del partido
        public const int MATCH_STATE_WAITING = 0;
        public const int MATCH_STATE_PLAYING = 1;
        public const int MATCH_STATE_FINISHED = 2;
        
        // Tipos de evento
        public const int EVENT_TYPE_MOVE = 0;
        public const int EVENT_TYPE_JUMP = 1;
        public const int EVENT_TYPE_QUESTION = 2;
        public const int EVENT_TYPE_SCORE_CHANGE = 3;
        public const int EVENT_TYPE_REVIEW = 4;
    }
}