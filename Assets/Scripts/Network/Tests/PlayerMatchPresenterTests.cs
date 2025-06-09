using System;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
using Network.Models;
using Network.Presenters;

namespace Network.Tests
{
    [TestFixture]
    public class PlayerMatchPresenterTests
    {
        private PlayerMatchPresenter _presenter;
        private MockPlayerMatchModel _mockPlayerMatchModel;
        private MockMatchModel _mockMatchModel;
        
        private const string TestPlayerMatchId = "player123";
        private const string TestMatchId = "match456";
        private const string TestPlayerName = "TestPlayer";
        
        [SetUp]
        public void Setup()
        {
            _mockPlayerMatchModel = new MockPlayerMatchModel();
            _mockMatchModel = new MockMatchModel();
            
            _presenter = new PlayerMatchPresenter(
                _mockPlayerMatchModel,
                _mockMatchModel);
        }
        
        [Test]
        public void UpdateScore_ShouldIncrementCurrentScore()
        {
            // Arrange
            string queriedPlayerId = null;
            string updatedPlayerId = null;
            int updatedScore = -1;
            
            PlayerMatchData existingPlayer = new PlayerMatchData(TestPlayerMatchId, TestPlayerName, TestMatchId, 10);
            
            _mockPlayerMatchModel.OnGetPlayerMatch = (playerMatchId, callback) => {
                queriedPlayerId = playerMatchId;
                callback?.Invoke(existingPlayer);
            };
            
            _mockPlayerMatchModel.OnUpdatePlayerScore = (playerMatchId, newScore, callback) => {
                updatedPlayerId = playerMatchId;
                updatedScore = newScore;
                callback?.Invoke(true);
            };
            
            // Act
            bool result = false;
            _presenter.UpdateScore(TestPlayerMatchId, 5, (success) => {
                result = success;
            });
            
            // Assert
            Assert.AreEqual(TestPlayerMatchId, queriedPlayerId);
            Assert.AreEqual(TestPlayerMatchId, updatedPlayerId);
            Assert.AreEqual(15, updatedScore); // 10 (existente) + 5 (incremento)
            Assert.IsTrue(result);
        }
        
        [Test]
        public void UpdateScore_ShouldHandleNegativeScoreChanges()
        {
            // Arrange
            string queriedPlayerId = null;
            string updatedPlayerId = null;
            int updatedScore = -1;
            
            PlayerMatchData existingPlayer = new PlayerMatchData(TestPlayerMatchId, TestPlayerName, TestMatchId, 10);
            
            _mockPlayerMatchModel.OnGetPlayerMatch = (playerMatchId, callback) => {
                queriedPlayerId = playerMatchId;
                callback?.Invoke(existingPlayer);
            };
            
            _mockPlayerMatchModel.OnUpdatePlayerScore = (playerMatchId, newScore, callback) => {
                updatedPlayerId = playerMatchId;
                updatedScore = newScore;
                callback?.Invoke(true);
            };
            
            // Act
            bool result = false;
            _presenter.UpdateScore(TestPlayerMatchId, -3, (success) => {
                result = success;
            });
            
            // Assert
            Assert.AreEqual(TestPlayerMatchId, queriedPlayerId);
            Assert.AreEqual(TestPlayerMatchId, updatedPlayerId);
            Assert.AreEqual(7, updatedScore); // 10 (existente) - 3 (decremento)
            Assert.IsTrue(result);
        }
        
        [Test]
        public void GetPlayerInfo_ShouldRetrievePlayerFromModel()
        {
            // Arrange
            string queriedPlayerId = null;
            PlayerMatchData existingPlayer = new PlayerMatchData(TestPlayerMatchId, TestPlayerName, TestMatchId, 10);
            
            _mockPlayerMatchModel.OnGetPlayerMatch = (playerMatchId, callback) => {
                queriedPlayerId = playerMatchId;
                callback?.Invoke(existingPlayer);
            };
            
            // Act
            PlayerMatchData result = default;
            _presenter.GetPlayerInfo(TestPlayerMatchId, (playerData) => {
                result = playerData;
            });
            
            // Assert
            Assert.AreEqual(TestPlayerMatchId, queriedPlayerId);
            Assert.AreEqual(TestPlayerMatchId, result._id);
            Assert.AreEqual(TestPlayerName, result._name);
            Assert.AreEqual(TestMatchId, result._matchId);
            Assert.AreEqual(10, result._tile);
        }
        
        [Test]
        public void GetMatchPlayers_ShouldRetrievePlayersFromModel()
        {
            // Arrange
            string queriedMatchId = null;
            var players = new List<PlayerMatchData> {
                new PlayerMatchData("p1", "Player1", TestMatchId, 10),
                new PlayerMatchData("p2", "Player2", TestMatchId, 15)
            };
            
            _mockPlayerMatchModel.OnGetPlayersByMatch = (matchId, callback) => {
                queriedMatchId = matchId;
                callback?.Invoke(players);
            };
            
            // Act
            List<PlayerMatchData> result = null;
            _presenter.GetMatchPlayers(TestMatchId, (playerList) => {
                result = playerList;
            });
            
            // Assert
            Assert.AreEqual(TestMatchId, queriedMatchId);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Player1", result[0]._name);
            Assert.AreEqual("Player2", result[1]._name);
        }
        
        [Test]
        public void ListenForPlayerUpdates_ShouldSetUpListenerInModel()
        {
            // Arrange
            string listenedPlayerId = null;
            Action<PlayerMatchData> registeredCallback = null;
            
            _mockPlayerMatchModel.OnListenForPlayerMatchChanges = (playerMatchId, callback) => {
                listenedPlayerId = playerMatchId;
                registeredCallback = callback;
            };
            
            // Act
            _presenter.ListenForPlayerUpdates(TestPlayerMatchId, (data) => { });
            
            // Assert
            Assert.AreEqual(TestPlayerMatchId, listenedPlayerId);
            Assert.IsNotNull(registeredCallback);
        }
        
        [Test]
        public void ListenForMatchPlayersUpdates_ShouldSetUpListenerInModel()
        {
            // Arrange
            string listenedMatchId = null;
            Action<List<PlayerMatchData>> registeredCallback = null;
            
            _mockPlayerMatchModel.OnListenForMatchPlayersChanges = (matchId, callback) => {
                listenedMatchId = matchId;
                registeredCallback = callback;
            };
            
            // Act
            _presenter.ListenForMatchPlayersUpdates(TestMatchId, (players) => { });
            
            // Assert
            Assert.AreEqual(TestMatchId, listenedMatchId);
            Assert.IsNotNull(registeredCallback);
        }
    }
}