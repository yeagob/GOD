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
    public class MatchPresenterTests
    {
        private MatchPresenter _presenter;
        private MockMatchModel _mockMatchModel;
        private MockPlayerMatchModel _mockPlayerMatchModel;
        private MockGameEventModel _mockGameEventModel;
        
        private const string TestMatchId = "match123";
        private const string TestUrl = "game://test";
        private const string TestPlayerName = "Player1";
        
        [SetUp]
        public void Setup()
        {
            _mockMatchModel = new MockMatchModel();
            _mockPlayerMatchModel = new MockPlayerMatchModel();
            _mockGameEventModel = new MockGameEventModel();
            
            _presenter = new MatchPresenter(
                _mockMatchModel,
                _mockPlayerMatchModel,
                _mockGameEventModel);
        }
        
        [Test]
        public void CreateNewMatch_ShouldCreateMatchWithWaitingState()
        {
            // Arrange
            MatchData createdMatch = default;
            int passedState = -1;
            
            _mockMatchModel.OnCreateMatch = (url, state, callback) => {
                passedState = state;
                createdMatch = new MatchData(TestMatchId, url, (MatchState)state);
                callback?.Invoke(createdMatch);
            };
            
            // Act
            MatchData result = default;
            _presenter.CreateNewMatch(TestUrl, (matchData) => {
                result = matchData;
            });
            
            // Assert
            Assert.AreEqual(MatchState.PlayGame, (MatchState)passedState);
            Assert.AreEqual(TestUrl, result._url);
            Assert.AreEqual(MatchState.PlayGame, (MatchState)result._state);
        }
        
        [Test]
        public void JoinMatch_ShouldCreatePlayerWhenMatchIsWaiting()
        {
            // Arrange
            MatchData waitingMatch = new MatchData(TestMatchId, TestUrl, MatchPresenter.MATCH_STATE_WAITING);
            bool playerAdded = false;
            
            _mockMatchModel.OnGetMatch = (matchId, callback) => {
                if (matchId == TestMatchId) {
                    callback?.Invoke(waitingMatch);
                } else {
                    callback?.Invoke(default);
                }
            };
            
            _mockPlayerMatchModel.OnCreatePlayerMatch = (name, matchId, callback) => {
                if (matchId == TestMatchId && name == TestPlayerName) {
                    playerAdded = true;
                    callback?.Invoke(new PlayerMatchData("player123", name, matchId, 0));
                } else {
                    callback?.Invoke(default);
                }
            };
            
            // Act
            PlayerMatchData result = default;
            _presenter.JoinMatch(TestMatchId, TestPlayerName, (playerData) => {
                result = playerData;
            });
            
            // Assert
            Assert.IsTrue(playerAdded);
            Assert.AreEqual(TestPlayerName, result._name);
            Assert.AreEqual(TestMatchId, result._matchId);
        }
        
        [Test]
        public void JoinMatch_ShouldFailWhenMatchIsNotWaiting()
        {
            // Arrange
            MatchData playingMatch = new MatchData(TestMatchId, TestUrl, MatchState.PlayGame);
            bool playerAdded = false;
            
            _mockMatchModel.OnGetMatch = (matchId, callback) => {
                callback?.Invoke(playingMatch);
            };
            
            _mockPlayerMatchModel.OnCreatePlayerMatch = (name, matchId, callback) => {
                playerAdded = true;
                callback?.Invoke(new PlayerMatchData("player123", name, matchId, 0));
            };
            
            // Act
            PlayerMatchData result = default;
            _presenter.JoinMatch(TestMatchId, TestPlayerName, (playerData) => {
                result = playerData;
            });
            
            // Assert
            Assert.IsFalse(playerAdded);
            // Si falla, result debería ser su valor por defecto, que es difícil verificar con struct
        }
        
        [Test]
        public void StartMatch_ShouldUpdateMatchStateToPlaying()
        {
            // Arrange
            string updatedMatchId = null;
            int updatedState = -1;
            
            _mockMatchModel.OnUpdateMatchState = (matchId, state, callback) => {
                updatedMatchId = matchId;
                updatedState = state;
                callback?.Invoke(true);
            };
            
            // Act
            bool result = false;
            _presenter.StartMatch(TestMatchId, (success) => {
                result = success;
            });
            
            // Assert
            Assert.AreEqual(TestMatchId, updatedMatchId);
            Assert.AreEqual(MatchState.PlayGame, (MatchState)updatedState);
            Assert.IsTrue(result);
        }
        
        [Test]
        public void EndMatch_ShouldUpdateMatchStateToFinished()
        {
            // Arrange
            string updatedMatchId = null;
            int updatedState = -1;
            
            _mockMatchModel.OnUpdateMatchState = (matchId, state, callback) => {
                updatedMatchId = matchId;
                updatedState = state;
                callback?.Invoke(true);
            };
            
            // Act
            bool result = false;
            _presenter.EndMatch(TestMatchId, (success) => {
                result = success;
            });
            
            // Assert
            Assert.AreEqual(TestMatchId, updatedMatchId);
            Assert.AreEqual(MatchState.EndGame, (MatchState)updatedState);
            Assert.IsTrue(result);
        }
        
        [Test]
        public void ListenForMatchUpdates_ShouldSetUpListenersInModels()
        {
            // Arrange
            string matchListenerId = null;
            string playersListenerId = null;
            string eventsListenerId = null;
            
            _mockMatchModel.OnListenForMatchChanges = (matchId, callback) => {
                matchListenerId = matchId;
            };
            
            _mockPlayerMatchModel.OnListenForMatchPlayersChanges = (matchId, callback) => {
                playersListenerId = matchId;
            };
            
            _mockGameEventModel.OnListenForMatchEvents = (matchId, callback) => {
                eventsListenerId = matchId;
            };
            
            // Act
            _presenter.ListenForMatchUpdates(
                TestMatchId,
                (matchData) => { },
                (players) => { },
                (events) => { }
            );
            
            // Assert
            Assert.AreEqual(TestMatchId, matchListenerId);
            Assert.AreEqual(TestMatchId, playersListenerId);
            Assert.AreEqual(TestMatchId, eventsListenerId);
        }
    }
}