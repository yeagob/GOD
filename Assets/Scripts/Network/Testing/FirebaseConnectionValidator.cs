using System;
using System.Threading.Tasks;
using UnityEngine;

public class FirebaseConnectionValidator : MonoBehaviour
{
    [Header("Validation Settings")]
    [SerializeField] private float timeoutSeconds = 10f;
    [SerializeField] private int maxRetries = 3;
    [SerializeField] private float retryDelay = 2f;
    
    private FirebaseService firebaseService;
    
    public event Action<bool> OnValidationComplete;
    public event Action<string> OnValidationError;
    public event Action<string> OnValidationProgress;
    
    private void Start()
    {
        firebaseService = FindObjectOfType<FirebaseService>();
        if (firebaseService == null)
        {
            firebaseService = gameObject.AddComponent<FirebaseService>();
        }
    }
    
    public async Task<bool> ValidateConnection()
    {
        OnValidationProgress?.Invoke("Starting Firebase validation...");
        
        if (!await WaitForFirebaseInitialization())
        {
            OnValidationError?.Invoke("Firebase failed to initialize within timeout period");
            return false;
        }
        
        OnValidationProgress?.Invoke("Firebase initialized, testing connection...");
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                OnValidationProgress?.Invoke($"Connection attempt {attempt}/{maxRetries}...");
                
                var connectionTest = await TestFirebaseOperations();
                if (connectionTest)
                {
                    OnValidationProgress?.Invoke("All Firebase operations successful!");
                    OnValidationComplete?.Invoke(true);
                    return true;
                }
                
                if (attempt < maxRetries)
                {
                    OnValidationProgress?.Invoke($"Attempt {attempt} failed, retrying in {retryDelay} seconds...");
                    await Task.Delay((int)(retryDelay * 1000));
                }
            }
            catch (Exception ex)
            {
                OnValidationError?.Invoke($"Attempt {attempt} failed: {ex.Message}");
                
                if (attempt < maxRetries)
                {
                    await Task.Delay((int)(retryDelay * 1000));
                }
            }
        }
        
        OnValidationError?.Invoke($"All {maxRetries} connection attempts failed");
        OnValidationComplete?.Invoke(false);
        return false;
    }
    
    private async Task<bool> WaitForFirebaseInitialization()
    {
        var startTime = Time.time;
        
        while (!firebaseService.IsInitialized && (Time.time - startTime) < timeoutSeconds)
        {
            await Task.Delay(100);
        }
        
        return firebaseService.IsInitialized;
    }
    
    private async Task<bool> TestFirebaseOperations()
    {
        try
        {
            OnValidationProgress?.Invoke("Testing write operation...");
            var testData = new { test = "validation", timestamp = DateTime.UtcNow.ToString() };
            await firebaseService.WriteData("validation/test", testData);
            
            OnValidationProgress?.Invoke("Testing read operation...");
            var readData = await firebaseService.ReadData<object>("validation/test");
            
            OnValidationProgress?.Invoke("Testing delete operation...");
            await firebaseService.DeleteData("validation/test");
            
            OnValidationProgress?.Invoke("All operations completed successfully");
            return true;
        }
        catch (Exception ex)
        {
            OnValidationError?.Invoke($"Operation failed: {ex.Message}");
            return false;
        }
    }
    
    public async Task<bool> ValidateGameDataStructure()
    {
        OnValidationProgress?.Invoke("Validating game data structure...");
        
        try
        {
            var testMatch = new MatchData
            {
                MatchId = "validation_match",
                StartTime = DateTime.UtcNow,
                Status = "testing",
                MaxPlayers = 4,
                CurrentPlayers = 1,
                BoardType = "validation"
            };
            
            OnValidationProgress?.Invoke("Testing MatchData serialization...");
            await firebaseService.WriteData("validation/match", testMatch);
            var retrievedMatch = await firebaseService.ReadData<MatchData>("validation/match");
            
            if (string.IsNullOrEmpty(retrievedMatch.MatchId))
            {
                throw new Exception("MatchData serialization failed");
            }
            
            var testPlayer = new PlayerMatchData
            {
                PlayerId = "validation_player",
                PlayerName = "Test Player",
                MatchId = "validation_match",
                Position = 0,
                Score = 100,
                JoinTime = DateTime.UtcNow,
                IsActive = true,
                IsWinner = false
            };
            
            OnValidationProgress?.Invoke("Testing PlayerMatchData serialization...");
            await firebaseService.WriteData("validation/player", testPlayer);
            var retrievedPlayer = await firebaseService.ReadData<PlayerMatchData>("validation/player");
            
            if (string.IsNullOrEmpty(retrievedPlayer.PlayerId))
            {
                throw new Exception("PlayerMatchData serialization failed");
            }
            
            var testEvent = new GameEventData
            {
                EventId = "validation_event",
                MatchId = "validation_match",
                PlayerId = "validation_player",
                EventType = "validation",
                EventData = "{\"test\":true}",
                Timestamp = DateTime.UtcNow,
                Turn = 1
            };
            
            OnValidationProgress?.Invoke("Testing GameEventData serialization...");
            await firebaseService.WriteData("validation/event", testEvent);
            var retrievedEvent = await firebaseService.ReadData<GameEventData>("validation/event");
            
            if (string.IsNullOrEmpty(retrievedEvent.EventId))
            {
                throw new Exception("GameEventData serialization failed");
            }
            
            OnValidationProgress?.Invoke("Cleaning up validation data...");
            await firebaseService.DeleteData("validation");
            
            OnValidationProgress?.Invoke("Data structure validation completed successfully");
            return true;
        }
        catch (Exception ex)
        {
            OnValidationError?.Invoke($"Data structure validation failed: {ex.Message}");
            return false;
        }
    }
    
    public async Task<ValidationReport> GenerateValidationReport()
    {
        var report = new ValidationReport
        {
            Timestamp = DateTime.UtcNow,
            IsFirebaseInitialized = firebaseService?.IsInitialized ?? false
        };
        
        OnValidationProgress?.Invoke("Generating validation report...");
        
        report.ConnectionTest = await ValidateConnection();
        report.DataStructureTest = await ValidateGameDataStructure();
        
        report.OverallStatus = report.ConnectionTest && report.DataStructureTest;
        
        OnValidationProgress?.Invoke($"Validation report generated - Status: {(report.OverallStatus ? "PASS" : "FAIL")}");
        
        return report;
    }
    
    [Serializable]
    public struct ValidationReport
    {
        public DateTime Timestamp;
        public bool IsFirebaseInitialized;
        public bool ConnectionTest;
        public bool DataStructureTest;
        public bool OverallStatus;
        
        public override string ToString()
        {
            return $"Firebase Validation Report\n" +
                   $"Timestamp: {Timestamp}\n" +
                   $"Firebase Initialized: {IsFirebaseInitialized}\n" +
                   $"Connection Test: {(ConnectionTest ? "PASS" : "FAIL")}\n" +
                   $"Data Structure Test: {(DataStructureTest ? "PASS" : "FAIL")}\n" +
                   $"Overall Status: {(OverallStatus ? "PASS" : "FAIL")}";
        }
    }
}
