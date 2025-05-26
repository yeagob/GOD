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
    public class GameEventPresenterTests
    {
        private GameEventPresenter _presenter;
        private MockGameEventModel _mockGameEventModel;
        private MockPlayerMatchModel _mockPlayerMatchModel;
        private MockMatchModel _mockMatchModel;
        
        private const string TestPlayerId = "player123";
        private const string TestMatchId = "match456";
        private const int TestTargetTile = 5;
        
        [SetUp]
        public void Setup()
        {
            _mockGameEventModel = new MockGameEventModel();
            _mockPlayerMatchModel = new MockPlayerMatchModel();
            _mockMatchModel = new MockMatchModel();
            
            _presenter = new GameEventPresenter(
                _mockGameEventModel,
                _mockPlayerMatchModel,
                _mockMatchModel);
        }
        
        [Test]
        public void RecordMovement_ShouldCreateEventWithCorrectType()
        {
            // Arrange
            GameEventData createdEvent = default;
            _mockGameEventModel.OnCreateGameEvent = (playerId, matchId, eventType, targetTile, callback) => {
                createdEvent = new GameEventData("mockEvent", playerId, matchId, eventType, targetTile);
                callback?.Invoke(createdEvent);
            };
            
            // Act
            GameEventData resultEvent = default;
            _presenter.RecordMovement(TestPlayerId, TestMatchId, TestTargetTile, (result) => {
                resultEvent = result;
            });
            
            // Assert
            Assert.AreEqual(NetworkConstants.EVENT_TYPE_MOVE, createdEvent._eventType);
            Assert.AreEqual(TestPlayerId, createdEvent._playerId);
            Assert.AreEqual(TestMatchId, createdEvent._matchId);
            Assert.AreEqual(TestTargetTile, createdEvent._targetTile);
            
            Assert.AreEqual(NetworkConstants.EVENT_TYPE_MOVE, resultEvent._eventType);
        }
        
        [Test]
        public void RecordJump_ShouldCreateEventWithCorrectType()
        {
            // Arrange
            GameEventData createdEvent = default;
            _mockGameEventModel.OnCreateGameEvent = (playerId, matchId, eventType, targetTile, callback) => {
                createdEvent = new GameEventData("mockEvent", playerId, matchId, eventType, targetTile);
                callback?.Invoke(createdEvent);
            };
            
            // Act
            GameEventData resultEvent = default;
            _presenter.RecordJump(TestPlayerId, TestMatchId, TestTargetTile, (result) => {
                resultEvent = result;
            });
            
            // Assert
            Assert.AreEqual(NetworkConstants.EVENT_TYPE_JUMP, createdEvent._eventType);
            Assert.AreEqual(TestPlayerId, createdEvent._playerId);
            Assert.AreEqual(TestMatchId, createdEvent._matchId);
            Assert.AreEqual(TestTargetTile, createdEvent._targetTile);
            
            Assert.AreEqual(NetworkConstants.EVENT_TYPE_JUMP, resultEvent._eventType);
        }
        
        [Test]
        public void RecordQuestion_ShouldCreateEventWithCorrectType()
        {
            // Arrange
            GameEventData createdEvent = default;
            _mockGameEventModel.OnCreateGameEvent = (playerId, matchId, eventType, targetTile, callback) => {
                createdEvent = new GameEventData("mockEvent", playerId, matchId, eventType, targetTile);
                callback?.Invoke(createdEvent);
            };
            
            // Act
            GameEventData resultEvent = default;
            _presenter.RecordQuestion(TestPlayerId, TestMatchId, TestTargetTile, (result) => {
                resultEvent = result;
            });
            
            // Assert
            Assert.AreEqual(NetworkConstants.EVENT_TYPE_QUESTION, createdEvent._eventType);
            Assert.AreEqual(TestPlayerId, createdEvent._playerId);
            Assert.AreEqual(TestMatchId, createdEvent._matchId);
            Assert.AreEqual(TestTargetTile, createdEvent._targetTile);
            
            Assert.AreEqual(NetworkConstants.EVENT_TYPE_QUESTION, resultEvent._eventType);
        }
        
        [Test]
        public void RecordScoreChange_ShouldCreateEventWithCorrectType()
        {
            // Arrange
            GameEventData createdEvent = default;
            _mockGameEventModel.OnCreateGameEvent = (playerId, matchId, eventType, targetTile, callback) => {
                createdEvent = new GameEventData("mockEvent", playerId, matchId, eventType, targetTile);
                callback?.Invoke(createdEvent);
            };
            
            // Act
            GameEventData resultEvent = default;
            _presenter.RecordScoreChange(TestPlayerId, TestMatchId, TestTargetTile, (result) => {
                resultEvent = result;
            });
            
            // Assert
            Assert.AreEqual(NetworkConstants.EVENT_TYPE_SCORE_CHANGE, createdEvent._eventType);
            Assert.AreEqual(TestPlayerId, createdEvent._playerId);
            Assert.AreEqual(TestMatchId, createdEvent._matchId);
            Assert.AreEqual(TestTargetTile, createdEvent._targetTile);
            
            Assert.AreEqual(NetworkConstants.EVENT_TYPE_SCORE_CHANGE, resultEvent._eventType);
        }
        
        [Test]
        public void RecordReview_ShouldCreateEventWithCorrectType()
        {
            // Arrange
            GameEventData createdEvent = default;
            _mockGameEventModel.OnCreateGameEvent = (playerId, matchId, eventType, targetTile, callback) => {
                createdEvent = new GameEventData("mockEvent", playerId, matchId, eventType, targetTile);
                callback?.Invoke(createdEvent);
            };
            
            // Act
            GameEventData resultEvent = default;
            _presenter.RecordReview(TestPlayerId, TestMatchId, TestTargetTile, (result) => {
                resultEvent = result;
            });
            
            // Assert
            Assert.AreEqual(NetworkConstants.EVENT_TYPE_REVIEW, createdEvent._eventType);
            Assert.AreEqual(TestPlayerId, createdEvent._playerId);
            Assert.AreEqual(TestMatchId, createdEvent._matchId);
            Assert.AreEqual(TestTargetTile, createdEvent._targetTile);
            
            Assert.AreEqual(NetworkConstants.EVENT_TYPE_REVIEW, resultEvent._eventType);
        }
        
        [Test]
        public void GetMatchTimeline_ShouldRetrieveEventsFromModel()
        {
            // Arrange
            string requestedMatchId = null;
            var mockEvents = new List<GameEventData> {
                new GameEventData("event1", TestPlayerId, TestMatchId, NetworkConstants.EVENT_TYPE_MOVE, 1),
                new GameEventData("event2", TestPlayerId, TestMatchId, NetworkConstants.EVENT_TYPE_JUMP, 2)
            };
            
            _mockGameEventModel.OnGetEventsByMatch = (matchId, callback) => {
                requestedMatchId = matchId;
                callback?.Invoke(mockEvents);
            };
            
            // Act
            List<GameEventData> resultEvents = null;
            _presenter.GetMatchTimeline(TestMatchId, (events) => {
                resultEvents = events;
            });
            
            // Assert
            Assert.AreEqual(TestMatchId, requestedMatchId);
            Assert.IsNotNull(resultEvents);
            Assert.AreEqual(2, resultEvents.Count);
            Assert.AreEqual("event1", resultEvents[0]._id);
            Assert.AreEqual("event2", resultEvents[1]._id);
        }
        
        [Test]
        public void ListenForMatchEvents_ShouldSetUpListenerInModel()
        {
            // Arrange
            string listenedMatchId = null;
            Action<List<GameEventData>> registeredCallback = null;
            
            _mockGameEventModel.OnListenForMatchEvents = (matchId, callback) => {
                listenedMatchId = matchId;
                registeredCallback = callback;
            };
            
            // Act
            _presenter.ListenForMatchEvents(TestMatchId, (events) => { });
            
            // Assert
            Assert.AreEqual(TestMatchId, listenedMatchId);
            Assert.IsNotNull(registeredCallback);
        }
    }
}