using System;
using System.Collections.Generic;
using UnityEngine;
using Network.Services;

namespace Network.Repositories
{
    public class GameEventRepository : IGameEventRepository
    {
        private readonly IFirebaseService _firebaseService;
        private const string PATH = "game_events";
        private const string MATCH_INDEX = "match_events_index";
        private const string PLAYER_INDEX = "player_events_index";

        public GameEventRepository(IFirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
        }

        public void CreateGameEvent(GameEventData gameEventData, Action<bool> callback = null)
        {
            string path = $"{PATH}/{gameEventData._id}";
            _firebaseService.SetData(path, gameEventData, result => {
                if (result)
                {
                    string matchIndexPath = $"{MATCH_INDEX}/{gameEventData._matchId}/{gameEventData._id}";
                    _firebaseService.SetData(matchIndexPath, true, matchIndexResult => {
                        if (matchIndexResult)
                        {
                            string playerIndexPath = $"{PLAYER_INDEX}/{gameEventData._playerId}/{gameEventData._id}";
                            _firebaseService.SetData(playerIndexPath, true, playerIndexResult => {
                                callback?.Invoke(playerIndexResult);
                            });
                        }
                        else
                        {
                            callback?.Invoke(false);
                        }
                    });
                }
                else
                {
                    callback?.Invoke(false);
                }
            });
        }

        public void GetGameEventsByMatchId(string matchId, Action<List<GameEventData>> callback)
        {
            callback?.Invoke(new List<GameEventData>());
        }

        public void GetGameEventsByPlayerId(string playerId, Action<List<GameEventData>> callback)
        {
            callback?.Invoke(new List<GameEventData>());
        }

        public void ListenForMatchEvents(string matchId, Action<List<GameEventData>> callback)
        {
            string path = $"{MATCH_INDEX}/{matchId}";
            
            callback?.Invoke(new List<GameEventData>());
        }

        public void StopListeningForMatchEvents(string matchId)
        {
            string path = $"{MATCH_INDEX}/{matchId}";
            _firebaseService.StopListening(path);
        }

        public string GenerateGameEventId()
        {
            return _firebaseService.GenerateId(PATH);
        }
    }
}
