using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.EventSystems;

public class PlayerCreationController : MonoBehaviour
{

	#region Fields

	[SerializeField] private TMP_InputField[] _playerNameInputs;
	[SerializeField] private Image[] _playerImageColors;
	[SerializeField] private Button _okButton;
	[SerializeField] private PlayerToken _playerTokenPrefab;
	private Color[] _playerColors = { Color.red, Color.blue, Color.green, Color.yellow, Color.magenta, Color.cyan, Color.black };
	private List<Player> _players = new List<Player>();

	private bool _showingPanel;

	#endregion

	#region Unity Callbacks

	private void Awake()
	{
		for (int i = 0; i < _playerImageColors.Length; i++)
		{
			_playerImageColors[i].color = _playerColors[i];
		}
	}

	private void Start()
	{
		_okButton.onClick.AddListener(CreatePlayers);
	}


	private void Update()
	{
		bool play = false;
		for (int i = 0; i < _playerNameInputs.Length; i++)
			if (_playerNameInputs[i].text != "")
				play = true;
		_okButton.gameObject.SetActive(play);
	}
	#endregion

	#region Private Methods
	private void CreatePlayers()
	{
		for (int i = 0; i < _playerNameInputs.Length; i++)
		{
			if (!string.IsNullOrEmpty(_playerNameInputs[i].text))
			{
				PlayerToken token = Instantiate(_playerTokenPrefab);
				token.Initialize(_playerNameInputs[i].text, _playerColors[i]);
				token.transform.position = new Vector3(token.transform.position.x, ((float)i / 10f), token.transform.position.z);

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
		if (players != null)
		{
			for (int i = 0; i < players.Count; i++)
			{
				if (_playerNameInputs.Length > i)
					_playerNameInputs[i].text = players[i];
			}
		}

		EventSystem.current.SetSelectedGameObject(_playerNameInputs[0].gameObject);

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
