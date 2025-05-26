using System;
using System.Threading.Tasks;
using UnityEngine;

public class GameEventPresenter : MonoBehaviour
{
    private readonly GameEventModel gameEventModel;
    private readonly PlayerMatchModel playerMatchModel;
    private readonly MatchModel matchModel;
    
    public event Action<GameEventData> OnEventLogged;
    public event Action<string, int, int> OnPlayerMoveLogged;
    public event Action<string, int> OnDiceRollLogged;
    public event Action<string, string, string> OnSpecialTileLogged;
    public event Action<string> OnError;
    
    public GameEventPresenter(GameEventModel gameEventModel, PlayerMatchModel playerMatchModel, MatchModel matchModel)
    {
        this.gameEventModel = gameEventModel ?? throw new ArgumentNullException(nameof(gameEventModel));
        this.playerMatchModel = playerMatchModel ?? throw new ArgumentNullException(nameof(playerMatchModel));
        this.matchModel = matchModel ?? throw new ArgumentNullException(nameof(matchModel));
    }
    
    public async Task LogPlayerMove(string matchId, string playerId, int fromPosition, int toPosition, int turn)
    {
        try
        {
            var eventData = await gameEventModel.LogPlayerMove(matchId, playerId, fromPosition, toPosition, turn);
            await playerMatchModel.UpdatePlayerPosition(playerId, matchId, toPosition);
            
            OnEventLogged?.Invoke(eventData);
            OnPlayerMoveLogged?.Invoke(playerId, fromPosition, toPosition);
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Failed to log player move: {ex.Message}");
        }
    }
    
    public async Task LogDiceRoll(string matchId, string playerId, int diceValue, int turn)
    {
        try
        {
            var eventData = await gameEventModel.LogDiceRoll(matchId, playerId, diceValue, turn);
            
            OnEventLogged?.Invoke(eventData);
            OnDiceRollLogged?.Invoke(playerId, diceValue);
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Failed to log dice roll: {ex.Message}");
        }
    }
    
    public async Task LogSpecialTile(string matchId, string playerId, string tileType, string effect, int turn)
    {
        try
        {
            var eventData = await gameEventModel.LogSpecialTile(matchId, playerId, tileType, effect, turn);
            
            OnEventLogged?.Invoke(eventData);
            OnSpecialTileLogged?.Invoke(playerId, tileType, effect);
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Failed to log special tile: {ex.Message}");
        }
    }
    
    public async Task GetEvent(string eventId)
    {
        try
        {
            var eventData = await gameEventModel.GetEvent(eventId);
            if (!string.IsNullOrEmpty(eventData.EventId))
            {
                OnEventLogged?.Invoke(eventData);
            }
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Failed to get event: {ex.Message}");
        }
    }
}
