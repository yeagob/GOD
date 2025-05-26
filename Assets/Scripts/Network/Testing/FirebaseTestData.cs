using System;
using UnityEngine;

[CreateAssetMenu(fileName = "FirebaseTestData", menuName = "GOD/Firebase Test Data")]
public class FirebaseTestData : ScriptableObject
{
    [Header("Test Configuration")]
    public string testPlayerId = "test_player_1";
    public string testPlayerName = "Test Player";
    public string testBoardType = "classic";
    public int maxPlayers = 4;
    
    [Header("Test Scenarios")]
    public TestScenario[] testScenarios;
    
    [Serializable]
    public struct TestScenario
    {
        public string scenarioName;
        public string description;
        public TestAction[] actions;
    }
    
    [Serializable]
    public struct TestAction
    {
        public TestActionType actionType;
        public string parameters;
        public float delay;
    }
    
    public enum TestActionType
    {
        TestConnection,
        CreateMatch,
        JoinMatch,
        MovePlayer,
        RollDice,
        LogSpecialTile,
        EndMatch,
        Wait
    }
}
