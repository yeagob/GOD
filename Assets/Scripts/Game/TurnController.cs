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

	public Player CurrentPlayer
	{
		get
		{
			if (_players == null || _players.Count == 0)
				return null;

			return _players[_currentIndex];
		}
	}

	public List<Player> Players 
	{ 
		get 
		{ 
			return _players; 
		} 
		
		set 
		{
			_players = value;
			if (!_players.Contains(CurrentPlayer))
				_currentIndex = 0;
		} 
	}

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
	}

	//Esto no va aqui!!
	internal void DestroyPlayerTokens()
	{
		foreach (Player player in _players)
			GameObject.Destroy(player.Token.gameObject);
	}


	#endregion
}
