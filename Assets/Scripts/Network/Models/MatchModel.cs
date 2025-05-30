using System;
using Network.Repositories;
using Network.Infrastructure;
using UnityEngine;

namespace Network.Models
{
    public class MatchModel : IMatchModel
    {
        private readonly IMatchRepository _matchRepository;
        private LocalMatchState _currentMatchState;

        public LocalMatchState CurrentMatchState => _currentMatchState;

        public MatchModel(IMatchRepository matchRepository)
        {
            _matchRepository = matchRepository;
            _currentMatchState = LocalMatchState.CreateEmptyState();
        }

        public void CreateMatch(string url, MatchState state, Action<MatchData> callback = null)
        {
            string matchId = _matchRepository.GenerateMatchId();
            CreateMatch(matchId, url, state, callback);
        }
        
        public void CreateMatch(string matchId, string url, MatchState state, Action<MatchData> callback = null)
        {
            MatchData matchData = new MatchData(matchId, url, MatchState.WaitingForPlayers);
            
            _matchRepository.CreateMatch(matchData, success => {
                if (success)
                {
                    SetAsHost(matchData);
                    callback?.Invoke(matchData);
                }
                else
                {
                    callback?.Invoke(default);
                }
            });
        }

        public void UpdateMatchState(string matchId, MatchState newState, Action<bool> callback = null)
        {
            _matchRepository.GetMatch(matchId, existingMatch => {
                if (string.IsNullOrEmpty(existingMatch._id))
                {
                    callback?.Invoke(false);
                    return;
                }

                MatchData updatedMatch = new MatchData(
                    existingMatch._id,
                    existingMatch._url,
                    newState
                );
                
                _matchRepository.UpdateMatch(updatedMatch, success => {
                    if (success && matchId == _currentMatchState.CurrentMatch._id)
                    {
                        UpdateLocalMatch(updatedMatch);
                    }
                    callback?.Invoke(success);
                });
            });
        }

        public void GetMatch(string matchId, Action<MatchData> callback)
        {
            _matchRepository.GetMatch(matchId, callback);
        }

        public void ListenForMatchChanges(string matchId, Action<MatchData> callback)
        {
            _matchRepository.ListenForMatchChanges(matchId, matchData => {
                if (matchId == _currentMatchState.CurrentMatch._id)
                {
                    UpdateLocalMatch(matchData);
                }
                callback?.Invoke(matchData);
            });
        }

        public void StopListeningForMatch(string matchId)
        {
            _matchRepository.StopListeningForMatch(matchId);
        }

        public void SetAsHost(MatchData matchData)
        {
            _currentMatchState = LocalMatchState.CreateHostState(matchData);
        }

        public void SetAsClient(MatchData matchData)
        {
            _currentMatchState = LocalMatchState.CreateClientState(matchData);
        }

        public void UpdateLocalMatch(MatchData matchData)
        {
            _currentMatchState.UpdateMatchData(matchData);
        }

        public void ClearMatchState()
        {
            _currentMatchState = LocalMatchState.CreateEmptyState();
        }

        public bool IsCurrentlyInMatch()
        {
            return _currentMatchState.IsInMatch;
        }

        public bool IsHost()
        {
            return _currentMatchState.IsHost;
        }
    }
}