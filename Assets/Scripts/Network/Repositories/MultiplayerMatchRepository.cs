using System;
using System.Threading.Tasks;
using Network.Models;
using Network.Services;
using UnityEngine;

namespace Network.Repositories
{
    public class MultiplayerMatchRepository
    {
        private readonly IFirebaseService _firebaseService;
        private const string MATCHES_PATH = "matches";
        
        public MultiplayerMatchRepository(IFirebaseService firebaseService)
        {
            _firebaseService = firebaseService ?? throw new ArgumentNullException(nameof(firebaseService));
        }

        public async Task<bool> CreateMatchAsync(MultiplayerMatchData matchData)
        {
            try
            {
                string matchPath = $"{MATCHES_PATH}/{matchData.roomId}";
                
                var tcs = new TaskCompletionSource<bool>();
                _firebaseService.SetData(matchPath, matchData, success => tcs.SetResult(success));
                
                return await tcs.Task;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error creating match: {ex.Message}");
                return false;
            }
        }

        public async Task<MultiplayerMatchData> GetMatchAsync(string roomId)
        {
            try
            {
                string matchPath = $"{MATCHES_PATH}/{roomId}";
                
                var tcs = new TaskCompletionSource<MultiplayerMatchData>();
                _firebaseService.GetData<MultiplayerMatchData>(matchPath, data => tcs.SetResult(data));
                
                return await tcs.Task;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error getting match: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateMatchStateAsync(string roomId, string newState)
        {
            try
            {
                var match = await GetMatchAsync(roomId);
                if (match != null)
                {
                    match.state = newState;
                    return await CreateMatchAsync(match);
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error updating match state: {ex.Message}");
                return false;
            }
        }

        public void ListenToMatchChanges(string roomId, Action<MultiplayerMatchData> onMatchChanged)
        {
            string matchPath = $"{MATCHES_PATH}/{roomId}";
            _firebaseService.ListenForChanges<MultiplayerMatchData>(matchPath, onMatchChanged);
        }

        public void StopListeningToMatch(string roomId)
        {
            string matchPath = $"{MATCHES_PATH}/{roomId}";
            _firebaseService.StopListening(matchPath);
        }
    }
}
