using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using UnityEngine;
using Newtonsoft.Json;
using Network.Models;
using Network.Services;

namespace Network
{
    public class FirebaseDatabaseService : MonoBehaviour, IFirebaseDatabaseService
    {
        private FirebaseApp app;
        private DatabaseReference databaseReference;
        private FirebaseAuth auth;
        private bool isInitialized = false;

        public event Action OnInitialized;
        public event Action<string> OnInitializationError;

        private const string MATCHES_PATH = "matches";
        private const string PLAYERS_PATH = "players";
        private const string EVENTS_PATH = "events";

        private void Start()
        {
            InitializeFirebase();
        }

        private async void InitializeFirebase()
        {
            try
            {
                var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
                
                if (dependencyStatus == DependencyStatus.Available)
                {
                    InitializeWithConfig();
                }
                else
                {
                    string error = $"Could not resolve Firebase dependencies: {dependencyStatus}";
                    OnInitializationError?.Invoke(error);
                    Debug.LogError(error);
                }
            }
            catch (Exception ex)
            {
                string error = $"Firebase initialization failed: {ex.Message}";
                OnInitializationError?.Invoke(error);
                Debug.LogError(error);
            }
        }

        private void InitializeWithConfig()
        {
            FirebaseConfigLoader.LoadConfigAsync(async config =>
            {
                try
                {
                    if (string.IsNullOrEmpty(config.databaseURL))
                    {
                        throw new InvalidOperationException("Database URL is not configured");
                    }

                    app = FirebaseApp.DefaultInstance;
                    databaseReference = FirebaseDatabase.GetInstance(app, config.databaseURL).RootReference;
                    auth = FirebaseAuth.DefaultInstance;
                    
                    await AuthenticateWithServiceAccount(config.serviceAccount);
                    
                    isInitialized = true;
                    OnInitialized?.Invoke();
                    Debug.Log($"Firebase initialized successfully with URL: {config.databaseURL}");
                }
                catch (Exception ex)
                {
                    string error = $"Firebase initialization with config failed: {ex.Message}";
                    OnInitializationError?.Invoke(error);
                    Debug.LogError(error);
                }
            });
        }

        private async Task AuthenticateWithServiceAccount(ServiceAccountData serviceAccount)
        {
            string serviceAccountJson = JsonConvert.SerializeObject(serviceAccount);
            var credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromJson(serviceAccountJson);
            var token = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
            await auth.SignInWithCustomTokenAsync(token);
        }

        public void CreateMatch(MatchData match, Action<bool, string> onComplete)
        {
            if (!isInitialized)
            {
                onComplete?.Invoke(false, "Firebase not initialized");
                return;
            }

            try
            {
                var newMatchRef = databaseReference.Child(MATCHES_PATH).Push();
                string matchId = newMatchRef.Key;
                
                string json = JsonConvert.SerializeObject(match);
                newMatchRef.SetRawJsonValueAsync(json).ContinueWith(task =>
                {
                    bool success = !task.IsFaulted && !task.IsCanceled;
                    onComplete?.Invoke(success, success ? matchId : task.Exception?.Message);
                });
            }
            catch (Exception ex)
            {
                onComplete?.Invoke(false, ex.Message);
            }
        }

        public void ListenForAvailableMatches(Action<List<MatchData>> onDataChanged)
        {
            if (!isInitialized)
            {
                return;
            }

            databaseReference.Child(MATCHES_PATH).ValueChanged += (object sender, ValueChangedEventArgs args) =>
            {
                try
                {
                    var matches = new List<MatchData>();
                    
                    if (args.Snapshot.Exists)
                    {
                        foreach (var child in args.Snapshot.Children)
                        {
                            string json = child.GetRawJsonValue();
                            MatchData match = JsonConvert.DeserializeObject<MatchData>(json);
                            matches.Add(match);
                        }
                    }
                    
                    onDataChanged?.Invoke(matches);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error processing matches: {ex.Message}");
                }
            };
        }

        public void StopListeningForAvailableMatches()
        {
            if (isInitialized)
            {
                databaseReference.Child(MATCHES_PATH).ValueChanged -= null;
            }
        }

        public void JoinMatch(string matchId, PlayerMatchData player, Action<bool> onComplete)
        {
            if (!isInitialized)
            {
                onComplete?.Invoke(false);
                return;
            }

            try
            {
                string json = JsonConvert.SerializeObject(player);
                var playerRef = databaseReference.Child(MATCHES_PATH).Child(matchId).Child(PLAYERS_PATH).Child(player.PlayerId);
                
                playerRef.SetRawJsonValueAsync(json).ContinueWith(task =>
                {
                    bool success = !task.IsFaulted && !task.IsCanceled;
                    onComplete?.Invoke(success);
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error joining match: {ex.Message}");
                onComplete?.Invoke(false);
            }
        }

        public void ListenForMatchStateChanges(string matchId, Action<MatchData> onDataChanged)
        {
            if (!isInitialized)
            {
                return;
            }

            databaseReference.Child(MATCHES_PATH).Child(matchId).ValueChanged += (object sender, ValueChangedEventArgs args) =>
            {
                try
                {
                    if (args.Snapshot.Exists)
                    {
                        string json = args.Snapshot.GetRawJsonValue();
                        MatchData match = JsonConvert.DeserializeObject<MatchData>(json);
                        onDataChanged?.Invoke(match);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error processing match state: {ex.Message}");
                }
            };
        }

        public void StopListeningForMatchStateChanges(string matchId)
        {
            if (isInitialized)
            {
                databaseReference.Child(MATCHES_PATH).Child(matchId).ValueChanged -= null;
            }
        }

        public void ListenForPlayersInMatch(string matchId, Action<List<PlayerMatchData>> onDataChanged)
        {
            if (!isInitialized)
            {
                return;
            }

            databaseReference.Child(MATCHES_PATH).Child(matchId).Child(PLAYERS_PATH).ValueChanged += (object sender, ValueChangedEventArgs args) =>
            {
                try
                {
                    var players = new List<PlayerMatchData>();
                    
                    if (args.Snapshot.Exists)
                    {
                        foreach (var child in args.Snapshot.Children)
                        {
                            string json = child.GetRawJsonValue();
                            PlayerMatchData player = JsonConvert.DeserializeObject<PlayerMatchData>(json);
                            players.Add(player);
                        }
                    }
                    
                    onDataChanged?.Invoke(players);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error processing players: {ex.Message}");
                }
            };
        }

        public void StopListeningForPlayersInMatch(string matchId)
        {
            if (isInitialized)
            {
                databaseReference.Child(MATCHES_PATH).Child(matchId).Child(PLAYERS_PATH).ValueChanged -= null;
            }
        }

        public void AddGameEvent(string matchId, GameEventData gameEvent, Action<bool> onComplete)
        {
            if (!isInitialized)
            {
                onComplete?.Invoke(false);
                return;
            }

            try
            {
                string json = JsonConvert.SerializeObject(gameEvent);
                var eventRef = databaseReference.Child(MATCHES_PATH).Child(matchId).Child(EVENTS_PATH).Push();
                
                eventRef.SetRawJsonValueAsync(json).ContinueWith(task =>
                {
                    bool success = !task.IsFaulted && !task.IsCanceled;
                    onComplete?.Invoke(success);
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error adding game event: {ex.Message}");
                onComplete?.Invoke(false);
            }
        }

        public void ListenForNewEventsInMatch(string matchId, Action<GameEventData> onEventAdded)
        {
            if (!isInitialized)
            {
                return;
            }

            databaseReference.Child(MATCHES_PATH).Child(matchId).Child(EVENTS_PATH).ChildAdded += (object sender, ChildChangedEventArgs args) =>
            {
                try
                {
                    if (args.Snapshot.Exists)
                    {
                        string json = args.Snapshot.GetRawJsonValue();
                        GameEventData gameEvent = JsonConvert.DeserializeObject<GameEventData>(json);
                        onEventAdded?.Invoke(gameEvent);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error processing game event: {ex.Message}");
                }
            };
        }

        public void StopListeningForNewEventsInMatch(string matchId)
        {
            if (isInitialized)
            {
                databaseReference.Child(MATCHES_PATH).Child(matchId).Child(EVENTS_PATH).ChildAdded -= null;
            }
        }

        public void UpdateMatchState(string matchId, int newState, Action<bool> onComplete)
        {
            if (!isInitialized)
            {
                onComplete?.Invoke(false);
                return;
            }

            try
            {
                var updates = new Dictionary<string, object>
                {
                    { "state", newState }
                };

                databaseReference.Child(MATCHES_PATH).Child(matchId).UpdateChildrenAsync(updates).ContinueWith(task =>
                {
                    bool success = !task.IsFaulted && !task.IsCanceled;
                    onComplete?.Invoke(success);
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error updating match state: {ex.Message}");
                onComplete?.Invoke(false);
            }
        }

        public void UpdatePlayerScore(string matchId, string playerId, int newScore, Action<bool> onComplete)
        {
            if (!isInitialized)
            {
                onComplete?.Invoke(false);
                return;
            }

            try
            {
                var updates = new Dictionary<string, object>
                {
                    { "score", newScore }
                };

                databaseReference.Child(MATCHES_PATH).Child(matchId).Child(PLAYERS_PATH).Child(playerId)
                    .UpdateChildrenAsync(updates).ContinueWith(task =>
                {
                    bool success = !task.IsFaulted && !task.IsCanceled;
                    onComplete?.Invoke(success);
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error updating player score: {ex.Message}");
                onComplete?.Invoke(false);
            }
        }

        public void UpdatePlayerData(string matchId, string playerId, PlayerMatchData playerData, Action<bool> onComplete)
        {
            if (!isInitialized)
            {
                onComplete?.Invoke(false);
                return;
            }

            try
            {
                string json = JsonConvert.SerializeObject(playerData);
                var playerRef = databaseReference.Child(MATCHES_PATH).Child(matchId).Child(PLAYERS_PATH).Child(playerId);
                
                playerRef.SetRawJsonValueAsync(json).ContinueWith(task =>
                {
                    bool success = !task.IsFaulted && !task.IsCanceled;
                    onComplete?.Invoke(success);
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error updating player data: {ex.Message}");
                onComplete?.Invoke(false);
            }
        }

        public void RemoveMatch(string matchId, Action<bool> onComplete)
        {
            if (!isInitialized)
            {
                onComplete?.Invoke(false);
                return;
            }

            try
            {
                databaseReference.Child(MATCHES_PATH).Child(matchId).RemoveValueAsync().ContinueWith(task =>
                {
                    bool success = !task.IsFaulted && !task.IsCanceled;
                    onComplete?.Invoke(success);
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error removing match: {ex.Message}");
                onComplete?.Invoke(false);
            }
        }

        public void RemovePlayerFromMatch(string matchId, string playerId, Action<bool> onComplete)
        {
            if (!isInitialized)
            {
                onComplete?.Invoke(false);
                return;
            }

            try
            {
                databaseReference.Child(MATCHES_PATH).Child(matchId).Child(PLAYERS_PATH).Child(playerId)
                    .RemoveValueAsync().ContinueWith(task =>
                {
                    bool success = !task.IsFaulted && !task.IsCanceled;
                    onComplete?.Invoke(success);
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error removing player from match: {ex.Message}");
                onComplete?.Invoke(false);
            }
        }

        private void OnDestroy()
        {
            if (app != null)
            {
                app.Dispose();
            }
        }
    }
}
