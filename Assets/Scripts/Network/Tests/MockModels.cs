using System;
using System.Collections.Generic;
using Network.Infrastructure;
using Network.Models;
using Network.Presenters;

namespace Network.Tests
{
    // Clases Mock compartidas para todos los tests
    public class MockGameEventModel : IGameEventModel
    {
        public Action<string, string, int, int, Action<GameEventData>> OnCreateGameEvent { get; set; }
        public Action<string, Action<List<GameEventData>>> OnGetEventsByMatch { get; set; }
        public Action<string, Action<List<GameEventData>>> OnGetEventsByPlayer { get; set; }
        public Action<string, Action<List<GameEventData>>> OnListenForMatchEvents { get; set; }
        public Action<string> OnStopListeningForMatchEvents { get; set; }
        
        public void CreateGameEvent(string playerId, string matchId, int eventType, int targetTile, Action<GameEventData> callback = null)
        {
            OnCreateGameEvent?.Invoke(playerId, matchId, eventType, targetTile, callback);
        }
        
        public void GetEventsByMatch(string matchId, Action<List<GameEventData>> callback)
        {
            OnGetEventsByMatch?.Invoke(matchId, callback);
        }
        
        public void GetEventsByPlayer(string playerId, Action<List<GameEventData>> callback)
        {
            OnGetEventsByPlayer?.Invoke(playerId, callback);
        }
        
        public void ListenForMatchEvents(string matchId, Action<List<GameEventData>> callback)
        {
            OnListenForMatchEvents?.Invoke(matchId, callback);
        }
        
        public void StopListeningForMatchEvents(string matchId)
        {
            OnStopListeningForMatchEvents?.Invoke(matchId);
        }
    }
    
    public class MockPlayerMatchModel : IPlayerMatchModel
    {
        public Action<string, string, Action<PlayerMatchData>> OnCreatePlayerMatch { get; set; }
        public Action<string, int, Action<bool>> OnUpdatePlayerScore { get; set; }
        public Action<string, Action<PlayerMatchData>> OnGetPlayerMatch { get; set; }
        public Action<string, Action<List<PlayerMatchData>>> OnGetPlayersByMatch { get; set; }
        public Action<string, Action<PlayerMatchData>> OnListenForPlayerMatchChanges { get; set; }
        public Action<string, Action<List<PlayerMatchData>>> OnListenForMatchPlayersChanges { get; set; }
        public Action<string> OnStopListeningForPlayerMatch { get; set; }
        public Action<string> OnStopListeningForMatchPlayers { get; set; }

        public string CreatePlayerMatch(string name, string matchId, Action<bool> callback = null)
        {
            throw new NotImplementedException();
        }

        public string CreatePlayerMatch(string name, string matchId, Action<PlayerMatchData> callback = null)
        {
            OnCreatePlayerMatch?.Invoke(name, matchId, callback);
            return "";
        }
        
        public void UpdatePlayerScore(string playerMatchId, int newScore, Action<bool> callback = null)
        {
            OnUpdatePlayerScore?.Invoke(playerMatchId, newScore, callback);
        }
        
        public void GetPlayerMatch(string playerMatchId, Action<PlayerMatchData> callback)
        {
            OnGetPlayerMatch?.Invoke(playerMatchId, callback);
        }
        
        public void GetPlayersByMatch(string matchId, Action<List<PlayerMatchData>> callback)
        {
            OnGetPlayersByMatch?.Invoke(matchId, callback);
        }
        
        public void ListenForPlayerMatchChanges(string playerMatchId, Action<PlayerMatchData> callback)
        {
            OnListenForPlayerMatchChanges?.Invoke(playerMatchId, callback);
        }
        
        public void ListenForMatchPlayersChanges(string matchId, Action<List<PlayerMatchData>> callback)
        {
            OnListenForMatchPlayersChanges?.Invoke(matchId, callback);
        }
        
        public void StopListeningForPlayerMatch(string playerMatchId)
        {
            OnStopListeningForPlayerMatch?.Invoke(playerMatchId);
        }
        
        public void StopListeningForMatchPlayers(string matchId)
        {
            OnStopListeningForMatchPlayers?.Invoke(matchId);
        }
    }
    
    public class MockMatchModel : IMatchModel
    {
        public Action<string, int, Action<MatchData>> OnCreateMatch { get; set; }
        public Action<string, int, Action<bool>> OnUpdateMatchState { get; set; }
        public Action<string, Action<MatchData>> OnGetMatch { get; set; }
        public Action<string, Action<MatchData>> OnListenForMatchChanges { get; set; }
        public Action<string> OnStopListeningForMatch { get; set; }
        
        public void CreateMatch(string url, int state, Action<MatchData> callback = null)
        {
            OnCreateMatch?.Invoke(url, state, callback);
        }
        
        public void UpdateMatchState(string matchId, int newState, Action<bool> callback = null)
        {
            OnUpdateMatchState?.Invoke(matchId, newState, callback);
        }

        public LocalMatchState CurrentMatchState { get; }
        
        public void CreateMatch(string url, MatchState state, Action<MatchData> callback = null)
        {
            throw new NotImplementedException();
        }

        public void CreateMatch(string matchId, string url, MatchState state, Action<MatchData> callback = null)
        {
            throw new NotImplementedException();
        }

        public void UpdateMatchState(string matchId, MatchState newState, Action<bool> callback = null)
        {
            throw new NotImplementedException();
        }

        public void GetMatch(string matchId, Action<MatchData> callback)
        {
            OnGetMatch?.Invoke(matchId, callback);
        }
        
        public void ListenForMatchChanges(string matchId, Action<MatchData> callback)
        {
            OnListenForMatchChanges?.Invoke(matchId, callback);
        }
        
        public void StopListeningForMatch(string matchId)
        {
            OnStopListeningForMatch?.Invoke(matchId);
        }

        public void SetAsHost(MatchData matchData)
        {
            throw new NotImplementedException();
        }

        public void SetAsClient(MatchData matchData)
        {
            throw new NotImplementedException();
        }

        public void UpdateLocalMatch(MatchData matchData)
        {
            throw new NotImplementedException();
        }

        public void ClearMatchState()
        {
            throw new NotImplementedException();
        }

        public bool IsCurrentlyInMatch()
        {
            throw new NotImplementedException();
        }

        public bool IsHost()
        {
            throw new NotImplementedException();
        }
    }
}