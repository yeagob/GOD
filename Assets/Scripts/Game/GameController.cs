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

	public Player CurrentPlayer { get; private set; }

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
		_turnController = new TurnController(players);	
		CurrentPlayer = _turnController.CurrentPlayer;
	}

	#endregion

	#region Private Methods

	private void CreateBoard(BoardData board)
	{
			_boardController = new BoardController(board, _boardCreator);
	}

	#endregion
}

