
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the board by controlling its tiles and challenges.
/// </summary>
[Serializable]
public class BoardController
{
	#region Fields

	[SerializeField] private BoardData _board;
	private List<Tile> _boardTiles = new List<Tile>();

	#endregion

	#region Properties

	public BoardData Board { get; private set; }
	public List<Tile> BoardTiles { get => _boardTiles; }

	#endregion

	#region Constructors

	public BoardController(string jsonData, GameOfDuckBoardCreator boardCreator)
	{
		// Deserialize JSON and construct Board
		_board = new BoardData(jsonData);
		_boardTiles = boardCreator.GetBoard(_board);
	}

	#endregion

	#region Private Methods
	#endregion
}
