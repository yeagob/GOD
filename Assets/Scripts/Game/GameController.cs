using GOD.Utils;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// Manages the game cycle and holds references to the Board and BoardController.
/// </summary>
public class GameController : MonoBehaviour
{
	#region Fields
	[SerializeField] private string _defaultBoard = "parent.json";

	[Header("TEMP Elements")]
	[SerializeField] private bool _loadDefault = false;
	[SerializeField] private Button _saveButton;
	[SerializeField] private GameObject _winEffects;
	[SerializeField] private MusicController _musicController;

	[Header("Controllers")]
	[SerializeField] private AIJsonGenerator _aiJsonGenerator;
	[SerializeField] private DiceController _diceController;
	[SerializeField] private BoardController _boardController;
	[SerializeField] private GameOfDuckBoardCreator _boardCreator;
	[SerializeField] private PopupsController _popupsController;
	[SerializeField] private EmailSender _emailSender;
	[SerializeField] private VolumeControl _volumeControl;

	[SerializeField, ReadOnly] private TurnController _turnController;

	private int _round = 1;//Noseusa

	 private bool _loadFromURLParam = false;

	private static GameStateState _prevGameState;
	private static GameStateState _gameState;

	private const string BOARDS_COLLECTION_FILE = "boardsCollection.json";

	#endregion

	#region Properties

	public BoardController BoardController => _boardController;

	public TurnController TurnController { get => _turnController; }

	private Player CurrentPlayer => _turnController.CurrentPlayer;
	private Tile CurrentTile => CurrentPlayer.CurrentTile;

	public int Round => _round;

	public static GameStateState GameState
	{
		get => _gameState;

		set
		{
			if (_prevGameState != _gameState)
			{
				_prevGameState = _gameState;
			}

			_gameState = value;
		}
	}
	//For Back Implementation
	public static GameStateState PreveGameState => _prevGameState;

	public event Action OnCuack;
	public event Action OnHappy;

	public event Action OnSad;
	public event Action OnRollDices;
	public event Action OnGameStarts;

	#endregion

	#region Unity Callbacks

	private void Awake()
	{
		if (Application.absoluteURL.Contains("board"))
			_loadFromURLParam = true;

		_saveButton.gameObject.SetActive(false);
	}

	private async void Start()
	{
		BoardData boardData = null;
		GameState = GameStateState.Welcome;
		_volumeControl.Initialize();
		_musicController.PlayBase();

		//LOAD / CREATE BOARD
		if (_loadDefault)
		{
			string boardJson = await LoadTextFileAsync(_defaultBoard);
			boardData = new BoardData(boardJson);
			//BOARD INFO
			await _popupsController.ShowBoardInfoPopup(boardData);
		}
		//BOARD FROM URL PARAM
		else if(_loadFromURLParam)
		{
			string boardParam = Application.absoluteURL.Split("board=")[1];

			if (boardParam.Contains("&"))
				boardParam = boardParam.Split('&')[0];

			BoardData board = await LoadBoardData(boardParam);

			//BOARD INFO
			await _popupsController.ShowBoardInfoPopup(boardData);
		}
		else
		{
			//WELCOME
			await _popupsController.ShowWelcome();
			OnCuack.Invoke();

			bool creating = await _popupsController.ShowCreateOrChooseBoard();
			OnCuack.Invoke();

			if (creating)
			{
				//First promt User Question
				string promptBase = await _popupsController.ShowCreateBoardQuestionPopup();
				OnCuack.Invoke();

				_popupsController.PatoCienciaPopup.Show("Preparando la propuesta...");

				//Create First Game Data
				_aiJsonGenerator = new AIJsonGenerator();
				//TODO: sHOW LOADING
				GameData gameData = await _aiJsonGenerator.CreateBaseGameData(promptBase);

				_popupsController.PatoCienciaPopup.Hide();

				//Creating seccond Game Data. Edit Board Mode
				GameData boardGameData = await _popupsController.ShowEditBoardPopup(gameData);

				_popupsController.PatoCienciaPopup.Show("Creando el tablero...");

				//Creating Board
				boardData = await _aiJsonGenerator.GetJsonBoard(boardGameData);

				_popupsController.PatoCienciaPopup.Hide();

				GameState = GameStateState.Playing;

			}
			//Select Board
			else
			{
				List<BoardData> boards = await LoadBoardsData();
				boardData = await _popupsController.ShowChooseBoardPopup(boards);
			}

			//ERROR CONTROL
			if (boardData == null)
			{
				OnSad.Invoke();
				await _popupsController.ShowGenericMessage("Error generando el tablero!\n Inténtalo de nuevo!", 7);
				OnCuack.Invoke();
				Start();
				return;
			}
		}

		//EDIT BOARD
		BoardData editedBoardData = await CheckEditMode(boardData);
		if (editedBoardData != null)
			boardData = editedBoardData;

		//CREATE BOARD!!!
		CreateBoard(boardData);

		OnCuack.Invoke();

		_boardController.OnMoveStep += Fart;		

		//PLAYER LIST
		List<Player> players = await _popupsController.PlayerCreationController.ShowAsync();
		if(players != null && players.Count > 0)
			GameState = GameStateState.Playing;

		OnCuack.Invoke();

		//TURN CONTROLLER
		_turnController = new TurnController(players, _popupsController);

		//GAME LOOP!!
		while (true)
		{
			StartGame(players);

			await GameLoop();

			//CREATING
			while (_gameState == GameStateState.Creating)
				await Task.Yield();

			//EDITING
			BoardData boardDataEdited = await CheckEditMode(_boardController.BoardData);

			if (boardDataEdited != null)
			{
				_boardController.ResetBoard();
				CreateBoard(boardDataEdited);
				continue;
			}

			//END GAME
			if (_gameState == GameStateState.EndGame)
			{
				await FinishGame();
				break;//Restart
			}

			//Back to Main Menu
			if (_gameState == GameStateState.Welcome)
			{
				_turnController.DestroyPlayerTokens();
				break;//Restart
			}
		}

		//Start Game flow again
		Start();
	}

	#endregion

	#region Private Methods

	private void StartGame(List<Player> players)
	{
		_loadDefault = false;

		MovePlayersToInitialTile(players);
		_musicController.PlayRock();

		OnGameStarts.Invoke();
	}

	private async Task GameLoop()
	{
		while (_gameState == GameStateState.Playing)
		{
			//LOST TURN
			if (CurrentPlayer.State == PlayerState.LostTurn)
			{
				CurrentPlayer.State = PlayerState.Waiting;
				_turnController.NextTurn();
				continue;
			}

			//CHALLENGE
			if (CurrentPlayer.State == PlayerState.OnChallenge)
			{
				bool completed = await _popupsController.ShowChallengePlayer(CurrentPlayer, false);

				//TODO: Control de flujo de salida!
				if (_gameState != GameStateState.Playing)
					break;

				OnCuack.Invoke();

				if (completed)
				{
					OnHappy.Invoke();
				}
				else
				{
					OnSad.Invoke();
					_turnController.NextTurn();
					continue;
				}
			}

			if (_gameState != GameStateState.Playing) break;

			//SHOW PLAYER TURN
			await _popupsController.ShowPlayerTurn(CurrentPlayer);

			if (_gameState != GameStateState.Playing) break;

			//Destroyed Player Control (SOLO AQUI?)
			if (CurrentPlayer.Token == null)
				continue;

			OnCuack.Invoke();

			//ON QUESTION
			if (CurrentPlayer.State == PlayerState.OnQuestion)
			{
				bool play = await _popupsController.ShowQuestion(CurrentTile.TileData.question);

				//TODO: Control de flujo de salida!
				if (_gameState != GameStateState.Playing)
					break;

				if (play)
				{
					OnHappy.Invoke();
				}
				else
				{
					OnSad.Invoke();
					_turnController.NextTurn();
					continue;
				}
			}

			if (_gameState != GameStateState.Playing) break;

			CurrentPlayer.State = PlayerState.Playing;

			//DICE ROLLING
			OnRollDices.Invoke();
			int diceValue = await _diceController.RollDice();
			await _popupsController.ShowPlayerDiceValue(diceValue);

			if (_gameState != GameStateState.Playing) break;

			//MOVE TOKEN
			Tile targetTile = await _boardController.MoveToken(CurrentPlayer, diceValue);

			if (_gameState != GameStateState.Playing) break;

			//APLLY TILE EFFECT
			bool playAgain = await ApplyTileEffect(targetTile);

			if (_gameState != GameStateState.Playing) break;

			if (playAgain)
				continue;

			_turnController.NextTurn();
		}
	}

	private async Task<bool> ApplyTileEffect(Tile targetTile)
	{
		switch (targetTile.TileType)
		{
			case TileType.Challenge:

				CurrentPlayer.State = PlayerState.OnChallenge;
				await _popupsController.ShowChallengePlayer(CurrentPlayer, true);
				_boardController.RefreshChallenge(CurrentPlayer.CurrentTile);
				OnCuack.Invoke();

				return false;

			case TileType.Question:

				bool playAgain = await _popupsController.ShowQuestion(targetTile.TileData.question);

				if (playAgain)
				{
					OnHappy.Invoke();
					CurrentPlayer.State = PlayerState.PlayAgain;
					_boardController.RefreshQuestion(CurrentPlayer.CurrentTile);
				}
				else
				{
					OnSad.Invoke();
					CurrentPlayer.State = PlayerState.OnQuestion;
				}

				return playAgain;

			case TileType.TravelToTile:

				await _popupsController.ShowGenericMessage("De pato a pato y tiro porque...\n CUACK!!", 2, CurrentPlayer.Token.Color);

				OnHappy.Invoke();

				await _boardController.TravelToNextTravelTile(CurrentPlayer);
				CurrentPlayer.State = PlayerState.PlayAgain;

				return true;

			case TileType.Bridge:

				await _popupsController.ShowGenericMessage("De puente a puente y tiro porque me lleva la corriente.", 2, CurrentPlayer.Token.Color);
				await _boardController.TravelToBridge(CurrentPlayer);
				OnCuack.Invoke();

				return true;

			case TileType.LoseTurnsUntil:
				OnSad.Invoke();
				await _popupsController.ShowGenericMessage("Tu patito se ha perdido!!\n Pierdes un turno.", 5, Color.gray);
				CurrentPlayer.State = PlayerState.LostTurn;

				break;

			case TileType.RollDicesAgain:
				CurrentPlayer.State = PlayerState.PlayAgain;
				OnHappy.Invoke();

				return true;

			case TileType.Die:
				OnSad.Invoke();
				await _popupsController.ShowGenericMessage("Casilla de la muerte.\n Vuelves al principio :(", 5, Color.black);
				OnHappy.Invoke();

				await _boardController.JumptToTile(CurrentPlayer, 0);
				CurrentPlayer.State = PlayerState.Waiting;

				return false;

			case TileType.End:
				GameState = GameStateState.EndGame;

				return false;
		}

		return false;
	}

	private async Task<BoardData> CheckEditMode(BoardData boardData)
	{
		if (_gameState == GameStateState.Editing)
		{
			_saveButton.gameObject.SetActive(true);
			_saveButton.onClick.AddListener(SaveBoard);

			_popupsController.HideAll();

			GameData gameData = await _popupsController.ShowEditBoardPopup(boardData);

			_popupsController.PatoCienciaPopup.Show("Creando el tablero...");

			//TODO: creo que esto no se debería hacer siempre... a veces solo quieres editar preguntas, no regenerar el board...
			boardData = await _aiJsonGenerator.GetJsonBoard(gameData);

			_popupsController.PatoCienciaPopup.Hide();
			return boardData;

		}
		
		_saveButton.gameObject.SetActive(false);
		return null;
	}

	private async Task FinishGame()
	{
		_musicController.PlayDrumBass();
		_winEffects.gameObject.SetActive(true);
		//duck Win Effects
		foreach (Player player in _turnController.Players)
		{
			if (player.CurrentTile.TileType == TileType.End)
				player.Token.Win();
			else
				player.Token.Loose();
		}

		bool userInteraction = false;
		while (!userInteraction)
		{
			OnHappy.Invoke();
			userInteraction = await _popupsController.ShowGenericMessage("Ha Ganado " + CurrentPlayer.Name + "!!!", 2);
			float time = 1;
			while (time > 0)
			{
				time -= Time.deltaTime;
				await Task.Yield();
			}
		}

		_winEffects.gameObject.SetActive(false);
		_musicController.PlayBase();

	}

	private void CreateBoard(BoardData boardData)
	{
		_boardController = new BoardController(boardData, _boardCreator);
	}

	//TODO: Move to Board o turn?!!
	private void MovePlayersToInitialTile(List<Player> players)
	{
		foreach (Player player in players)
		{
			player.State = PlayerState.Waiting;
			player.Token.ResetState();
			_boardController.JumptToTile(player, 0).WrapErrors();
		}
	}

	//MOVER!

	private void SaveBoard()
	{
		//TODO: Show Edit Tittle & Descriptin
		//TODO: Show Send Mail popup
		//TODO: _boardController.BoardData.autor = email;

		_emailSender.SendEmail(_boardController.BoardData);
		_popupsController.ShowBoardInfoPopup(_boardController.BoardData).WrapErrors();

	}

	//MOVER!

	/// <summary>
	/// Loads all BoardData from the JSON files in StreamingAssets.
	/// </summary>
	public async Task<List<BoardData>> LoadBoardsData()
	{
		List<BoardData> boards = new List<BoardData>();

		// Load the board names collection
		string boardsCollectionJson = await LoadTextFileAsync(BOARDS_COLLECTION_FILE);
		if (string.IsNullOrEmpty(boardsCollectionJson))
		{
			Debug.LogError("Boards collection file is empty or could not be loaded.");
			return boards;
		}

		// Deserialize the collection of board names
		BoardsCollection boardsCollection = JsonUtility.FromJson<BoardsCollection>(boardsCollectionJson);

		// Load each board's data
		foreach (string boardName in boardsCollection.BoardNames)
		{
			string boardJson = await LoadTextFileAsync($"{boardName}.json");
			if (string.IsNullOrEmpty(boardJson))
			{
				Debug.LogError($"Failed to load board data for {boardName}");
				continue;
			}

			BoardData boardData = JsonUtility.FromJson<BoardData>(boardJson);
			boards.Add(boardData);
		}

		return boards;
	}

	public async Task<BoardData> LoadBoardData(string boardName)
	{
		// Check if the board name is provided
		if (string.IsNullOrEmpty(boardName))
		{
			Debug.LogError("Board name is empty or null.");
			return null;
		}

		// Load the specific board data
		string boardJson = await LoadTextFileAsync($"{boardName}.json");
		if (string.IsNullOrEmpty(boardJson))
		{
			Debug.LogError($"Failed to load board data for {boardName}");
			return null;
		}

		// Deserialize the board data
		BoardData boardData = JsonUtility.FromJson<BoardData>(boardJson);

		return boardData;
	}


	[System.Serializable]
	private class BoardsCollection
	{
		public List<string> BoardNames;
	}
	//MOVER!

	private void Fart()
	{
		CurrentPlayer.Token.Fart();
	}

	//MOVER!!!

	/// <summary>
	/// Loads a text file asynchronously from StreamingAssets and returns its content.
	/// </summary>
	private async Task<string> LoadTextFileAsync(string fileName)
	{
		string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

		// For WebGL, we need to access the file via UnityWebRequest
		if (filePath.Contains("://") || filePath.Contains(":///"))
		{
			using (UnityWebRequest www = UnityWebRequest.Get(filePath))
			{
				var request = www.SendWebRequest();

				// Wait until the request is done
				while (!request.isDone)
				{
					await Task.Yield();
				}

				if (www.result != UnityWebRequest.Result.Success)
				{
					Debug.LogError("Error loading file: " + www.error);
					return null;
				}
				else
				{
					return www.downloadHandler.text;
				}
			}
		}
		else
		{
			// For non-WebGL platforms, we can read directly from the file system
			if (File.Exists(filePath))
			{
				return await Task.Run(() => File.ReadAllText(filePath));
			}
			else
			{
				Debug.LogError("File not found: " + filePath);
				return null;
			}
		}
	}

	#endregion

	#region Public Methods
	public async Task RerollGame()
	{
		GameState = GameStateState.Creating;
		_popupsController.HideAll();
		_popupsController.PatoCienciaPopup.Show("Regenerando el tablero...");
		GameData gameData = EditBoardPopup.ConvertBoardDataToGameData(_boardController.BoardData, _boardController.BoardData.challengeTypes);

		_boardController.ResetBoard();
		BoardData boardData = await _aiJsonGenerator.GetJsonBoard(gameData);
		CreateBoard(boardData);
		_popupsController.PatoCienciaPopup.Hide();
		GameState = GameStateState.Playing;
	}

	public void BackToMainMenu()
	{
		if (_gameState == GameStateState.Editing)
			Debug.Log("Salir del modo edición??");

		_popupsController.HideAll();
		_boardController.ResetBoard();
		GameState = GameStateState.Welcome;
	}

	public void RestartGame()
	{
		_popupsController.HideAll();
		MovePlayersToInitialTile(_turnController.Players);
	}

	public async Task EditPlayers()
	{
		//PLAYER LIST
		List<Player> players = await _popupsController.PlayerCreationController.ShowAsync(_turnController.Players);
		_turnController.Players = players;

		OnCuack.Invoke();

		//TURN CONTROLLER

	}

	#endregion

	#region EDITOR

	[Button]
	public async void MoveCurrentPLayer(int tileId)
	{
		Tile targetTile = await _boardController.JumptToTile(CurrentPlayer, tileId);
		await ApplyTileEffect(targetTile);
		_turnController.NextTurn();
	}

	internal void EnterEditMode()
	{
		GameState = GameStateState.Editing;
		_popupsController.HideAll();
	}

	#endregion
}

