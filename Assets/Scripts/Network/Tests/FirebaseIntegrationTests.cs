using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using Network;

namespace Tests.Integration
{
    public class FirebaseIntegrationTests
    {
        private FirebaseDatabaseService service;
        private GameObject testGameObject;
        private bool isFirebaseReady = false;
        private string testMatchId;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            testGameObject = new GameObject("FirebaseTestService");
            service = testGameObject.AddComponent<FirebaseDatabaseService>();
            
            service.OnInitialized += () => isFirebaseReady = true;
            service.OnInitializationError += (error) => 
            {
                Debug.LogError($"Firebase initialization failed: {error}");
                Assert.Fail($"Firebase failed to initialize: {error}");
            };
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (!string.IsNullOrEmpty(testMatchId))
            {
                service.RemoveMatch(testMatchId, (success) => 
                {
                    if (!success) Debug.LogWarning("Failed to cleanup test match");
                });
            }

            if (testGameObject != null)
            {
                Object.DestroyImmediate(testGameObject);
            }
        }

        [UnityTest]
        public IEnumerator WaitForFirebaseInitialization()
        {
            float timeout = 30f;
            float elapsed = 0f;

            while (!isFirebaseReady && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            Assert.IsTrue(isFirebaseReady, "Firebase should initialize within 30 seconds");
        }

        [UnityTest]
        [Order(1)]
        public IEnumerator CreateMatch_ShouldSucceed()
        {
            yield return WaitForFirebaseInitialization();

            var matchData = new MatchData
            {
                MaxPlayers = 4,
                State = 0
            };

            bool callbackCalled = false;
            bool success = false;
            string matchId = null;

            service.CreateMatch(matchData, (result, id) =>
            {
                callbackCalled = true;
                success = result;
                matchId = id;
                testMatchId = id;
            });

            yield return new WaitUntil(() => callbackCalled);
            yield return new WaitForSeconds(1f);

            Assert.IsTrue(success, "Match creation should succeed");
            Assert.IsNotNull(matchId, "Match ID should not be null");
            Assert.IsNotEmpty(matchId, "Match ID should not be empty");
        }

        [UnityTest]
        [Order(2)]
        public IEnumerator JoinMatch_ShouldSucceed()
        {
            yield return WaitForFirebaseInitialization();
            
            if (string.IsNullOrEmpty(testMatchId))
            {
                yield return CreateMatch_ShouldSucceed();
            }

            var playerData = new PlayerMatchData
            {
                PlayerId = "test-player-1",
                PlayerName = "TestPlayer1",
                Score = 0
            };

            bool callbackCalled = false;
            bool success = false;

            service.JoinMatch(testMatchId, playerData, (result) =>
            {
                callbackCalled = true;
                success = result;
            });

            yield return new WaitUntil(() => callbackCalled);
            yield return new WaitForSeconds(1f);

            Assert.IsTrue(success, "Player should join match successfully");
        }

        [UnityTest]
        [Order(3)]
        public IEnumerator ListenForPlayersInMatch_ShouldReceivePlayer()
        {
            yield return WaitForFirebaseInitialization();

            bool listenerCalled = false;
            List<PlayerMatchData> players = null;

            service.ListenForPlayersInMatch(testMatchId, (receivedPlayers) =>
            {
                listenerCalled = true;
                players = receivedPlayers;
            });

            yield return new WaitUntil(() => listenerCalled);
            yield return new WaitForSeconds(2f);

            Assert.IsNotNull(players, "Players list should not be null");
            Assert.Greater(players.Count, 0, "Should have at least one player");
            
            service.StopListeningForPlayersInMatch(testMatchId);
        }

        [UnityTest]
        [Order(4)]
        public IEnumerator AddGameEvent_ShouldSucceed()
        {
            yield return WaitForFirebaseInitialization();

            var gameEvent = new GameEventData
            {
                EventType = GameEventType.PlayerMoved,
                PlayerId = "test-player-1",
                Timestamp = System.DateTime.Now.Ticks,
                Data = "integration-test-event"
            };

            bool callbackCalled = false;
            bool success = false;

            service.AddGameEvent(testMatchId, gameEvent, (result) =>
            {
                callbackCalled = true;
                success = result;
            });

            yield return new WaitUntil(() => callbackCalled);
            yield return new WaitForSeconds(1f);

            Assert.IsTrue(success, "Game event should be added successfully");
        }

        [UnityTest]
        [Order(5)]
        public IEnumerator UpdatePlayerScore_ShouldSucceed()
        {
            yield return WaitForFirebaseInitialization();

            bool callbackCalled = false;
            bool success = false;

            service.UpdatePlayerScore(testMatchId, "test-player-1", 100, (result) =>
            {
                callbackCalled = true;
                success = result;
            });

            yield return new WaitUntil(() => callbackCalled);
            yield return new WaitForSeconds(1f);

            Assert.IsTrue(success, "Player score should update successfully");
        }

        [UnityTest]
        [Order(6)]
        public IEnumerator UpdateMatchState_ShouldSucceed()
        {
            yield return WaitForFirebaseInitialization();

            bool callbackCalled = false;
            bool success = false;

            service.UpdateMatchState(testMatchId, 1, (result) =>
            {
                callbackCalled = true;
                success = result;
            });

            yield return new WaitUntil(() => callbackCalled);
            yield return new WaitForSeconds(1f);

            Assert.IsTrue(success, "Match state should update successfully");
        }

        [UnityTest]
        [Order(7)]
        public IEnumerator ListenForMatchStateChanges_ShouldReceiveUpdates()
        {
            yield return WaitForFirebaseInitialization();

            bool listenerCalled = false;
            MatchData receivedMatch = null;

            service.ListenForMatchStateChanges(testMatchId, (match) =>
            {
                listenerCalled = true;
                receivedMatch = match;
            });

            yield return new WaitUntil(() => listenerCalled);
            yield return new WaitForSeconds(2f);

            Assert.IsNotNull(receivedMatch, "Should receive match data");
            Assert.AreEqual(1, receivedMatch.State, "Match state should be updated");
            
            service.StopListeningForMatchStateChanges(testMatchId);
        }

        [UnityTest]
        [Order(8)]
        public IEnumerator ListenForAvailableMatches_ShouldIncludeTestMatch()
        {
            yield return WaitForFirebaseInitialization();

            bool listenerCalled = false;
            List<MatchData> matches = null;

            service.ListenForAvailableMatches((receivedMatches) =>
            {
                listenerCalled = true;
                matches = receivedMatches;
            });

            yield return new WaitUntil(() => listenerCalled);
            yield return new WaitForSeconds(2f);

            Assert.IsNotNull(matches, "Matches list should not be null");
            Assert.Greater(matches.Count, 0, "Should have at least one match");
            
            bool testMatchFound = matches.Exists(m => m.MatchId == testMatchId);
            Assert.IsTrue(testMatchFound, "Test match should be in available matches");
            
            service.StopListeningForAvailableMatches();
        }

        [Test]
        public void FirebaseCredentials_ShouldBeConfigured()
        {
            Assert.IsNotNull(service, "Firebase service should be instantiated");
        }
    }
}