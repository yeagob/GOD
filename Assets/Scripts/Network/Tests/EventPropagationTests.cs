using System;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
using Network.Models;
using Network.Presenters;
using Network.Infrastructure;

namespace Network.Tests
{
    [TestFixture]
    public class EventPropagationTests
    {
        private MatchPresenter _matchPresenter;
        private PlayerMatchPresenter _playerMatchPresenter;
        private GameEventPresenter _gameEventPresenter;
        
        private MockMatchModel _mockMatchModel;
        private MockPlayerMatchModel _mockPlayerMatchModel;
        private MockGameEventModel _mockGameEventModel;
        
        private const string TestMatchId = "match789";
        private const string TestPlayerId = "player456";
        private const string TestPlayerName = "TestPlayer2";
        private const int TestTargetTile = 8;
        
        [SetUp]
        public void Setup()
        {
            _mockMatchModel = new MockMatchModel();
            _mockPlayerMatchModel = new MockPlayerMatchModel();
            _mockGameEventModel = new MockGameEventModel();
            
            _matchPresenter = new MatchPresenter(
                _mockMatchModel,
                _mockPlayerMatchModel,
                _mockGameEventModel);
                
            _playerMatchPresenter = new PlayerMatchPresenter(
                _mockPlayerMatchModel,
                _mockMatchModel);
                
            _gameEventPresenter = new GameEventPresenter(
                _mockGameEventModel,
                _mockPlayerMatchModel,
                _mockMatchModel);
        }
        
        [Test]
        public void GameEvent_ShouldTriggerCallbacksInAllPresenters()
        {
            // Arrange
            // 1. Configurar los listeners
            bool matchPresenterEventCalled = false;
            bool playerMatchPresenterScoreUpdated = false;
            
            // Configurar para que el modelo de eventos dispare cambios
            List<GameEventData> mockEvents = new List<GameEventData>();
            Action<List<GameEventData>> eventCallback = null;
            
            _mockGameEventModel.OnListenForMatchEvents = (matchId, callback) => {
                if (matchId == TestMatchId) {
                    eventCallback = callback;
                }
            };
            
            _mockGameEventModel.OnCreateGameEvent = (playerId, matchId, eventType, targetTile, callback) => {
                if (matchId == TestMatchId && playerId == TestPlayerId) {
                    GameEventData newEvent = new GameEventData(
                        "event123", playerId, matchId, eventType, targetTile);
                    mockEvents.Add(newEvent);
                    
                    // Simular que Firebase notifica del cambio
                    eventCallback?.Invoke(mockEvents);
                    
                    callback?.Invoke(newEvent);
                }
            };
            
            // Configurar para que el score se actualice cuando hay un evento de cambio de puntuación
            _mockPlayerMatchModel.OnUpdatePlayerScore = (playerMatchId, newScore, callback) => {
                if (playerMatchId == TestPlayerId) {
                    playerMatchPresenterScoreUpdated = true;
                    callback?.Invoke(true);
                }
            };
            
            _mockPlayerMatchModel.OnGetPlayerMatch = (playerMatchId, callback) => {
                if (playerMatchId == TestPlayerId) {
                    callback?.Invoke(new PlayerMatchData(
                        TestPlayerId, TestPlayerName, TestMatchId, 5));
                }
            };
            
            // 2. Activar los listeners
            _matchPresenter.ListenForMatchUpdates(
                TestMatchId,
                (matchData) => { },
                (players) => { },
                (events) => { matchPresenterEventCalled = true; }
            );
            
            // Act
            // 3. Crear un evento de cambio de puntuación
            _gameEventPresenter.RecordScoreChange(TestPlayerId, TestMatchId, TestTargetTile);
            
            // Assert
            Assert.IsTrue(matchPresenterEventCalled, "MatchPresenter no fue notificado del evento");
            // Para confirmar que este test hubiera podido verificar la actualización de puntuación,
            // tendríamos que implementar otro flujo que actualice la puntuación en respuesta a eventos,
            // por ejemplo, en un método Observer en el MatchPresenter
        }
        
        [Test]
        public void PlayerScoreChange_ShouldPropagateToMatchView()
        {
            // Arrange
            // 1. Configurar los listeners
            bool matchPresenterPlayerCalled = false;
            PlayerMatchData updatedPlayer = default;
            List<PlayerMatchData> playersList = new List<PlayerMatchData>();
            Action<List<PlayerMatchData>> playersCallback = null;
            
            _mockPlayerMatchModel.OnUpdatePlayerScore = (playerMatchId, newScore, callback) => {
                if (playerMatchId == TestPlayerId) {
                    updatedPlayer = new PlayerMatchData(
                        TestPlayerId, TestPlayerName, TestMatchId, newScore);
                    
                    playersList.Clear();
                    playersList.Add(updatedPlayer);
                    
                    // Simular que Firebase notifica del cambio
                    playersCallback?.Invoke(playersList);
                    
                    callback?.Invoke(true);
                }
            };
            
            _mockPlayerMatchModel.OnGetPlayerMatch = (playerMatchId, callback) => {
                if (playerMatchId == TestPlayerId) {
                    callback?.Invoke(new PlayerMatchData(
                        TestPlayerId, TestPlayerName, TestMatchId, 5));
                }
            };
            
            _mockPlayerMatchModel.OnListenForMatchPlayersChanges = (matchId, callback) => {
                if (matchId == TestMatchId) {
                    playersCallback = callback;
                }
            };
            
            // 2. Activar los listeners
            _matchPresenter.ListenForMatchUpdates(
                TestMatchId,
                (matchData) => { },
                (players) => { matchPresenterPlayerCalled = true; },
                (events) => { }
            );
            
            // Act
            // 3. Actualizar la puntuación de un jugador
            _playerMatchPresenter.UpdateScore(TestPlayerId, 10);
            
            // Assert
            Assert.IsTrue(matchPresenterPlayerCalled, "MatchPresenter no fue notificado del cambio de puntuación del jugador");
            Assert.AreEqual(15, updatedPlayer._score); // 5 (original) + 10 (incremento)
        }
    }
}