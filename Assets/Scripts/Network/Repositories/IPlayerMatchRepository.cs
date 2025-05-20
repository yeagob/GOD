using System;
using System.Collections.Generic;

namespace Network.Repositories
{
    public interface IPlayerMatchRepository
    {
        void CreatePlayerMatch(PlayerMatchData playerMatchData, Action<bool> callback = null);
        void UpdatePlayerMatch(PlayerMatchData playerMatchData, Action<bool> callback = null);
        void GetPlayerMatch(string playerMatchId, Action<PlayerMatchData> callback);
        void GetPlayerMatchesByMatchId(string matchId, Action<List<PlayerMatchData>> callback);
        void ListenForPlayerMatchChanges(string playerMatchId, Action<PlayerMatchData> callback);
        void ListenForMatchPlayerChanges(string matchId, Action<List<PlayerMatchData>> callback);
        void StopListeningForPlayerMatch(string playerMatchId);
        void StopListeningForMatchPlayers(string matchId);
        string GeneratePlayerMatchId();
    }
}