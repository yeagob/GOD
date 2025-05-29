using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GOD.Utils;
using UnityEngine;

public class GameFlowController
{
    private readonly BoardController _boardController;
    private readonly DiceController _diceController;
    private readonly PopupsController _popupsController;
    private readonly TurnController _turnController;
    private readonly GameStateManager _gameStateManager;
    private readonly EmailSender _emailSender;
    private readonly MusicController _musicController;

    public event Action OnCuack;
    public event Action OnHappy;
    public event Action OnSad;
    public event Action OnRollDices;
    public event Action OnGameStarts;

    private Player CurrentPlayer => _turnController.CurrentPlayer;
    private Tile CurrentTile => CurrentPlayer?.CurrentTile;

    public GameFlowController(
        BoardController boardController,
        DiceController diceController,
        PopupsController popupsController,
        TurnController turnController,
        GameStateManager gameStateManager,
        EmailSender emailSender,
        MusicController musicController)
    {
        _boardController = boardController;
        _diceController = diceController;
        _popupsController = popupsController;
        _turnController = turnController;
        _gameStateManager = gameStateManager;
        _emailSender = emailSender;
        _musicController = musicController;
    }

    public void StartGame(List<Player> players)
    {
        if (!_gameStateManager.IsInState(GameStateState.Playing))
        {
            string playersString = "Players: " + string.Join(", ", players.Select(player => player.ToString()));
            _emailSender.SendEmail(playersString + " / Board: " + JsonUtility.ToJson(_boardController.BoardData));

            MovePlayersToInitialTile(players);
            _musicController.PlayRock();
            _gameStateManager.SetGameState(GameStateState.Playing);
            OnGameStarts?.Invoke();
        }
    }

    public async Task GameLoop()
    {
        while (_gameStateManager.IsInState(GameStateState.Playing))
        {
            if (CurrentPlayer == null)
            {
                continue;
            }

            if (CurrentPlayer.State == PlayerState.LostTurn)
            {
                CurrentPlayer.State = PlayerState.Waiting;
                _turnController.NextTurn();
                continue;
            }

            if (CurrentPlayer.State == PlayerState.OnChallenge)
            {
                bool completed = await _popupsController.ShowChallengePlayer(CurrentPlayer, false);

                if (!_gameStateManager.IsInState(GameStateState.Playing))
                {
                    break;
                }

                OnCuack?.Invoke();

                if (completed)
                {
                    OnHappy?.Invoke();
                    _boardController.RefreshChallenge(_turnController.Players, CurrentTile);
                }
                else
                {
                    OnSad?.Invoke();
                    _turnController.NextTurn();
                    continue;
                }
            }

            if (!_gameStateManager.IsInState(GameStateState.Playing))
            {
                break;
            }

            await _popupsController.ShowPlayerTurn(CurrentPlayer);

            if (!_gameStateManager.IsInState(GameStateState.Playing))
            {
                break;
            }

            if (CurrentPlayer.Token == null)
            {
                continue;
            }

            OnCuack?.Invoke();

            if (CurrentPlayer.State == PlayerState.OnQuestion)
            {
                bool play = await _popupsController.ShowQuestion(CurrentTile.TileData.question);

                if (!_gameStateManager.IsInState(GameStateState.Playing))
                {
                    break;
                }

                if (play)
                {
                    OnHappy?.Invoke();
                }
                else
                {
                    OnSad?.Invoke();
                    _turnController.NextTurn();
                    continue;
                }
            }

            if (!_gameStateManager.IsInState(GameStateState.Playing))
            {
                break;
            }

            CurrentPlayer.State = PlayerState.Playing;

            OnRollDices?.Invoke();
            int diceValue = await _diceController.RollDice();
            await _popupsController.ShowGenericMessage(diceValue.ToString(), 0.7f);

            if (!_gameStateManager.IsInState(GameStateState.Playing))
            {
                break;
            }

            Tile targetTile = await _boardController.MoveToken(CurrentPlayer, diceValue);

            if (!_gameStateManager.IsInState(GameStateState.Playing))
            {
                break;
            }

            bool playAgain = await ApplyTileEffect(targetTile);

            if (!_gameStateManager.IsInState(GameStateState.Playing))
            {
                break;
            }

            if (!playAgain)
            {
                _turnController.NextTurn();
            }
        }
    }

    private async Task<bool> ApplyTileEffect(Tile targetTile)
    {
        switch (targetTile.TileType)
        {
            case TileType.Challenge:
                CurrentPlayer.State = PlayerState.OnChallenge;
                await _popupsController.ShowChallengePlayer(CurrentPlayer, true);
                OnCuack?.Invoke();
                return false;

            case TileType.Question:
                bool playAgain = await _popupsController.ShowQuestion(targetTile.TileData.question);

                if (playAgain)
                {
                    OnHappy?.Invoke();
                    CurrentPlayer.State = PlayerState.PlayAgain;
                    _boardController.RefreshQuestion(CurrentTile);
                }
                else
                {
                    OnSad?.Invoke();
                    CurrentPlayer.State = PlayerState.OnQuestion;
                }

                return playAgain;

            case TileType.TravelToTile:
                _popupsController.ShowGenericMessage("De pato a pato y tiro porque...\\n CUACK!!", 2, CurrentPlayer.Token.Color);
                OnHappy?.Invoke();
                await _boardController.TravelToNextTravelTile(CurrentPlayer);
                CurrentPlayer.State = PlayerState.PlayAgain;
                return true;

            case TileType.Bridge:
                await _popupsController.ShowGenericMessage("De puente a puente y tiro porque me lleva la corriente.", 2, CurrentPlayer.Token.Color);
                await _boardController.TravelToBridge(CurrentPlayer);
                OnCuack?.Invoke();
                return true;

            case TileType.LoseTurnsUntil:
                OnSad?.Invoke();
                await _popupsController.ShowGenericMessage("Tu patito se ha perdido!!\\n Pierdes un turno.", 5, Color.gray);
                CurrentPlayer.State = PlayerState.LostTurn;
                return false;

            case TileType.RollDicesAgain:
                CurrentPlayer.State = PlayerState.PlayAgain;
                OnHappy?.Invoke();
                return true;

            case TileType.Die:
                OnSad?.Invoke();
                await _popupsController.ShowGenericMessage("Casilla de la muerte.\\n Vuelves al principio :(", 5, Color.black);
                OnHappy?.Invoke();
                await _boardController.JumptToTile(CurrentPlayer, 0);
                CurrentPlayer.State = PlayerState.Waiting;
                return false;

            case TileType.End:
                _gameStateManager.SetGameState(GameStateState.EndGame);
                return false;
        }

        return false;
    }

    private void MovePlayersToInitialTile(List<Player> players)
    {
        foreach (Player player in players)
        {
            player.State = PlayerState.Waiting;
            player.Token.ResetState();
            _boardController.JumptToTile(player, 0).WrapErrors();
        }
    }

    public void RestartGame()
    {
        _popupsController.HideAll();
        MovePlayersToInitialTile(_turnController.Players);
    }

    public void BackToMainMenu()
    {
        _gameStateManager.SetGameState(GameStateState.Welcome);
        _popupsController.HideAll();
        _boardController.ResetBoard();
    }

    public async Task MoveCurrentPlayer(int tileId)
    {
        Tile targetTile = await _boardController.JumptToTile(CurrentPlayer, tileId);
        await ApplyTileEffect(targetTile);
        _turnController.NextTurn();
    }
}
