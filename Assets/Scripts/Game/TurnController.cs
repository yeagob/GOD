using System.Collections.Generic;

public class TurnController
{
	#region Fields
	private List<Player> _players;
	private int _currentIndex = 0;
	#endregion

	#region Properties
	public Player CurrentPlayer => _players[_currentIndex];
	#endregion

	#region Public Methods
	public TurnController(List<Player> players)
	{
		_players = players;
	}

	public void NextTurn()
	{
		_currentIndex = (_currentIndex + 1) % _players.Count;
	}
	#endregion
}
