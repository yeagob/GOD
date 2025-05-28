using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.EventSystems;
using System;
using System.Linq;
using UI.UIPopups;
using Network.Infrastructure;
using Network.Models;
using Network;

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

	[SerializeField] private MultiplayerPanel _multiplayerPanel;
	[SerializeField] private TMP_InputField[] _playerNameInputs;
	[SerializeField] private Image[] _playerImageColors;
	[SerializeField] private Button _okButton;
	[SerializeField] private Button _multiplayerButton;
	[SerializeField] private PlayerToken _playerTokenPrefab;
	private Color[] _playerColors = { Color.red, Color.blue, Color.green, Color.yellow, Color.magenta, Color.cyan, Color.black };
	private List<Player> _players = new List<Player>();
	private List<Player> _previousPlayers = new List<Player>();

	private IMatchModel _matchModel;
	private NetworkInstaller _networkInstaller;
	private const string PLAYERS_KEY = "players";

	#endregion

	#region Unity Callbacks

	private void Awake()
	{
		for (int i = 0; i < _playerImageColors.Length; i++)
		{
			_playerImageColors[i].color = _playerColors[i];
		}
		
		InitializeNetworking();
	}

	private void Start()
	{
		_okButton.onClick.AddListener(CreatePlayers);
		_multiplayerButton.onClick.AddListener(StartMultiplayer);
	}

	private void Update()
	{
		bool play = false;
		for (int i = 0; i < _playerNameInputs.Length; i++)
		{
			if (_playerNameInputs[i].text != "")
			{
				play = true;
			}
		}
		_okButton.gameObject.SetActive(play);
	}

	#endregion

	#region Private Methods

	private void InitializeNetworking()
	{
		_networkInstaller = FindObjectOfType<NetworkInstaller>();
		if (_networkInstaller == null)
		{
			GameObject networkObj = new GameObject("NetworkInstaller");
			_networkInstaller = networkObj.AddComponent<NetworkInstaller>();
			DontDestroyOnLoad(networkObj);
			Debug.Log("PlayerCreationController: Created NetworkInstaller");
		}

		StartCoroutine(WaitForNetworkInitialization());
	}

	private System.Collections.IEnumerator WaitForNetworkInitialization()
	{
		yield return new WaitForSeconds(0.5f);
		
		_matchModel = NetworkInstaller.Resolve<IMatchModel>();
		if (_matchModel == null)
		{
			Debug.LogError("PlayerCreationController: MatchModel not found after initialization!");
		}
		else
		{
			Debug.Log("PlayerCreationController: MatchModel successfully resolved");
		}
	}

	private void CreatePlayers()
	{
		for (int i = 0; i < _playerNameInputs.Length; i++)
		{
			if (!string.IsNullOrEmpty(_playerNameInputs[i].text))
			{
				PlayerToken token = Instantiate(_playerTokenPrefab);
				token.Initialize(_playerNameInputs[i].text, _playerColors[i]);
				token.transform.position = new Vector3(token.transform.position.x, ((float)i / 10f), token.transform.position.z);
				Player newPlayer = new Player(_playerNameInputs[i].text, token,i);
				if(_previousPlayers.Count > 0)
				{
					Player oldPlayer = _previousPlayers.FirstOrDefault((p) => p.Id == i);
					if (oldPlayer != null)
					{
						newPlayer.State = oldPlayer.State;
						newPlayer.Token.MoveToTile(oldPlayer.CurrentTile);
					}
				}
				_players.Add(newPlayer);
			}
		}

		if (_previousPlayers.Count > 0)
		{
			foreach (Player player in _previousPlayers)
			{
				Destroy(player.Token.gameObject);
			}
			_previousPlayers.Clear();
		}

		CloseInputPlayers();
	}

	private void StartMultiplayer()
	{
		Debug.Log("PlayerCreationController: StartMultiplayer called");

		if (_matchModel == null)
		{
			Debug.LogError("PlayerCreationController: MatchModel not initialized, attempting to resolve again...");
			_matchModel = NetworkInstaller.Resolve<IMatchModel>();
			
			if (_matchModel == null)
			{
				Debug.LogError("PlayerCreationController: Failed to resolve MatchModel. Cannot start multiplayer.");
				return;
			}
		}

		string currentUrl = GetCurrentGameUrl();
		Debug.Log($"PlayerCreationController: Creating match with URL: {currentUrl}");

		_matchModel.CreateMatch(currentUrl, 0, (matchData) => {
			HandleMatchCreationResult(matchData, currentUrl);
		});
	}

	private string GetCurrentGameUrl()
	{
		string currentUrl = Application.absoluteURL;
		
		if (string.IsNullOrEmpty(currentUrl))
		{
			currentUrl = "https://gameofduckai.netlify.app/";
			Debug.Log("PlayerCreationController: Using default URL (no absoluteURL available)");
		}
		else
		{
			Debug.Log($"PlayerCreationController: Using current URL: {currentUrl}");
		}

		return currentUrl;
	}

	private void HandleMatchCreationResult(MatchData matchData, string originalUrl)
	{
		if (!string.IsNullOrEmpty(matchData._id))
		{
			Debug.Log($"PlayerCreationController: Match created successfully!");
			Debug.Log($"PlayerCreationController: Match ID: {matchData._id}");
			Debug.Log($"PlayerCreationController: Match URL: {matchData._url}");
			Debug.Log($"PlayerCreationController: Match State: {matchData._state}");
			Debug.Log($"PlayerCreationController: Created At: {matchData._createdAt}");
			
			string multiplayerUrl = BuildMultiplayerUrl(originalUrl, matchData._id);
			Debug.Log($"PlayerCreationController: Generated multiplayer URL: {multiplayerUrl}");
			
			if (_multiplayerPanel != null)
			{
				_multiplayerPanel.gameObject.SetActive(true);
				Debug.Log("PlayerCreationController: MultiplayerPanel activated");
			}
			else
			{
				Debug.LogWarning("PlayerCreationController: MultiplayerPanel is null!");
			}
		}
		else
		{
			Debug.LogError("PlayerCreationController: Failed to create match - received empty match data");
		}
	}

	private string BuildMultiplayerUrl(string baseUrl, string matchId)
	{
		string separator = baseUrl.Contains("?") ? "&" : "?";
		return $"{baseUrl}{separator}match={matchId}&multiplayer=true";
	}

	internal async Task<List<Player>> ShowAsync(bool multiplayer, List<Player> players = null)
	{
		if (players != null)
		{
			_previousPlayers = new List<Player>(players);
		}

		_players.Clear();
		ShowInputPlayers();

		while(gameObject.activeSelf)
		{
			await Task.Yield();
		}

		return _players;
	}

	private void ShowInputPlayers()
	{
		if(GameController.GameState == GameStateState.Playing)
		{
			LoadPlayers(_previousPlayers);
		}
		else
		{
			LoadPlayers();
		}

		EventSystem.current.SetSelectedGameObject(_playerNameInputs[0].gameObject);
		gameObject.SetActive(true);
	}

	private void CloseInputPlayers()
	{
		SavePlayers();
		gameObject.SetActive(false);
	}

	public void LoadPlayers(List<Player> previousPlayers = null)
	{
		if (previousPlayers != null)
		{
			foreach (Player player in previousPlayers)
			{
				_playerNameInputs[player.Id].text = player.Name;
			}
			return;
		}

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
