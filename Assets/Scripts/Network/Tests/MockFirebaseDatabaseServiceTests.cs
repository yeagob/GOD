using NUnit.Framework;
using System.Collections.Generic;
using Network;

namespace Tests
{
    public class MockFirebaseDatabaseServiceTests
    {
        private MockFirebaseDatabaseService mockService;

        [SetUp]
        public void Setup()
        {
            mockService = new MockFirebaseDatabaseService();
        }

        [Test]
        public void CreateMatch_Should_ReturnSuccessWithMatchId()
        {
            var matchData = new MatchData
            {
                MaxPlayers = 4,
                State = 0
            };

            bool success = false;
            string matchId = null;

            mockService.CreateMatch(matchData, (result, id) =>
            {
                success = result;
                matchId = id;
            });

            Assert.IsTrue(success);
            Assert.IsNotNull(matchId);
            Assert.IsNotEmpty(matchId);
        }

        [Test]
        public void JoinMatch_ValidMatch_Should_ReturnSuccess()
        {
            var matchData = new MatchData { MaxPlayers = 4, State = 0 };
            string matchId = null;

            mockService.CreateMatch(matchData, (success, id) => matchId = id);

            var playerData = new PlayerMatchData
            {
                PlayerId = "player1",
                PlayerName = "TestPlayer",
                Score = 0
            };

            bool joinResult = false;
            mockService.JoinMatch(matchId, playerData, (result) => joinResult = result);

            Assert.IsTrue(joinResult);
        }

        [Test]
        public void JoinMatch_InvalidMatch_Should_ReturnFailure()
        {
            var playerData = new PlayerMatchData
            {
                PlayerId = "player1",
                PlayerName = "TestPlayer",
                Score = 0
            };

            bool joinResult = true;
            mockService.JoinMatch("invalid-match-id", playerData, (result) => joinResult = result);

            Assert.IsFalse(joinResult);
        }

        [Test]
        public void ListenForAvailableMatches_Should_ReturnMatchList()
        {
            var matchData = new MatchData { MaxPlayers = 4, State = 0 };
            mockService.CreateMatch(matchData, (success, id) => { });

            List<MatchData> matches = null;
            mockService.ListenForAvailableMatches((result) => matches = result);

            Assert.IsNotNull(matches);
            Assert.AreEqual(1, matches.Count);
        }

        [Test]
        public void UpdatePlayerScore_ValidPlayer_Should_ReturnSuccess()
        {
            var matchData = new MatchData { MaxPlayers = 4, State = 0 };
            string matchId = null;
            mockService.CreateMatch(matchData, (success, id) => matchId = id);

            var playerData = new PlayerMatchData
            {
                PlayerId = "player1",
                PlayerName = "TestPlayer",
                Score = 0
            };
            mockService.JoinMatch(matchId, playerData, (result) => { });

            bool updateResult = false;
            mockService.UpdatePlayerScore(matchId, "player1", 100, (result) => updateResult = result);

            Assert.IsTrue(updateResult);
        }

        [Test]
        public void AddGameEvent_ValidMatch_Should_ReturnSuccess()
        {
            var matchData = new MatchData { MaxPlayers = 4, State = 0 };
            string matchId = null;
            mockService.CreateMatch(matchData, (success, id) => matchId = id);

            var gameEvent = new GameEventData
            {
                EventType = GameEventType.PlayerMoved,
                PlayerId = "player1",
                Timestamp = System.DateTime.Now.Ticks,
                Data = "test-data"
            };

            bool eventResult = false;
            mockService.AddGameEvent(matchId, gameEvent, (result) => eventResult = result);

            Assert.IsTrue(eventResult);
        }

        [Test]
        public void RemoveMatch_ValidMatch_Should_ReturnSuccess()
        {
            var matchData = new MatchData { MaxPlayers = 4, State = 0 };
            string matchId = null;
            mockService.CreateMatch(matchData, (success, id) => matchId = id);

            bool removeResult = false;
            mockService.RemoveMatch(matchId, (result) => removeResult = result);

            Assert.IsTrue(removeResult);
        }

        [Test]
        public void UpdateMatchState_ValidMatch_Should_ReturnSuccess()
        {
            var matchData = new MatchData { MaxPlayers = 4, State = 0 };
            string matchId = null;
            mockService.CreateMatch(matchData, (success, id) => matchId = id);

            bool updateResult = false;
            mockService.UpdateMatchState(matchId, 1, (result) => updateResult = result);

            Assert.IsTrue(updateResult);
        }

        [Test]
        public void ListenForPlayersInMatch_EmptyMatch_Should_ReturnEmptyList()
        {
            var matchData = new MatchData { MaxPlayers = 4, State = 0 };
            string matchId = null;
            mockService.CreateMatch(matchData, (success, id) => matchId = id);

            List<PlayerMatchData> players = null;
            mockService.ListenForPlayersInMatch(matchId, (result) => players = result);

            Assert.IsNotNull(players);
            Assert.AreEqual(0, players.Count);
        }
    }
}