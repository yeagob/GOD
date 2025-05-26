using System;
using System.Collections.Generic;
using Network;

namespace Tests
{
    public class MockFirebaseDatabaseService : IFirebaseDatabaseService
    {
        private Dictionary<string, MatchData> matches = new Dictionary<string, MatchData>();
        private Dictionary<string, List<PlayerMatchData>> matchPlayers = new Dictionary<string, List<PlayerMatchData>>();
        private Dictionary<string, List<GameEventData>> matchEvents = new Dictionary<string, List<GameEventData>>();
        
        public void CreateMatch(MatchData match, Action<bool, string> onComplete)
        {
            string matchId = Guid.NewGuid().ToString();
            match.MatchId = matchId;
            matches[matchId] = match;
            matchPlayers[matchId] = new List<PlayerMatchData>();
            matchEvents[matchId] = new List<GameEventData>();
            onComplete?.Invoke(true, matchId);
        }

        public void ListenForAvailableMatches(Action<List<MatchData>> onDataChanged)
        {
            var matchList = new List<MatchData>(matches.Values);
            onDataChanged?.Invoke(matchList);
        }

        public void StopListeningForAvailableMatches()
        {
        }

        public void JoinMatch(string matchId, PlayerMatchData player, Action<bool> onComplete)
        {
            if (matches.ContainsKey(matchId))
            {
                if (!matchPlayers.ContainsKey(matchId))
                {
                    matchPlayers[matchId] = new List<PlayerMatchData>();
                }
                matchPlayers[matchId].Add(player);
                onComplete?.Invoke(true);
            }
            else
            {
                onComplete?.Invoke(false);
            }
        }

        public void ListenForMatchStateChanges(string matchId, Action<MatchData> onDataChanged)
        {
            if (matches.ContainsKey(matchId))
            {
                onDataChanged?.Invoke(matches[matchId]);
            }
        }

        public void StopListeningForMatchStateChanges(string matchId)
        {
        }

        public void ListenForPlayersInMatch(string matchId, Action<List<PlayerMatchData>> onDataChanged)
        {
            if (matchPlayers.ContainsKey(matchId))
            {
                onDataChanged?.Invoke(matchPlayers[matchId]);
            }
            else
            {
                onDataChanged?.Invoke(new List<PlayerMatchData>());
            }
        }

        public void StopListeningForPlayersInMatch(string matchId)
        {
        }

        public void AddGameEvent(string matchId, GameEventData gameEvent, Action<bool> onComplete)
        {
            if (matches.ContainsKey(matchId))
            {
                if (!matchEvents.ContainsKey(matchId))
                {
                    matchEvents[matchId] = new List<GameEventData>();
                }
                matchEvents[matchId].Add(gameEvent);
                onComplete?.Invoke(true);
            }
            else
            {
                onComplete?.Invoke(false);
            }
        }

        public void ListenForNewEventsInMatch(string matchId, Action<GameEventData> onEventAdded)
        {
            if (matchEvents.ContainsKey(matchId) && matchEvents[matchId].Count > 0)
            {
                var lastEvent = matchEvents[matchId][matchEvents[matchId].Count - 1];
                onEventAdded?.Invoke(lastEvent);
            }
        }

        public void StopListeningForNewEventsInMatch(string matchId)
        {
        }

        public void UpdateMatchState(string matchId, int newState, Action<bool> onComplete)
        {
            if (matches.ContainsKey(matchId))
            {
                matches[matchId].State = newState;
                onComplete?.Invoke(true);
            }
            else
            {
                onComplete?.Invoke(false);
            }
        }

        public void UpdatePlayerScore(string matchId, string playerId, int newScore, Action<bool> onComplete)
        {
            if (matchPlayers.ContainsKey(matchId))
            {
                var player = matchPlayers[matchId].Find(p => p.PlayerId == playerId);
                if (player != null)
                {
                    player.Score = newScore;
                    onComplete?.Invoke(true);
                    return;
                }
            }
            onComplete?.Invoke(false);
        }

        public void UpdatePlayerData(string matchId, string playerId, PlayerMatchData playerData, Action<bool> onComplete)
        {
            if (matchPlayers.ContainsKey(matchId))
            {
                var playerIndex = matchPlayers[matchId].FindIndex(p => p.PlayerId == playerId);
                if (playerIndex >= 0)
                {
                    matchPlayers[matchId][playerIndex] = playerData;
                    onComplete?.Invoke(true);
                    return;
                }
            }
            onComplete?.Invoke(false);
        }

        public void RemoveMatch(string matchId, Action<bool> onComplete)
        {
            bool removed = matches.Remove(matchId);
            if (removed)
            {
                matchPlayers.Remove(matchId);
                matchEvents.Remove(matchId);
            }
            onComplete?.Invoke(removed);
        }

        public void RemovePlayerFromMatch(string matchId, string playerId, Action<bool> onComplete)
        {
            if (matchPlayers.ContainsKey(matchId))
            {
                var removed = matchPlayers[matchId].RemoveAll(p => p.PlayerId == playerId) > 0;
                onComplete?.Invoke(removed);
            }
            else
            {
                onComplete?.Invoke(false);
            }
        }
    }
}