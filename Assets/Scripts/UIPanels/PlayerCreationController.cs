using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.EventSystems;
using System;
using UnityEditorInternal;

public class PlayerCreationController : MonoBehaviour
{
	[Serializable]
	struct PlayerList
	{
		public string PlayerName;
		public int ColorId;
	}
	[Serializable]
	private struct PlayerListWrapper
	{
		public List<PlayerList> Players;
	}

	#region Fields

	[SerializeField] private TMP_InputField[] _playerNameInputs;
	[SerializeField] private Image[] _playerImageColors;
	[SerializeField] private Button _okButton;
	[SerializeField] private PlayerToken _playerTokenPrefab;
	private Color[] _playerColors = { Color.red, Color.blue, Color.green, Color.yellow, Color.magenta, Color.cyan, Color.black };
	private List<Player> _players = new List<Player>();

	private bool _showingPanel;

	private const string PLAYERS_KEY = "players";

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


	internal async Task<List<Player>> GetPlayers()
	{		
		ShowInputPlayers();

		while(_showingPanel)
			await Task.Yield();

		return _players;

	}

	private void ShowInputPlayers()
	{
		LoadPlayers();

		EventSystem.current.SetSelectedGameObject(_playerNameInputs[0].gameObject);

		_showingPanel = true;
		gameObject.SetActive(true);
	}

	private void CloseInputPlayers()
	{
		SavePlayers();
		_showingPanel = false;
		gameObject.SetActive(false);
	}

	/// <summary>
	/// Load the players data from PlayerPrefs and populate the input fields.
	/// </summary>
	public void LoadPlayers()
	{
		if (PlayerPrefs.HasKey(PLAYERS_KEY))
		{
			string json = PlayerPrefs.GetString(PLAYERS_KEY);
			PlayerListWrapper playerData = JsonUtility.FromJson<PlayerListWrapper>(json);

			foreach (var player in playerData.Players)
			{
				if (player.ColorId >= 0 && player.ColorId < _playerNameInputs.Length)
				{
					_playerNameInputs[player.ColorId].text = player.PlayerName;
				}
			}
		}
	}

	/// <summary>
	/// Save the players data to PlayerPrefs as JSON.
	/// </summary>
	private void SavePlayers()
	{
		List<PlayerList> players = new List<PlayerList>();

		for (int i = 0; i < _playerNameInputs.Length; i++)
		{
			if (!string.IsNullOrEmpty(_playerNameInputs[i].text))
			{
				players.Add(new PlayerList
				{
					PlayerName = _playerNameInputs[i].text,
					ColorId = i
				});
			}
		}

		string json = JsonUtility.ToJson(new PlayerListWrapper { Players = players });
		PlayerPrefs.SetString(PLAYERS_KEY, json);
		PlayerPrefs.Save();
	}

	#endregion

}
