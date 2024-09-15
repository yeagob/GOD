using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Manages the game cycle and holds references to the Board and BoardController.
/// </summary>
public class GameController: MonoBehaviour
{
	#region Fields

	[SerializeField] private DiceController _diceController;
	[SerializeField] private BoardController _boardController;
	[SerializeField] private GameOfDuckBoardCreator _boardCreator;
	[SerializeField] private PopupsController _popupsController;
	[SerializeField] private AIJsonGenerator _aiJsonGenerator;
	private TurnController _turnController;

	private int _round = 1;

	#endregion

	#region Properties

	public BoardController BoardController { get; private set; }
	public TurnController TurnController { get => _turnController;  }

	private Player CurrentPlayer => _turnController.CurrentPlayer;
	private Tile CurrentTile => CurrentPlayer.CurrentTile;

	public int Round => _round;

	#endregion

	#region Unity Callbacks

	private async void Start()
	{
		//Welcome
		string [] answers = await _popupsController.ShowWelcome();
		_aiJsonGenerator = new AIJsonGenerator(answers[0], answers[1]);
		//Generate Board
		PlayersBoardData playersBoardData = await _aiJsonGenerator.GetJsonBoardAndPlayers();
		CreateBoard(playersBoardData.board);
		_popupsController.HideWelcome();

		List<Player> players = await _popupsController.PlayerCreationController.GetPlayers(playersBoardData.players);
		_turnController = new TurnController(players, _popupsController);	
		MovePlayerToInitialTile(players);

		await GameLoop();
	}

	private async Task GameLoop()
	{
		while(CurrentTile.TileType != TileType.End)//TODO: Game States...
		{
			//Lost Turn Control
			if (CurrentPlayer.State == PlayerState.LostTurn)
			{
				CurrentPlayer.State = PlayerState.Waiting;
				_turnController.NextTurn();
				continue;
			}

			//Show player Turn
			await _popupsController.ShowPlayerTurn(CurrentPlayer);

			//Player on Challenge
			if (CurrentPlayer.State == PlayerState.OnChallenge)
			{
				bool completed = await _popupsController.ShowChallengePlayer(CurrentPlayer, false);
				if (!completed)
				{
					_turnController.NextTurn();
					continue;
				}
			}
			
			//Player on Question Tile
			if (CurrentPlayer.State == PlayerState.OnQuestion)
			{
				bool play = await _popupsController.ShowQuestion(CurrentTile.TileData.question);
				if (!play)
				{
					_turnController.NextTurn();
					continue;
				}
			}

			CurrentPlayer.State = PlayerState.Playing;

			//Roll Dice
			int diceValue = await _diceController.RollDice();
			await _popupsController.ShowPlayerDiceValue(diceValue);

			//Move Token
			Tile targetTile = await _boardController.MoveToken(CurrentPlayer, diceValue);

			bool playAgain = await ApplyTileEffect(targetTile);

			if (playAgain)
				continue;

			CurrentPlayer.State = PlayerState.Waiting;

			_turnController.NextTurn();

		}
	}

	private async Task<bool> ApplyTileEffect(Tile targetTile)
	{
		switch (targetTile.TileType)
		{
			case TileType.Challenge:
				CurrentPlayer.State = PlayerState.OnChallenge;
				await _popupsController.ShowChallengePlayer(CurrentPlayer, true);
				return false;

			case TileType.Question:
				bool playAgain = await _popupsController.ShowQuestion(targetTile.TileData.question);
				if (!playAgain)
					CurrentPlayer.State = PlayerState.OnQuestion;
				return playAgain;

			case TileType.TravelToTile:
				await _boardController.TravelToNextTravelTile(CurrentPlayer);
				return true;
			case TileType.LoseTurnsUntil:
				CurrentPlayer.State = PlayerState.LostTurn;
				break;
			case TileType.RollDicesAgain:
				return true;
		}

		return false;
	}

	private void MovePlayerToInitialTile(List<Player> players)
	{
		foreach (var item in players)
		{
			item.Token.MoveToTile(_boardController.BoardTiles[0]);
		}
	}

	#endregion

	#region Private Methods

	private void CreateBoard(BoardData board)
	{
			_boardController = new BoardController(board, _boardCreator);
	}

	#endregion
}

