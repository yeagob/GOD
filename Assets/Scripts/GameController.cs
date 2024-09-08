using System;
using UnityEngine;

/// <summary>
/// Manages the game cycle and holds references to the Board and BoardController.
/// </summary>
public class GameController: MonoBehaviour
{
	#region Fields

	[SerializeField] private string _json;

	[SerializeField] private BoardController _boardController;
	[SerializeField] private GameOfDuckBoardCreator _boardCreator;

	#endregion

	#region Properties

	public BoardController BoardController { get; private set; }

	#endregion

	#region Constructors

	public void Start()
	{
		if (_json != "")
			_boardController = new BoardController(_json, _boardCreator);
	}

	#endregion
}

