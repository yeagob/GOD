using System;
using System.Collections.Generic;
using Network.Repositories;

namespace Network.Models
{
    public class PlayerMatchModel : IPlayerMatchModel
    {
        private readonly IPlayerMatchRepository _playerMatchRepository;

        public PlayerMatchModel(IPlayerMatchRepository playerMatchRepository)
        {
            _playerMatchRepository = playerMatchRepository;
        }

        public string CreatePlayerMatch(string name, string matchId, Action<PlayerMatchData> callback = null)
        {
            throw new NotImplementedException();
        }

        public string CreatePlayerMatch(string name, string matchId, Action<bool> callback = null)
        {
            string playerMatchId = _playerMatchRepository.GeneratePlayerMatchId();
            PlayerMatchData playerMatchData = new PlayerMatchData(playerMatchId, name, matchId, 0);
            
            _playerMatchRepository.CreatePlayerMatch(playerMatchData, success => {
                if (success)
                {
                    callback?.Invoke(true);
                }
                else
                {
                    callback?.Invoke(false);
                }
            });

            return playerMatchId;
        }

        public string CreatePlayerMatch(PlayerMatchData playerMatchData, Action<bool> callback = null)
        {
            string playerMatchId = _playerMatchRepository.GeneratePlayerMatchId();
            
            PlayerMatchData completePlayerData = new PlayerMatchData(
                playerMatchId,
                playerMatchData._name,
                playerMatchData._matchId,
                playerMatchData._tile,
                playerMatchData._color
            );
            
            _playerMatchRepository.CreatePlayerMatch(completePlayerData, success => {
                if (success)
                {
                    callback?.Invoke(true);
                }
                else
                {
                    callback?.Invoke(false);
                }
            });

            return playerMatchId;
        }

        public void UpdatePlayerScore(string playerMatchId, int newScore, Action<bool> callback = null)
        {
            _playerMatchRepository.GetPlayerMatch(playerMatchId, existingPlayer => {
                if (string.IsNullOrEmpty(existingPlayer._id))
                {
                    callback?.Invoke(false);
                    return;
                }
                
                PlayerMatchData updatedPlayer = new PlayerMatchData(
                    existingPlayer._id,
                    existingPlayer._name,
                    existingPlayer._matchId,
                    newScore,
                    existingPlayer._color
                );
                
                _playerMatchRepository.UpdatePlayerMatch(updatedPlayer, callback);
            });
        }

        public void GetPlayerMatch(string playerMatchId, Action<PlayerMatchData> callback)
        {
            _playerMatchRepository.GetPlayerMatch(playerMatchId, callback);
        }

        public void GetPlayersByMatch(string matchId, Action<List<PlayerMatchData>> callback)
        {
            _playerMatchRepository.GetPlayerMatchesByMatchId(matchId, callback);
        }

        public void ListenForPlayerMatchChanges(string playerMatchId, Action<PlayerMatchData> callback)
        {
            _playerMatchRepository.ListenForPlayerMatchChanges(playerMatchId, callback);
        }

        public void ListenForMatchPlayersChanges(string matchId, Action<List<PlayerMatchData>> callback)
        {
            _playerMatchRepository.ListenForMatchPlayerChanges(matchId, callback);
        }

        public void StopListeningForPlayerMatch(string playerMatchId)
        {
            _playerMatchRepository.StopListeningForPlayerMatch(playerMatchId);
        }

        public void StopListeningForMatchPlayers(string matchId)
        {
            _playerMatchRepository.StopListeningForMatchPlayers(matchId);
        }
    }
}