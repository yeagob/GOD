using GOD.Utils;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static UnityEditor.Experimental.GraphView.GraphView;

/// <summary>
/// Manages the game cycle and holds references to the Board and BoardController.
/// </summary>
public class GameController : MonoBehaviour
{
	#region Fields

	[Header("TEMP Elements")]
	[SerializeField] private bool _loadDefault = false;
	[SerializeField] private Button _saveButton;	
	[SerializeField] private GameObject _winEffects;
	[SerializeField] private MusicController _musicController;

	[Header("Controllers")]
	[SerializeField] private DiceController _diceController;
	[SerializeField] private BoardController _boardController;
	[SerializeField] private GameOfDuckBoardCreator _boardCreator;
	[SerializeField] private PopupsController _popupsController;
	[SerializeField] private EmailSender _emailSender;
	[SerializeField] private VolumeControl _volumeControl;

	[SerializeField, ReadOnly] private TurnController _turnController;

	private AIJsonGenerator _aiJsonGenerator;
	private int _round = 1;

	private static GameStateState _gameState;

	#endregion

	#region Properties

	public BoardController BoardController => _boardController;

	public TurnController TurnController { get => _turnController; }

	private Player CurrentPlayer => _turnController.CurrentPlayer;
	private Tile CurrentTile => CurrentPlayer.CurrentTile;

	public int Round => _round;

	public static GameStateState GameState { get => _gameState; set => _gameState = value; }

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
			_loadDefault = true;
	}

	private async void Start()
	{
		BoardData boardData;
		_gameState = GameStateState.Welcome;
		_volumeControl.Initialize();

		_musicController.PlayBase();
		OnCuack.Invoke();

		//LOAD / CREATE BOARD
		if (_loadDefault)
		{
			string boardJson = await LoadTextFileAsync("adolescencia.json");
			boardData = new BoardData(boardJson);
			CreateBoard(boardData);
			_popupsController.HideWelcome();
		}
		else
		{
			//WELCOME
			string prompt = await _popupsController.ShowWelcome();
			OnCuack.Invoke();
			_aiJsonGenerator = new AIJsonGenerator(prompt);

			//TODO: Send by mail BoardData? prompt?

			//Generate Board
			boardData = await _aiJsonGenerator.GetJsonBoard();

			//ERROR CONTROL
			if (boardData == null)
			{
				OnSad.Invoke();
				await _popupsController.ShowGenericMessage("error generando el tablero!\n Inténtalo de nuevo!", 7);
				OnCuack.Invoke();
				Start();
				return;
			}

			CreateBoard(boardData);
			_popupsController.HideWelcome();
		}

		_boardController.OnMoveStep += Fart;
		OnCuack.Invoke();

		//BOARD INFO
		await _popupsController.ShowBoardInfoPopup(_boardController.BoardData);

		OnCuack.Invoke();

		//EDIT BOARD
		await CheckEditMode();

		//PLAYER LIST
		List<Player> players = await _popupsController.PlayerCreationController.GetPlayers();

		OnCuack.Invoke();

		//TURN CONTROLLER
		_turnController = new TurnController(players, _popupsController);

		while (true)
		{
			StartGame(players);

			await GameLoop();

			await CheckEditMode();

			if (_gameState == GameStateState.EndGame)
			{
				await FinishGame();
				break;
			}

			if(_gameState == GameStateState.Welcome)
			{
				_turnController.DestroyPlayerTokens();
				break;
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
		_gameState = GameStateState.Playing;
		_musicController.PlayRock();

		OnGameStarts.Invoke();
	}

	private async Task GameLoop()
	{
		while (_gameState == GameStateState.Playing)
		{
			//Lost Turn Control
			if (CurrentPlayer.State == PlayerState.LostTurn)
			{
				CurrentPlayer.State = PlayerState.Waiting;
				_turnController.NextTurn();
				continue;
			}

			//Player on Challenge
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

			//Show player Turn
			await _popupsController.ShowPlayerTurn(CurrentPlayer);

			//Destroyed Player Control
			if (CurrentPlayer.Token == null)
				continue;

			//TODO: Control de flujo de salida!
			if (_gameState != GameStateState.Playing)
				break;

			OnCuack.Invoke();

			//Player on Question Tile
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

			CurrentPlayer.State = PlayerState.Playing;

			//Roll Dice
			OnRollDices.Invoke();
			int diceValue = await _diceController.RollDice();
			await _popupsController.ShowPlayerDiceValue(diceValue);

			//Move Token
			Tile targetTile = await _boardController.MoveToken(CurrentPlayer, diceValue);

			bool playAgain = await ApplyTileEffect(targetTile);

			//TODO: Control de flujo de salida!
			if (_gameState != GameStateState.Playing)
				break;

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
				_gameState = GameStateState.EndGame;
				return false;
		}

		return false;
	}

	private async Task CheckEditMode()
	{
		if (_gameState == GameStateState.Editing)
		{
			_saveButton.gameObject.SetActive(true);
			_saveButton.onClick.AddListener(SaveBoard);

			_popupsController.HideAll();

			while (_gameState == GameStateState.Editing)
				await Task.Yield();
		}
		else
			_saveButton.gameObject.SetActive(false);
	}

	private void SaveBoard()
	{
		//TODO: Show Edit Tittle & Descriptin
		//TODO: Show Send Mail popup
		//TODO: _boardController.BoardData.autor = email;

		_emailSender.SendEmail(_boardController.BoardData);
		_popupsController.ShowBoardInfoPopup(_boardController.BoardData).WrapErrors();

	}

	private async Task FinishGame()
	{
		_musicController.PlayDrumBass();
		_winEffects.gameObject.SetActive(true);
		bool usrInteraction = false;
		while (!usrInteraction)
		{
			OnHappy.Invoke();
			usrInteraction = await _popupsController.ShowGenericMessage("Ha Ganado " + CurrentPlayer.Name + "!!!", 2);
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

	public void BackToMainMenu()
	{
		if (_gameState == GameStateState.Editing)
			Debug.Log("Salir del modo edición??");

		_boardController.ResetBoard();
		_gameState = GameStateState.Welcome;
		_popupsController.HideAll();
	}


	public void RestartGame()
	{
		_popupsController.HideAll();
		MovePlayersToInitialTile(_turnController.Players);
	}

	public async Task EditPlayers()
	{
		//PLAYER LIST
		List<Player> players = await _popupsController.PlayerCreationController.GetPlayers(_turnController.Players);
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
	#endregion
}

