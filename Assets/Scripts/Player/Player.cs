using Sirenix.OdinInspector;
using System;
using UnityEngine;

[Serializable]
public class Player
{
	#region Fields

	[SerializeField, ReadOnly] private int _id;
	[SerializeField,ReadOnly] private string _name;
	[SerializeField, ReadOnly] private PlayerState _state;
	private PlayerToken _token;

	#endregion

	#region Properties

	public string Name => _name;
	public PlayerState State { get => _state; set => _state = value; }
	public PlayerToken Token => _token;
	public Tile CurrentTile => Token.CurrentTile; 

	public int Id { get => _id; set => _id = value; }

	#endregion

	#region Public Methods

	public Player(string name, PlayerToken token, int i)
	{
		_name = name;
		_token = token;
		_state = PlayerState.Waiting;
		_id = i;
	}

	#endregion
}
