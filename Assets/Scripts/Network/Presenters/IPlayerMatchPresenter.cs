using System;
using System.Collections.Generic;

namespace Network.Presenters
{
    public interface IPlayerMatchPresenter
    {
        void JoinMatch(string matchId, string playerName, Action<PlayerMatchData> callback = null);
        void UpdateScore(string playerMatchId, int scoreChange, Action<bool> callback = null);
        void GetPlayerInfo(string playerMatchId, Action<PlayerMatchData> callback);
        void GetMatchPlayers(string matchId, Action<List<PlayerMatchData>> callback);
        void ListenForPlayerUpdates(string playerMatchId, Action<PlayerMatchData> callback);
        void ListenForMatchPlayersUpdates(string matchId, Action<List<PlayerMatchData>> callback);
        void StopListeningForPlayer(string playerMatchId);
        void StopListeningForMatchPlayers(string matchId);
    }
}