namespace Network
{
    public readonly struct MatchData
    {
        public readonly string _id;
        public readonly string _url;
        public readonly int _state;

        public MatchData(string id, string url, int state)
        {
            _id = id;
            _url = url;
            _state = state;
        }
    }
}