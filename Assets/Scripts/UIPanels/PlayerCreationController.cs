using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
using System.Threading.Tasks;

public class PlayerCreationController : MonoBehaviour
{

	#region Fields

	[SerializeField] private InputField[] _playerNameInputs;
	[SerializeField] private Button _okButton;
	private Color[] _playerColors = { Color.red, Color.blue, Color.green, Color.yellow, Color.magenta, Color.cyan, Color.black };
	private List<Player> _players = new List<Player>();

	private bool _showingPanel;

	#endregion

	#region Unity Callbacks

	private void Awake()
	{
		gameObject.SetActive(false);
	}

	private void Start()
	{
		_okButton.onClick.AddListener(CreatePlayers);
	}

	#endregion

	#region Private Methods
	private void CreatePlayers()
	{
		for (int i = 0; i < _playerNameInputs.Length; i++)
		{
			if (!string.IsNullOrEmpty(_playerNameInputs[i].text))
			{
				PlayerToken token = new PlayerToken(_playerNameInputs[i].text, _playerColors[i], null);
				_players.Add(new Player(_playerNameInputs[i].text, token));
			}
		}

		CloseInputPlayers();
	}


	internal async Task<List<Player>> GetPlayers(List<string> players)
	{		
		ShowInputPlayers(players);

		while(_showingPanel)
			await Task.Yield();

		return _players;

	}

	private void ShowInputPlayers(List<string> players)
	{
		for (int i = 0; i < players.Count; i++)
		{
			if (_playerNameInputs.Length > i)
				_playerNameInputs[i].text = players[i];
		}

		_showingPanel = true;
		gameObject.SetActive(true);
	}
	private void CloseInputPlayers()
	{
		_showingPanel = false;
		gameObject.SetActive(false);

	}
	#endregion
}
