using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TurnController
{
	#region Fields

	[SerializeField] private List<Player> _players;
	private int _currentIndex = 0;
	PopupsController _popupsController;
	
	#endregion

	#region Properties
	
	public Player CurrentPlayer => _players[_currentIndex];

	public List<Player> Players { get => _players; set => _players = value; }

	#endregion

	#region Public Methods
	public TurnController(List<Player> players, PopupsController popupsController)
	{
		_popupsController = popupsController;//???????
		_players = players;
	}

	public void NextTurn()
	{
		_currentIndex = (_currentIndex + 1) % _players.Count;
		if (GameController.GameState == GameStateState.EndGame)
		{
			foreach (Player player in _players)
			{
				if (player.CurrentTile.TileType == TileType.End)
					player.Token.Win();
				else
					player.Token.Loose();
			}
		}
	}

	//Esto no va aqui!!
	internal void DestroyPlayerTokens()
	{
		foreach (Player player in _players)
			GameObject.Destroy(player.Token.gameObject);
	}

	
	#endregion
}
