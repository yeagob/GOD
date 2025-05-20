using System;
using System.Collections.Generic;

namespace Network.Models
{
    public interface IPlayerMatchModel
    {
        void CreatePlayerMatch(string name, string matchId, Action<PlayerMatchData> callback = null);
        void UpdatePlayerScore(string playerMatchId, int newScore, Action<bool> callback = null);
        void GetPlayerMatch(string playerMatchId, Action<PlayerMatchData> callback);
        void GetPlayersByMatch(string matchId, Action<List<PlayerMatchData>> callback);
        void ListenForPlayerMatchChanges(string playerMatchId, Action<PlayerMatchData> callback);
        void ListenForMatchPlayersChanges(string matchId, Action<List<PlayerMatchData>> callback);
        void StopListeningForPlayerMatch(string playerMatchId);
        void StopListeningForMatchPlayers(string matchId);
    }
}