using System;
using System.Collections.Generic;
using Network.Services;
using Network.Infrastructure;

namespace Network.Repositories
{
    public class MatchRepository : IMatchRepository
    {
        private readonly IFirebaseService _firebaseService;

        public MatchRepository(IFirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
        }

        public void CreateMatch(MatchData matchData, Action<bool> callback = null)
        {
            string path = $"{NetworkConstants.MATCHES_PATH}/{matchData._id}";
            _firebaseService.SetData(path, matchData, callback);
        }

        public void UpdateMatch(MatchData matchData, Action<bool> callback = null)
        {
            CreateMatch(matchData, callback);
        }

        public void GetMatch(string matchId, Action<MatchData> callback)
        {
            string path = $"{NetworkConstants.MATCHES_PATH}/{matchId}";
            _firebaseService.GetData<MatchData>(path, callback);
        }

        public void ListenForMatchChanges(string matchId, Action<MatchData> callback)
        {
            string path = $"{NetworkConstants.MATCHES_PATH}/{matchId}";
            _firebaseService.ListenForChanges(path, callback);
        }

        public void StopListeningForMatch(string matchId)
        {
            string path = $"{NetworkConstants.MATCHES_PATH}/{matchId}";
            _firebaseService.StopListening(path);
        }

        public string GenerateMatchId()
        {
            return _firebaseService.GenerateId(NetworkConstants.MATCHES_PATH);
        }
    }
}
