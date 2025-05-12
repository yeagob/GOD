using System;
using System.Collections.Generic;

namespace Network
{
    public interface IFirebaseDatabaseService
    {
        // Match Management
        public void CreateMatch(MatchData match, Action<bool, string> onComplete);

        // bool success, string matchId
        public void ListenForAvailableMatches(Action<List<MatchData>> onDataChanged);
        public void StopListeningForAvailableMatches();
        public void JoinMatch(string matchId, PlayerMatchData player, Action<bool> onComplete);
        public void ListenForMatchStateChanges(string matchId, Action<MatchData> onDataChanged);
        public void StopListeningForMatchStateChanges(string matchId);
        public void ListenForPlayersInMatch(string matchId, Action<List<PlayerMatchData>> onDataChanged);
        public void StopListeningForPlayersInMatch(string matchId);

        // Game Events
        public void AddGameEvent(string matchId, GameEventData gameEvent, Action<bool> onComplete);
        public void ListenForNewEventsInMatch(string matchId, Action<GameEventData> onEventAdded);
        public void StopListeningForNewEventsInMatch(string matchId);

        // Data Updates
        public void UpdateMatchState(string matchId, int newState, Action<bool> onComplete);
        public void UpdatePlayerScore(string matchId, string playerId, int newScore, Action<bool> onComplete);
        public void UpdatePlayerData(string matchId, string playerId, PlayerMatchData playerData, Action<bool> onComplete);

        // Clean Up
        public void RemoveMatch(string matchId, Action<bool> onComplete);
        public void RemovePlayerFromMatch(string matchId, string playerId, Action<bool> onComplete);

        // Initialization (conceptual, Firebase init handled elsewhere usually)
        // void InitializeFirebase();
    }
}