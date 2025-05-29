using System;
using System.Collections.Generic;
using UnityEngine;
using Network.Services;

namespace Network.Repositories
{
    public class PlayerMatchRepository : IPlayerMatchRepository
    {
        private readonly IFirebaseService _firebaseService;
        private const string PATH = "player_matches";
        private const string MATCH_INDEX = "match_index";

        public PlayerMatchRepository(IFirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
        }

        public void CreatePlayerMatch(PlayerMatchData playerMatchData, Action<bool> callback = null)
        {
            string path = $"{PATH}/{playerMatchData._id}";
            _firebaseService.SetData(path, playerMatchData, result => {
                if (result)
                {
                    // También indexamos por match para consultas más rápidas
                    string indexPath = $"{MATCH_INDEX}/{playerMatchData._matchId}/{playerMatchData._id}";
                    _firebaseService.SetData(indexPath, true, indexResult => {
                        callback?.Invoke(indexResult);
                    });
                }
                else
                {
                    callback?.Invoke(false);
                }
            });
        }

        public void UpdatePlayerMatch(PlayerMatchData playerMatchData, Action<bool> callback = null)
        {
            CreatePlayerMatch(playerMatchData, callback);
        }

        public void GetPlayerMatch(string playerMatchId, Action<PlayerMatchData> callback)
        {
            string path = $"{PATH}/{playerMatchId}";
            _firebaseService.GetData<PlayerMatchData>(path, callback);
        }

        public void GetPlayerMatchesByMatchId(string matchId, Action<List<PlayerMatchData>> callback)
        {
            // En una implementación real, aquí se obtendría la lista de IDs de PlayerMatch
            // asociados al matchId desde el índice, y luego se recuperaría cada uno individualmente
            
            // Para este ejemplo, simplemente devolvemos una lista vacía
            Debug.Log($"Getting players for match {matchId}");
            callback?.Invoke(new List<PlayerMatchData>());
        }

        public void ListenForPlayerMatchChanges(string playerMatchId, Action<PlayerMatchData> callback)
        {
            string path = $"{PATH}/{playerMatchId}";
            _firebaseService.ListenForChanges(path, callback);
        }

        public void ListenForMatchPlayerChanges(string matchId, Action<List<PlayerMatchData>> callback)
        {
            // En una implementación real, aquí escucharíamos cambios en los índices de jugadores
            // por partido y actualizaríamos la lista completa cuando hubiera cambios
            
            string path = $"{MATCH_INDEX}/{matchId}";
            Debug.Log($"Listening for player changes in match {matchId}");
            
            // Para este ejemplo, simulamos que no hay jugadores
            callback?.Invoke(new List<PlayerMatchData>());
        }

        public void StopListeningForPlayerMatch(string playerMatchId)
        {
            string path = $"{PATH}/{playerMatchId}";
            _firebaseService.StopListening(path);
        }

        public void StopListeningForMatchPlayers(string matchId)
        {
            string path = $"{MATCH_INDEX}/{matchId}";
            _firebaseService.StopListening(path);
        }

        public string GeneratePlayerMatchId()
        {
            return _firebaseService.GenerateId(PATH);
        }
    }
}