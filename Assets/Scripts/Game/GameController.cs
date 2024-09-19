using Sirenix.OdinInspector;
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
	[SerializeField] private EmailSender _emailSender;
	[SerializeField, ReadOnly] private TurnController _turnController;

	private AIJsonGenerator _aiJsonGenerator;
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

		await _popupsController.ShowBoardDataPopup(_boardController.BoardData);

		List<Player> players = await _popupsController.PlayerCreationController.GetPlayers(playersBoardData.players);
		_turnController = new TurnController(players, _popupsController);	
		MovePlayerToInitialTile(players);

		//Send me the board
		_emailSender.SendEmail(players[0].Name, playersBoardData.board);

		await GameLoop();

		await _popupsController.ShowGenericMessage("Ha Ganado "+CurrentPlayer.Name+"!!!", 10);
		await _popupsController.ShowGenericMessage("Ahora se creara el Nivel 2 de este tablero!!", 10);
		await _popupsController.ShowGenericMessage("No, que va, hasta aqui llega la demo, te reinicio la partida por si quieres echar otra...", 10);

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
				if (playAgain)
					CurrentPlayer.State = PlayerState.PlayAgain;
				else
					CurrentPlayer.State = PlayerState.OnQuestion;

				return playAgain;

			case TileType.TravelToTile:				
				await _popupsController.ShowGenericMessage("De pato a pato y tiro porque...\n CUACK!!", 2, CurrentPlayer.Token.Color);
				await _boardController.TravelToNextTravelTile(CurrentPlayer);
				CurrentPlayer.State = PlayerState.PlayAgain;
				return true;

			case TileType.LoseTurnsUntil:
				await _popupsController.ShowGenericMessage("Tu patito se ha perdido!!\n Pierdes un turno.", 5, Color.gray);
				CurrentPlayer.State = PlayerState.LostTurn;
				break;

			case TileType.RollDicesAgain:
				CurrentPlayer.State = PlayerState.PlayAgain;
				return true;
			case TileType.Die:
				CurrentPlayer.Token.MoveToTile(_boardController.BoardTiles[0]);
				await _popupsController.ShowGenericMessage("Casilla de la muerte.\n Vuelves al principio :(", 5, Color.black);
				CurrentPlayer.State = PlayerState.Waiting;
				return false;
			case TileType.End:
				return true;
		}

		return false;
	}

	//TODO: Move to Board!!
	private void MovePlayerToInitialTile(List<Player> players)
	{
		foreach (Player player in players)
		{
			player.State = PlayerState.Waiting;
			player.Token.MoveToTile(_boardController.BoardTiles[0]);
		}
	}

	#endregion

	#region Private Methods

	private void CreateBoard(BoardData boardData)
	{
			_boardController = new BoardController(boardData, _boardCreator);
	}

	#endregion
}

