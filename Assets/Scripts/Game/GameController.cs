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
				bool completed = await _popupsController.ShowChallengePlayer(CurrentPlayer);
				if (!completed)
				{
					_turnController.NextTurn();
					continue;
				}
			}

			//Roll Dice
			int diceValue = await _diceController.RollDice();
			await _popupsController.ShowPlayerDiceValue(diceValue);

			//Move Token
			Tile targetTile = await _boardController.MoveToken(CurrentPlayer, diceValue);

			await ApplyTileEffect(targetTile);

			_turnController.NextTurn();

		}
	}

	private async Task ApplyTileEffect(Tile targetTile)
	{
		switch (targetTile.TileType)
		{
			case TileType.Challenge:
				break;
			case TileType.Question:
				break;
			case TileType.TravelToTile:
				break;
			case TileType.LoseTurnsUntil:
				break;
			case TileType.RollDicesAgain:
				break;
		}

		await Task.Yield();
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

