using System;
using System.Collections.Generic;

namespace Network
{
    public class LobbyPresenter : IDisposable
    {
        private readonly IFirebaseDatabaseService _databaseService;
        private List<MatchData> _availableMatches;

        public event Action<List<MatchData>> OnAvailableMatchesUpdated;
        public event Action<bool, string> OnMatchCreated; // success, matchId
        public event Action<bool> OnJoinedMatch; // success


        public LobbyPresenter(IFirebaseDatabaseService databaseService)
        {
            _databaseService = databaseService;
            _availableMatches = new List<MatchData>();
            StartListeningForAvailableMatches();
        }

        private void StartListeningForAvailableMatches()
        {
            _databaseService.ListenForAvailableMatches((matches) =>
            {
                _availableMatches = matches;
                OnAvailableMatchesUpdated?.Invoke(_availableMatches);
            });
        }

        public void CreateMatch(string matchUrl, string matchId)
        {
            MatchData newMatch = new MatchData(matchId, matchUrl, 0);
            _databaseService.CreateMatch(newMatch, (success, matchId) =>
            {
                OnMatchCreated?.Invoke(success, matchId);
                if (success)
                {
                    // Maybe join the created match automatically
                    // JoinMatch(matchId, new PlayerMatchData { _name = "HostPlayer" }); // Example
                }
            });
        }

        public void JoinMatch(string matchId, string playerName, string playerId)
        {
            PlayerMatchData player = new PlayerMatchData(playerId,matchId,playerName, 0);
            _databaseService.JoinMatch(matchId, player, (success) =>
            {
                OnJoinedMatch?.Invoke(success);
            });
        }

        public void Dispose()
        {
            _databaseService.StopListeningForAvailableMatches();
        }
    }
}