using UnityEngine;
using Network;
using System.Collections;

public class FirebaseConnectionTester : MonoBehaviour
{
    [Header("Firebase Connection Test")]
    [SerializeField] private bool runTestOnStart = true;
    [SerializeField] private float testTimeout = 30f;
    
    private FirebaseDatabaseService firebaseService;
    private bool connectionTested = false;

    private void Start()
    {
        if (runTestOnStart)
        {
            StartCoroutine(TestFirebaseConnection());
        }
    }

    [ContextMenu("Test Firebase Connection")]
    public void TestConnection()
    {
        if (!connectionTested)
        {
            StartCoroutine(TestFirebaseConnection());
        }
        else
        {
            Debug.Log("Connection already tested. Check console for results.");
        }
    }

    private IEnumerator TestFirebaseConnection()
    {
        Debug.Log("=== FIREBASE CONNECTION TEST STARTED ===");
        connectionTested = true;

        if (firebaseService == null)
        {
            var serviceGO = new GameObject("FirebaseConnectionTest");
            firebaseService = serviceGO.AddComponent<FirebaseDatabaseService>();
        }

        bool initializationComplete = false;
        bool initializationSuccess = false;

        firebaseService.OnInitialized += () =>
        {
            initializationComplete = true;
            initializationSuccess = true;
            Debug.Log("✅ Firebase initialized successfully!");
        };

        firebaseService.OnInitializationError += (error) =>
        {
            initializationComplete = true;
            initializationSuccess = false;
            Debug.LogError($"❌ Firebase initialization failed: {error}");
        };

        float elapsed = 0f;
        while (!initializationComplete && elapsed < testTimeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!initializationComplete)
        {
            Debug.LogError($"❌ Firebase initialization timed out after {testTimeout} seconds");
            yield break;
        }

        if (!initializationSuccess)
        {
            Debug.LogError("❌ Firebase connection test failed - initialization error");
            yield break;
        }

        yield return TestDatabaseOperations();
    }

    private IEnumerator TestDatabaseOperations()
    {
        Debug.Log("--- Testing Database Operations ---");

        yield return TestCreateMatch();
        yield return TestJoinMatch();
        yield return TestListMatches();
        yield return TestAddEvent();
        yield return TestUpdateScore();
        yield return TestCleanup();

        Debug.Log("=== FIREBASE CONNECTION TEST COMPLETED ===");
    }

    private IEnumerator TestCreateMatch()
    {
        Debug.Log("Testing: Create Match");
        
        var matchData = new MatchData
        {
            MaxPlayers = 4,
            State = 0
        };

        bool completed = false;
        bool success = false;

        firebaseService.CreateMatch(matchData, (result, matchId) =>
        {
            completed = true;
            success = result;
            
            if (success)
            {
                Debug.Log($"✅ Match created with ID: {matchId}");
                PlayerPrefs.SetString("TestMatchId", matchId);
            }
            else
            {
                Debug.LogError("❌ Failed to create match");
            }
        });

        yield return new WaitUntil(() => completed);
    }

    private IEnumerator TestJoinMatch()
    {
        Debug.Log("Testing: Join Match");
        
        string matchId = PlayerPrefs.GetString("TestMatchId");
        if (string.IsNullOrEmpty(matchId))
        {
            Debug.LogError("❌ No test match ID available");
            yield break;
        }

        var playerData = new PlayerMatchData
        {
            PlayerId = $"test-player-{System.DateTime.Now.Ticks}",
            PlayerName = "TestPlayer",
            Score = 0
        };

        bool completed = false;
        bool success = false;

        firebaseService.JoinMatch(matchId, playerData, (result) =>
        {
            completed = true;
            success = result;
            
            if (success)
            {
                Debug.Log("✅ Player joined match successfully");
                PlayerPrefs.SetString("TestPlayerId", playerData.PlayerId);
            }
            else
            {
                Debug.LogError("❌ Failed to join match");
            }
        });

        yield return new WaitUntil(() => completed);
    }

    private IEnumerator TestListMatches()
    {
        Debug.Log("Testing: List Matches");
        
        bool completed = false;

        firebaseService.ListenForAvailableMatches((matches) =>
        {
            completed = true;
            Debug.Log($"✅ Received {matches.Count} available matches");
            firebaseService.StopListeningForAvailableMatches();
        });

        yield return new WaitUntil(() => completed);
    }

    private IEnumerator TestAddEvent()
    {
        Debug.Log("Testing: Add Game Event");
        
        string matchId = PlayerPrefs.GetString("TestMatchId");
        string playerId = PlayerPrefs.GetString("TestPlayerId");
        
        if (string.IsNullOrEmpty(matchId) || string.IsNullOrEmpty(playerId))
        {
            Debug.LogError("❌ Missing test data for event test");
            yield break;
        }

        var gameEvent = new GameEventData
        {
            EventType = GameEventType.PlayerMoved,
            PlayerId = playerId,
            Timestamp = System.DateTime.Now.Ticks,
            Data = "test-event-data"
        };

        bool completed = false;
        bool success = false;

        firebaseService.AddGameEvent(matchId, gameEvent, (result) =>
        {
            completed = true;
            success = result;
            
            if (success)
            {
                Debug.Log("✅ Game event added successfully");
            }
            else
            {
                Debug.LogError("❌ Failed to add game event");
            }
        });

        yield return new WaitUntil(() => completed);
    }

    private IEnumerator TestUpdateScore()
    {
        Debug.Log("Testing: Update Player Score");
        
        string matchId = PlayerPrefs.GetString("TestMatchId");
        string playerId = PlayerPrefs.GetString("TestPlayerId");
        
        if (string.IsNullOrEmpty(matchId) || string.IsNullOrEmpty(playerId))
        {
            Debug.LogError("❌ Missing test data for score update");
            yield break;
        }

        bool completed = false;
        bool success = false;

        firebaseService.UpdatePlayerScore(matchId, playerId, 100, (result) =>
        {
            completed = true;
            success = result;
            
            if (success)
            {
                Debug.Log("✅ Player score updated successfully");
            }
            else
            {
                Debug.LogError("❌ Failed to update player score");
            }
        });

        yield return new WaitUntil(() => completed);
    }

    private IEnumerator TestCleanup()
    {
        Debug.Log("Testing: Cleanup (Remove Match)");
        
        string matchId = PlayerPrefs.GetString("TestMatchId");
        if (string.IsNullOrEmpty(matchId))
        {
            Debug.Log("⚠️ No test match to cleanup");
            yield break;
        }

        bool completed = false;
        bool success = false;

        firebaseService.RemoveMatch(matchId, (result) =>
        {
            completed = true;
            success = result;
            
            if (success)
            {
                Debug.Log("✅ Test match cleaned up successfully");
                PlayerPrefs.DeleteKey("TestMatchId");
                PlayerPrefs.DeleteKey("TestPlayerId");
            }
            else
            {
                Debug.LogError("❌ Failed to cleanup test match");
            }
        });

        yield return new WaitUntil(() => completed);
    }

    private void OnDestroy()
    {
        if (firebaseService != null && firebaseService.gameObject != null)
        {
            DestroyImmediate(firebaseService.gameObject);
        }
    }
}