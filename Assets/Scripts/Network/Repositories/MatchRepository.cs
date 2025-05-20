using System;
using System.Collections.Generic;
using Network.Services;

namespace Network.Repositories
{
    public class MatchRepository : IMatchRepository
    {
        private readonly IFirebaseService _firebaseService;
        private const string PATH = "matches";

        public MatchRepository(IFirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
        }

        public void CreateMatch(MatchData matchData, Action<bool> callback = null)
        {
            string path = $"{PATH}/{matchData._id}";
            _firebaseService.SetData(path, matchData, callback);
        }

        public void UpdateMatch(MatchData matchData, Action<bool> callback = null)
        {
            CreateMatch(matchData, callback);
        }

        public void GetMatch(string matchId, Action<MatchData> callback)
        {
            string path = $"{PATH}/{matchId}";
            _firebaseService.GetData<MatchData>(path, callback);
        }

        public void ListenForMatchChanges(string matchId, Action<MatchData> callback)
        {
            string path = $"{PATH}/{matchId}";
            _firebaseService.ListenForChanges(path, callback);
        }

        public void StopListeningForMatch(string matchId)
        {
            string path = $"{PATH}/{matchId}";
            _firebaseService.StopListening(path);
        }

        public string GenerateMatchId()
        {
            return _firebaseService.GenerateId(PATH);
        }
    }
}