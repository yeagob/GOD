using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Network
{
    public class MatchPresenter : IDisposable
    {
        private readonly IFirebaseDatabaseService _databaseService;
        private string _matchId;
        private string _localPlayerId; // ID del jugador en este cliente

        private MatchData _currentMatchData;
        private Dictionary<string, PlayerMatchData> _playersInMatch = new Dictionary<string, PlayerMatchData>();

        public event Action<MatchData> OnMatchDataUpdated;
        public event Action<List<PlayerMatchData>> OnPlayersInMatchUpdated;
        public event Action<GameEventData> OnNewGameEvent;
        public event Action OnMatchEnded;

        public MatchData CurrentMatchData => _currentMatchData;
        public List<PlayerMatchData> PlayersInMatch => _playersInMatch.Values.ToList();
        public string LocalPlayerId => _localPlayerId;


        public MatchPresenter(IFirebaseDatabaseService databaseService, string matchId, string localPlayerId)
        {
            _databaseService = databaseService;
            _matchId = matchId;
            _localPlayerId = localPlayerId; 
            StartListeningForMatchUpdates();
            StartListeningForPlayerUpdates();
            StartListeningForEvents();
        }

        private void StartListeningForMatchUpdates()
        {
            _databaseService.ListenForMatchStateChanges(_matchId, (matchData) =>
            {
                _currentMatchData = matchData;
                OnMatchDataUpdated?.Invoke(_currentMatchData);
                if (_currentMatchData._state == (int)GameEventType.GAME_END)
                {
                    OnMatchEnded?.Invoke();
                    Dispose(); 
                }
            });
        }

        private void StartListeningForPlayerUpdates()
        {
            _databaseService.ListenForPlayersInMatch(_matchId, (playersList) =>
            {
                _playersInMatch.Clear();
                foreach (var player in playersList)
                {
                    _playersInMatch[player._id] = player;
                }
                OnPlayersInMatchUpdated?.Invoke(playersList);
            });
        }

        private void StartListeningForEvents()
        {
            _databaseService.ListenForNewEventsInMatch(_matchId, (newEvent) =>
            {
                OnNewGameEvent?.Invoke(newEvent);
                ProcessGameEvent(newEvent); 
            });
        }

        private void ProcessGameEvent(GameEventData gameEvent)
        {
            Debug.Log($"Processing Event: Type={gameEvent._eventType}, Player={gameEvent._playerId}, Tile={gameEvent._targetTile}");
            switch ((GameEventType)gameEvent._eventType)
            {
                case GameEventType.MOVE:
                    Debug.Log($"Player {gameEvent._playerId} moved to tile {gameEvent._targetTile}");
                    break;
                case GameEventType.JUMP:
                    Debug.Log($"Player {gameEvent._playerId} jumped to tile {gameEvent._targetTile}");
                    break;
                case GameEventType.SCORE_CHANGE:
                    if (_playersInMatch.TryGetValue(gameEvent._playerId, out PlayerMatchData player))
                    {
                     
                    }
                    break;
                case GameEventType.GAME_END:
                    Debug.Log("Game has ended.");
                    
                    break;
                // Handle other event types
            }
        }


        // --- Methods called by the View/Game Logic to send events ---

        public void SendMoveEvent(string id, string playerId, string matchId, int eventType, int targetTile)
        {
            if ( _currentMatchData._state != 1) return; // Only send if match is in progress

            GameEventData moveEvent = new GameEventData(id, playerId, matchId, eventType, targetTile);
            _databaseService.AddGameEvent(_matchId, moveEvent, (success) =>
            {
                if (!success) Debug.LogError("Failed to send move event.");
            });
        }

        public void SendJumpEvent(int targetTile)
        {
            if (_currentMatchData._state != 1)
            {
                return;
            }

            GameEventData jumpEvent = new GameEventData();
            /*{
                _playerId = _localPlayerId,
                _eventType = (int)GameEventType.JUMP,
                _targetTile = targetTile
            };*/
            _databaseService.AddGameEvent(_matchId, jumpEvent, (success) =>
            {
                if (!success) Debug.LogError("Failed to send jump event.");
            });
        }

        public void SendScoreChangeEvent(int scoreChangeAmount)
        {
            if ( _currentMatchData._state != 1)
            {
                return;
            }

            GameEventData scoreEvent = new GameEventData();
            /*{
                _playerId = _localPlayerId,
                _eventType = (int)GameEventType.SCORE_CHANGE,
                _targetTile = scoreChangeAmount // Using target_tile to carry the score change value
            };*/
            _databaseService.AddGameEvent(_matchId, scoreEvent, (success) =>
            {
                if (!success) Debug.LogError("Failed to send score change event.");
            });
        }

        public void EndMatch()
        {
            if (_currentMatchData._state != 1)
            {
                return;
            }

            _databaseService.UpdateMatchState(_matchId, (int)GameEventType.GAME_END, (success) =>
            {
                if (!success) Debug.LogError("Failed to end match.");
            });

            // Alternatively, send a GAME_END event if events are the source of truth
            /*
         GameEventData endEvent = new GameEventData
         {
             _playerId = _localPlayerId, // Player who triggered the end (if applicable)
             _eventType = (int)GameEventType.GAME_END,
             _targetTile = -1 // No target tile for end event
         };
         _databaseService.AddGameEvent(_matchId, endEvent, (success) =>
         {
             if (!success) Debug.LogError("Failed to send game end event.");
         });
         */
        }


        public void Dispose()
        {
            _databaseService.StopListeningForMatchStateChanges(_matchId);
            _databaseService.StopListeningForPlayersInMatch(_matchId);
            _databaseService.StopListeningForNewEventsInMatch(_matchId);
            // Clean up other resources
        }
    }
}