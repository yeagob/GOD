using System;
using System.Collections.Generic;
using Network.Repositories;

namespace Network.Models
{
    public class MatchModel : IMatchModel
    {
        private readonly IMatchRepository _matchRepository;

        public MatchModel(IMatchRepository matchRepository)
        {
            _matchRepository = matchRepository;
        }

        public void CreateMatch(string url, int state, Action<MatchData> callback = null)
        {
            string matchId = _matchRepository.GenerateMatchId();
            MatchData matchData = new MatchData(matchId, url, state);
            
            _matchRepository.CreateMatch(matchData, success => {
                if (success)
                {
                    callback?.Invoke(matchData);
                }
                else
                {
                    callback?.Invoke(default);
                }
            });
        }

        public void UpdateMatchState(string matchId, int newState, Action<bool> callback = null)
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
                
                _matchRepository.UpdateMatch(updatedMatch, callback);
            });
        }

        public void GetMatch(string matchId, Action<MatchData> callback)
        {
            _matchRepository.GetMatch(matchId, callback);
        }

        public void ListenForMatchChanges(string matchId, Action<MatchData> callback)
        {
            _matchRepository.ListenForMatchChanges(matchId, callback);
        }

        public void StopListeningForMatch(string matchId)
        {
            _matchRepository.StopListeningForMatch(matchId);
        }
    }
}