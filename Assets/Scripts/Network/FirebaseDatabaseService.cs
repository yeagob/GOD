using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using UnityEngine;
using Newtonsoft.Json;

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
                    app = FirebaseApp.DefaultInstance;
                    databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
                    auth = FirebaseAuth.DefaultInstance;
                    
                    await AuthenticateWithServiceAccount();
                    
                    isInitialized = true;
                    OnInitialized?.Invoke();
                    Debug.Log("Firebase initialized successfully");
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

        private async Task AuthenticateWithServiceAccount()
        {
            string serviceAccountJson = @"{
                ""type"": ""service_account"",
                ""project_id"": ""game-of-duck-multiplayer"",
                ""private_key_id"": ""800ac48ca756bbbe8c8b9dc5b1be0b579eccd93e"",
                ""private_key"": ""-----BEGIN PRIVATE KEY-----\nMIIEvwIBADANBgkqhkiG9w0BAQEFAASCBKkwggSlAgEAAoIBAQC7tUu9cH7NqZQH\nUalcZPvIhtlHdVdukDT+QibfgPkrcgJlRfdvCEcPKEYyz2jonEPK6LE5aNIU8vkW\nTviqjuvT1ModY0jmfoUE1xFuszqwbnzLEhkj0aw/98tkZcOPdlx2a/tKTERkAhpo\nkSM6rLbkLe6pYlUiXw3u4nZiQNx2EOA/jhEHgqEVbm8C6JrRhNujYU+WALwic1N0\n9b+x8uriQUkrEQ1oDBf5lqVhDhBtZb9MXTNnaMwzmygE5ZrmnPKblevcGg4sOyOt\nrPMesG3lmtqD/p1822VuDtwh8hDo6zKb1bytxL8DBxni51dNb7U9/84iSpbRyHQS\n6nS0g+AXAgMBAAECggEAEPzfjcBUcYd4h/sKK/xQVH3DxKBS2S+VjtyXc7XSlxh6\ngrDaK63LmeG9Yf7RvJoxK81zBezodRCSSMRn49kLGxY7Hn1sTRPuikQZO0le3+B3\n9ybm/eNixpKbUTWWhRwjUm0DsTMwI1Q/Ze7wtHA0yHd8YyRXlm8/gxBIF4H4PHpd\nwutdrYPv8Ni+gTGl/tFE9WzvckpX1MQ7RDbAzGQDWI0OIeGZl91kzm4kV5KvSZmB\nlqfc54g+ZSbmAlXYbVlOB5mr2i4c0gpZmv6tgcPRVkAReOAcoI7WqqF04+9k7qSB\niXG6x+YdjitGqRpOv5FvzyB5l93ppuVny9ZoXMlMEQKBgQDpJIBXe3tFLa9NORuj\ncCLN+PXJjg/ewcZi+B+TG8CExNEptxLaKeQeHXCE/nO15SWyA+KUkwMbfNZlSFQn\np9UereCgNxB8HiX5MrA+uJeJtZ45qf0veW05stOqLWeoAwA07FNAoKzQiPOdeHPn\nxTSV3JNZeTquMRBfXsq7YZQzRwKBgQDOHHcbflWTy3XfBtxfxqTFB8MVNeysPYgr\n2hY/U740SCpO0ZnXw0g9Qe8XMmIBNCiFRBmj8RJl9k/MsI+iHFAjXWNPx72y/snx\nZf22Mx+c7nik7PUKzHGT/053Ss064PqDp6SLPun1f/1HhrdSPrm+PCQiwNY8iut8\nSeiC5U40sQKBgQCRI58eVwoLrAApA/dXzPRt46Inwt/QXjPB4xPNAgbc4KYR4R3E\nYTXZJZypvrqML0ZDRzXkJo1VrGbQELILKel8OuTO+NizXBVpyIt90G7OVRlWbqPm\nzSIZPGGW3MNeDdgwGjtNzXkoLUnz60vEqrks3m+A0P6d+H9nz9xHwNyA+wKBgQCi\n6+VKmkZTGVUHAolYO9Eq3cPbFFEMpWbqIu3LCQskkJbAzvXok7iak2/GylCl2vDc\nxsPtzzVX26egiUBASFkgW0WRXrYYs0Y1xwUR7L9kcSx0UcowywJMllcT/NDVZdkg\nEHgEiaquIIm47Egkfuib8zYtMnkmSjlyeYmNTmzPQQKBgQDGhHVrXPk92ch+sdc0\nui/QFfBTkivi+bDGDfuOvhvqS+GmPgUUUSnB8HmP/LjSb/4Wt1Ug/a9ECw23MKeH\nFICcp7Mxq6lLxdiDi2G6+v2LfQ4ApeMLnDXoodZ3LLAKYf9KvGBdcO7PEEE3J4Bs\nJ/RKWlNlDg/ZgNGnXxEuX8CH2g==\n-----END PRIVATE KEY-----\n"",
                ""client_email"": ""firebase-adminsdk-fbsvc@game-of-duck-multiplayer.iam.gserviceaccount.com"",
                ""client_id"": ""106505944126890827034"",
                ""auth_uri"": ""https://accounts.google.com/o/oauth2/auth"",
                ""token_uri"": ""https://oauth2.googleapis.com/token"",
                ""auth_provider_x509_cert_url"": ""https://www.googleapis.com/oauth2/v1/certs"",
                ""client_x509_cert_url"": ""https://www.googleapis.com/robot/v1/metadata/x509/firebase-adminsdk-fbsvc%40game-of-duck-multiplayer.iam.gserviceaccount.com"",
                ""universe_domain"": ""googleapis.com""
            }";

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