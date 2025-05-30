using System;
using System.Collections.Generic;

namespace Network.Models
{
    public interface IPlayerMatchModel
    {
        public string CreatePlayerMatch(string name, string matchId, Action<bool> callback = null);
        
        public string CreatePlayerMatch(string name, string matchId, Action<PlayerMatchData> callback = null);

        public void UpdatePlayerScore(string playerMatchId, int newScore, Action<bool> callback = null);

        public void GetPlayerMatch(string playerMatchId, Action<PlayerMatchData> callback);

        public void GetPlayersByMatch(string matchId, Action<List<PlayerMatchData>> callback);

        public void ListenForPlayerMatchChanges(string playerMatchId, Action<PlayerMatchData> callback);

        public void ListenForMatchPlayersChanges(string matchId, Action<List<PlayerMatchData>> callback);

        public void StopListeningForPlayerMatch(string playerMatchId);

        public void StopListeningForMatchPlayers(string matchId);
    }
}