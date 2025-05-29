using System;

public class GameStateManager 
{
    private static GameStateState _prevGameState;
    private static GameStateState _gameState;

    public static GameStateState GameState
    {
        get => _gameState;
        set
        {
            if (_prevGameState != _gameState)
            {
                _prevGameState = _gameState;
            }
            _gameState = value;
        }
    }

    public static GameStateState PreviousGameState => _prevGameState;

    public event Action<GameStateState, GameStateState> OnGameStateChanged;

    public void SetGameState(GameStateState newState)
    {
        var oldState = _gameState;
        GameState = newState;
        OnGameStateChanged?.Invoke(oldState, newState);
    }

    public bool IsInState(GameStateState state)
    {
        return _gameState == state;
    }

    public bool WasInState(GameStateState state)
    {
        return _prevGameState == state;
    }

    public void ResetToWelcome()
    {
        SetGameState(GameStateState.Welcome);
    }
}
