using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using Moq;
using Network;

namespace Tests
{
    public class FirebaseDatabaseServiceTests
    {
        private FirebaseDatabaseService service;
        private GameObject testGameObject;

        [SetUp]
        public void Setup()
        {
            testGameObject = new GameObject();
            service = testGameObject.AddComponent<FirebaseDatabaseService>();
        }

        [TearDown]
        public void TearDown()
        {
            if (testGameObject != null)
            {
                Object.DestroyImmediate(testGameObject);
            }
        }

        [Test]
        public void CreateMatch_WithValidData_CallsCallback()
        {
            var matchData = new MatchData
            {
                MatchId = "test-match",
                MaxPlayers = 4,
                State = 0
            };

            bool callbackCalled = false;
            string resultMatchId = null;

            service.CreateMatch(matchData, (success, matchId) =>
            {
                callbackCalled = true;
                resultMatchId = matchId;
            });

            Assert.IsTrue(callbackCalled);
        }

        [Test]
        public void JoinMatch_WithValidPlayer_CallsCallback()
        {
            var playerData = new PlayerMatchData
            {
                PlayerId = "test-player",
                PlayerName = "TestPlayer",
                Score = 0
            };

            bool callbackCalled = false;
            bool result = false;

            service.JoinMatch("test-match", playerData, (success) =>
            {
                callbackCalled = true;
                result = success;
            });

            Assert.IsTrue(callbackCalled);
        }

        [Test]
        public void AddGameEvent_WithValidEvent_CallsCallback()
        {
            var gameEvent = new GameEventData
            {
                EventType = GameEventType.PlayerMoved,
                PlayerId = "test-player",
                Timestamp = System.DateTime.Now.Ticks,
                Data = "test-data"
            };

            bool callbackCalled = false;

            service.AddGameEvent("test-match", gameEvent, (success) =>
            {
                callbackCalled = true;
            });

            Assert.IsTrue(callbackCalled);
        }

        [Test]
        public void UpdatePlayerScore_WithValidData_CallsCallback()
        {
            bool callbackCalled = false;

            service.UpdatePlayerScore("test-match", "test-player", 100, (success) =>
            {
                callbackCalled = true;
            });

            Assert.IsTrue(callbackCalled);
        }

        [Test]
        public void UpdateMatchState_WithValidState_CallsCallback()
        {
            bool callbackCalled = false;

            service.UpdateMatchState("test-match", 1, (success) =>
            {
                callbackCalled = true;
            });

            Assert.IsTrue(callbackCalled);
        }

        [Test]
        public void RemoveMatch_WithValidMatchId_CallsCallback()
        {
            bool callbackCalled = false;

            service.RemoveMatch("test-match", (success) =>
            {
                callbackCalled = true;
            });

            Assert.IsTrue(callbackCalled);
        }

        [Test]
        public void RemovePlayerFromMatch_WithValidData_CallsCallback()
        {
            bool callbackCalled = false;

            service.RemovePlayerFromMatch("test-match", "test-player", (success) =>
            {
                callbackCalled = true;
            });

            Assert.IsTrue(callbackCalled);
        }

        [UnityTest]
        public IEnumerator ListenForAvailableMatches_RegistersListener()
        {
            bool listenerCalled = false;
            List<MatchData> receivedMatches = null;

            service.ListenForAvailableMatches((matches) =>
            {
                listenerCalled = true;
                receivedMatches = matches;
            });

            yield return new WaitForSeconds(0.1f);

            service.StopListeningForAvailableMatches();
            
            Assert.IsNotNull(receivedMatches);
        }

        [UnityTest]
        public IEnumerator ListenForPlayersInMatch_RegistersListener()
        {
            bool listenerCalled = false;
            List<PlayerMatchData> receivedPlayers = null;

            service.ListenForPlayersInMatch("test-match", (players) =>
            {
                listenerCalled = true;
                receivedPlayers = players;
            });

            yield return new WaitForSeconds(0.1f);

            service.StopListeningForPlayersInMatch("test-match");
            
            Assert.IsNotNull(receivedPlayers);
        }
    }
}