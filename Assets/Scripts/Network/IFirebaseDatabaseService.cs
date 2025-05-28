using System;
using System.Collections.Generic;

namespace Network
{
    public interface IFirebaseDatabaseService
    {
        event Action OnInitialized;
        event Action<string> OnInitializationError;

        void CreateMatch(MatchData match, Action<bool, string> onComplete);
        void ListenForAvailableMatches(Action<List<MatchData>> onDataChanged);
        void StopListeningForAvailableMatches();
        void JoinMatch(string matchId, PlayerMatchData player, Action<bool> onComplete);
        void ListenForMatchStateChanges(string matchId, Action<MatchData> onDataChanged);
        void StopListeningForMatchStateChanges(string matchId);
        void ListenForPlayersInMatch(string matchId, Action<List<PlayerMatchData>> onDataChanged);
        void StopListeningForPlayersInMatch(string matchId);
        void AddGameEvent(string matchId, GameEventData gameEvent, Action<bool> onComplete);
        void ListenForNewEventsInMatch(string matchId, Action<GameEventData> onEventAdded);
        void StopListeningForNewEventsInMatch(string matchId);
        void UpdateMatchState(string matchId, int newState, Action<bool> onComplete);
        void UpdatePlayerScore(string matchId, string playerId, int newScore, Action<bool> onComplete);
        void UpdatePlayerData(string matchId, string playerId, PlayerMatchData playerData, Action<bool> onComplete);
        void RemoveMatch(string matchId, Action<bool> onComplete);
        void RemovePlayerFromMatch(string matchId, string playerId, Action<bool> onComplete);
    }
}
