using System;
using System.Collections.Generic;

namespace Network.Presenters
{
    public interface IPlayerMatchPresenter
    {
        string JoinMatch(PlayerMatchData playerMatchData, Action<bool> callback = null);
        void UpdateScore(string playerMatchId, int scoreChange, Action<bool> callback = null);
        void GetPlayerInfo(string playerMatchId, Action<PlayerMatchData> callback);
        void GetMatchPlayers(string matchId, Action<List<PlayerMatchData>> callback);
        void ListenForPlayerUpdates(string playerMatchId, Action<PlayerMatchData> callback);
        void ListenForMatchPlayersUpdates(string matchId, Action<List<PlayerMatchData>> callback);
        void StopListeningForPlayer(string playerMatchId);
        void StopListeningForMatchPlayers(string matchId);
    }
}
