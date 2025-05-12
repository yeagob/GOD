namespace Network
{
    public class Game
    {
        
    }


    /*
using Firebase.Database;
using UnityEngine; // Needed for Debug.LogError

public class FirebaseDatabaseService : IFirebaseDatabaseService
{
    private DatabaseReference _dbReference;
    private Dictionary<string, ValueChangedEventHandler> _matchListeners = new Dictionary<string, ValueChangedEventHandler>();
    private Dictionary<string, ValueChangedEventHandler> _playersListeners = new Dictionary<string, ValueChangedEventHandler>();
    private Dictionary<string, ChildAddedEventHandler> _eventsListeners = new Dictionary<string, ChildAddedEventHandler>();
     private ValueChangedEventHandler _availableMatchesListener;
     private DatabaseReference _availableMatchesRef;


    public FirebaseDatabaseService()
    {
        // Initialize Firebase App elsewhere, typically once at application start
        // Example:
        // Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
        //     if (task.Result == Firebase.DependencyStatus.Available) {
        //         _dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        //         Debug.Log("Firebase Initialized");
        //     } else {
        //         Debug.LogError("Could not resolve Firebase dependencies: " + task.Result);
        //     }
        // });
         _dbReference = FirebaseDatabase.DefaultInstance.RootReference; // Assume initialized
    }

    public void CreateMatch(MatchData match, Action<bool, string> onComplete)
    {
        string matchKey = _dbReference.Child("matches").Push().Key;
        match._id = matchKey;
        string json = JsonUtility.ToJson(match);
        _dbReference.Child("matches").Child(matchKey).SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                onComplete?.Invoke(true, matchKey);
            }
            else
            {
                Debug.LogError("Failed to create match: " + task.Exception);
                onComplete?.Invoke(false, null);
            }
        });
    }

     public void ListenForAvailableMatches(Action<List<MatchData>> onDataChanged)
    {
        _availableMatchesRef = _dbReference.Child("matches");
        _availableMatchesListener = (object sender, ValueChangedEventArgs args) =>
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }
            List<MatchData> matches = new List<MatchData>();
            if (args.Snapshot != null && args.Snapshot.Exists)
            {
                foreach (var childSnapshot in args.Snapshot.Children)
                {
                    MatchData match = JsonUtility.FromJson<MatchData>(childSnapshot.GetRawJsonValue());
                    if (match != null)
                    {
                         match._id = childSnapshot.Key; // Ensure ID is set from key
                        matches.Add(match);
                    }
                }
            }
            onDataChanged?.Invoke(matches);
        };
        _availableMatchesRef.ValueChanged += _availableMatchesListener;
    }

    public void StopListeningForAvailableMatches()
    {
        if (_availableMatchesRef != null && _availableMatchesListener != null)
        {
            _availableMatchesRef.ValueChanged -= _availableMatchesListener;
            _availableMatchesRef = null;
            _availableMatchesListener = null;
        }
    }

    public void JoinMatch(string matchId, PlayerMatchData player, Action<bool> onComplete)
    {
         string playerKey = _dbReference.Child("matches").Child(matchId).Child("players").Push().Key;
         player._id = playerKey; // Set player ID from Firebase Key
         player._matchId = matchId;
         string json = JsonUtility.ToJson(player);
         _dbReference.Child("matches").Child(matchId).Child("players").Child(playerKey).SetRawJsonValueAsync(json).ContinueWith(task =>
         {
             if (task.IsCompleted)
             {
                 onComplete?.Invoke(true);
             }
             else
             {
                 Debug.LogError($"Failed to join match {matchId}: " + task.Exception);
                 onComplete?.Invoke(false);
             }
         });
    }


    public void ListenForMatchStateChanges(string matchId, Action<MatchData> onDataChanged)
    {
         DatabaseReference matchRef = _dbReference.Child("matches").Child(matchId);
        ValueChangedEventHandler listener = (object sender, ValueChangedEventArgs args) =>
        {
            if (args.DatabaseError != null) { Debug.LogError(args.DatabaseError.Message); return; }
            MatchData data = JsonUtility.FromJson<MatchData>(args.Snapshot.GetRawJsonValue());
            if(data != null) data._id = args.Snapshot.Key; // Ensure ID is set
            onDataChanged?.Invoke(data);
        };
        matchRef.ValueChanged += listener;
        _matchListeners[matchId] = listener;
    }

     public void StopListeningForMatchStateChanges(string matchId)
    {
        if (_matchListeners.ContainsKey(matchId))
        {
             DatabaseReference matchRef = _dbReference.Child("matches").Child(matchId);
             matchRef.ValueChanged -= _matchListeners[matchId];
             _matchListeners.Remove(matchId);
        }
    }


    public void ListenForPlayersInMatch(string matchId, Action<List<PlayerMatchData>> onDataChanged)
    {
         DatabaseReference playersRef = _dbReference.Child("matches").Child(matchId).Child("players");
        ValueChangedEventHandler listener = (object sender, ValueChangedEventArgs args) =>
        {
            if (args.DatabaseError != null) { Debug.LogError(args.DatabaseError.Message); return; }
            List<PlayerMatchData> players = new List<PlayerMatchData>();
             if (args.Snapshot != null && args.Snapshot.Exists)
            {
                 foreach (var childSnapshot in args.Snapshot.Children)
                {
                    PlayerMatchData player = JsonUtility.FromJson<PlayerMatchData>(childSnapshot.GetRawJsonValue());
                    if (player != null)
                    {
                         player._id = childSnapshot.Key; // Ensure ID is set from key
                        players.Add(player);
                    }
                }
            }
            onDataChanged?.Invoke(players);
        };
        playersRef.ValueChanged += listener;
        _playersListeners[matchId] = listener;
    }

     public void StopListeningForPlayersInMatch(string matchId)
    {
         if (_playersListeners.ContainsKey(matchId))
        {
             DatabaseReference playersRef = _dbReference.Child("matches").Child(matchId).Child("players");
             playersRef.ValueChanged -= _playersListeners[matchId];
             _playersListeners.Remove(matchId);
        }
    }


    public void AddGameEvent(string matchId, GameEventData gameEvent, Action<bool> onComplete)
    {
         string eventKey = _dbReference.Child("matches").Child(matchId).Child("events").Push().Key;
         gameEvent._id = eventKey;
         gameEvent._matchId = matchId;
         string json = JsonUtility.ToJson(gameEvent);
         _dbReference.Child("matches").Child(matchId).Child("events").Child(eventKey).SetRawJsonValueAsync(json).ContinueWith(task =>
         {
             if (task.IsCompleted)
             {
                 onComplete?.Invoke(true);
             }
             else
             {
                 Debug.LogError($"Failed to add event to match {matchId}: " + task.Exception);
                 onComplete?.Invoke(false);
             }
         });
    }

    public void ListenForNewEventsInMatch(string matchId, Action<GameEventData> onEventAdded)
    {
         DatabaseReference eventsRef = _dbReference.Child("matches").Child(matchId).Child("events");
         // Use ChildAdded to get events as they are added
         ChildAddedEventHandler listener = (object sender, ChildChangedEventArgs args) =>
         {
             if (args.DatabaseError != null) { Debug.LogError(args.DatabaseError.Message); return; }
             GameEventData eventData = JsonUtility.FromJson<GameEventData>(args.Snapshot.GetRawJsonValue());
             if(eventData != null) eventData._id = args.Snapshot.Key; // Ensure ID is set
             onEventAdded?.Invoke(eventData);
         };
         eventsRef.ChildAdded += listener;
         _eventsListeners[matchId] = listener;
    }

    public void StopListeningForNewEventsInMatch(string matchId)
    {
        if (_eventsListeners.ContainsKey(matchId))
        {
             DatabaseReference eventsRef = _dbReference.Child("matches").Child(matchId).Child("events");
             eventsRef.ChildAdded -= _eventsListeners[matchId];
             _eventsListeners.Remove(matchId);
        }
    }


    public void UpdateMatchState(string matchId, int newState, Action<bool> onComplete)
    {
         _dbReference.Child("matches").Child(matchId).Child("state").SetValueAsync(newState).ContinueWith(task =>
         {
             onComplete?.Invoke(task.IsCompleted);
         });
    }

    public void UpdatePlayerScore(string matchId, string playerId, int newScore, Action<bool> onComplete)
    {
         _dbReference.Child("matches").Child(matchId).Child("players").Child(playerId).Child("score").SetValueAsync(newScore).ContinueWith(task =>
         {
             onComplete?.Invoke(task.IsCompleted);
         });
    }

     public void UpdatePlayerData(string matchId, string playerId, PlayerMatchData playerData, Action<bool> onComplete)
    {
        // Note: This overwrites the player data at this path
        string json = JsonUtility.ToJson(playerData);
         _dbReference.Child("matches").Child(matchId).Child("players").Child(playerId).SetRawJsonValueAsync(json).ContinueWith(task =>
         {
             onComplete?.Invoke(task.IsCompleted);
         });
    }


    public void RemoveMatch(string matchId, Action<bool> onComplete)
    {
         _dbReference.Child("matches").Child(matchId).RemoveValueAsync().ContinueWith(task =>
         {
             onComplete?.Invoke(task.IsCompleted);
         });
    }

    public void RemovePlayerFromMatch(string matchId, string playerId, Action<bool> onComplete)
    {
        _dbReference.Child("matches").Child(matchId).Child("players").Child(playerId).RemoveValueAsync().ContinueWith(task =>
         {
             onComplete?.Invoke(task.IsCompleted);
         });
    }

    // Remember to call StopListening methods when no longer needed
    // For a full implementation, you'd need proper error handling and task management
}
*/

    

}