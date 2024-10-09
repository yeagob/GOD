using GOD.Utils;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
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

	[DllImport("__Internal")]
	private static extern void GeneratePDFFromUnity(string base64Image, string name);

	[SerializeField] private string _defaultBoard = "parent.json";

	[Header("TEMP Elements")]
	[SerializeField] private bool _loadDefault = false;
	[SerializeField] private Button _saveButton;
	[SerializeField] private Button _downloadButton;
	[SerializeField] private Camera _downloadCamera;
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
		Debug.Log("url:" + Application.absoluteURL);
		if (Application.absoluteURL.Contains("board"))
			_loadFromURLParam = true;

		_saveButton.gameObject.SetActive(false);
		_saveButton.onClick.AddListener(SaveBoard);
		_downloadButton.onClick.AddListener(DownloadBoard);
	}

	private void Update()
	{
		_downloadButton.gameObject.SetActive(GameState == GameStateState.Playing);
	}

	private async void Start()
	{
		BoardData boardData = null;
		GameState = GameStateState.Welcome;
		_volumeControl.Initialize();
		_musicController.PlayBase();

		//LOAD BOARD
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

			boardData = await LoadBoardData(boardParam);

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
				_saveButton.gameObject.SetActive(true);


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
		{
			boardData = editedBoardData;
			if (GameState == GameStateState.Creating)
			{
				GameData gameData = EditBoardPopup.ConvertBoardDataToGameData(editedBoardData, editedBoardData.challengeTypes);
				_popupsController.PatoCienciaPopup.Show("Creando el tablero...");
				boardData = await _aiJsonGenerator.GetJsonBoard(gameData);
				_popupsController.PatoCienciaPopup.Hide();
			}
		}
		else
			GameState = GameStateState.Welcome;

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

			//EDITING
			BoardData boardDataEdited = await CheckEditMode(_boardController.BoardData);

			if (boardDataEdited != null)
			{
				if (GameState == GameStateState.Creating)
				{
					GameData gameData = EditBoardPopup.ConvertBoardDataToGameData(boardDataEdited, boardDataEdited.challengeTypes);
					_popupsController.PatoCienciaPopup.Show("Creando el tablero...");
					boardData = await _aiJsonGenerator.GetJsonBoard(gameData);
					_popupsController.PatoCienciaPopup.Hide();
					_boardController.ResetBoard();
					CreateBoard(boardData);
				}
				else
				{
					_boardController.UpdateBoard(boardDataEdited);
					GameState = GameStateState.Playing;
				}
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
		
		if (GameState != GameStateState.Playing)
		{ 
		MovePlayersToInitialTile(players);
		_musicController.PlayRock();
		GameState = GameStateState.Playing;
		OnGameStarts.Invoke();
		}
	}

	private async Task GameLoop()
	{
		while (_gameState == GameStateState.Playing)
		{
			if (CurrentPlayer == null)
				continue;

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
					_boardController.RefreshChallenge(_turnController.Players, CurrentTile);
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
			//await _popupsController.ShowPlayerDiceValue(diceValue);
			await _popupsController.ShowGenericMessage(diceValue.ToString(), 0.7f);

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
				OnCuack.Invoke();

				return false;

			case TileType.Question:

				bool playAgain = await _popupsController.ShowQuestion(targetTile.TileData.question);

				if (playAgain)
				{
					OnHappy.Invoke();
					CurrentPlayer.State = PlayerState.PlayAgain;
					_boardController.RefreshQuestion(CurrentTile);
				}
				else
				{
					OnSad.Invoke();
					CurrentPlayer.State = PlayerState.OnQuestion;
				}

				return playAgain;

			case TileType.TravelToTile:

				 _popupsController.ShowGenericMessage("De pato a pato y tiro porque...\n CUACK!!", 2, CurrentPlayer.Token.Color).WrapErrors();
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

			_popupsController.HideAll();

			boardData = await _popupsController.ShowEditBoardPopup(boardData);

			if (boardData != null)
				_saveButton.gameObject.SetActive(true);
			else
				GameState = GameStateState.Playing;

			return boardData;

		}
				
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
		_turnController.DestroyPlayerTokens();

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
		ShowPublish().WrapErrors();
	}

	private async Task ShowPublish()
	{
		string mail = await _popupsController.ShowPublishBoardPopup();

		if (mail != "")
		{
			_boardController.BoardData.autor = mail;
			_emailSender.SendEmail(_boardController.BoardData);
			await _popupsController.ShowGenericMessage("Solicitud de publicacion enviada!!");
		}
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

	private void DownloadBoard()
	{
		RenderTexture renderTexture = new RenderTexture(1920, 1080, 24);
		_downloadCamera.targetTexture = renderTexture;
		Texture2D screenShot = new Texture2D(1920, 1080, TextureFormat.RGB24, false);

		_downloadCamera.Render();
		RenderTexture.active = renderTexture;
		screenShot.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
		screenShot.Apply();

		_downloadCamera.targetTexture = null;
		RenderTexture.active = null;
		Destroy(renderTexture);

		byte[] bytes = screenShot.EncodeToPNG();
		string base64Image = System.Convert.ToBase64String(bytes);

		// Llamar a la función JS para generar el PDF
		GeneratePDFFromUnity(base64Image,_boardController.BoardData.tittle);
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

		GameState = GameStateState.Welcome;
		_popupsController.HideAll();
		_boardController.ResetBoard();
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

