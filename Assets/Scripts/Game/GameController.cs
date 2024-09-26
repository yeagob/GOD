using Sc.Utils;
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
public class GameController: MonoBehaviour
{
	public bool LoadDefault = false;

	#region Fields

	[Header("TEMP Elements")]
	[SerializeField] private Button _saveButton;
	[SerializeField] private GameObject _winEffects;

	[Header("Controllers")]
	[SerializeField] private DiceController _diceController;
	[SerializeField] private BoardController _boardController;
	[SerializeField] private GameOfDuckBoardCreator _boardCreator;
	[SerializeField] private PopupsController _popupsController;
	[SerializeField] private EmailSender _emailSender;
	
	[SerializeField, ReadOnly] private TurnController _turnController;

	private AIJsonGenerator _aiJsonGenerator;
	private int _round = 1;

	private static GameStateState _gameState;

	#endregion

	#region Properties

	public BoardController BoardController { get; private set; }
	public TurnController TurnController { get => _turnController;  }

	private Player CurrentPlayer => _turnController.CurrentPlayer;
	private Tile CurrentTile => CurrentPlayer.CurrentTile;

	public int Round => _round;

	public static GameStateState GameState { get => _gameState; set => _gameState = value; }

	#endregion

	#region Unity Callbacks

	private async void Start()
	{
		BoardData boardData;
		_gameState = GameStateState.Welcome;

		//LOAD / CREATE BOARD
		if (Application.absoluteURL.Contains("board") || LoadDefault)
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
			_aiJsonGenerator = new AIJsonGenerator(prompt);

			//TODO: Send by mail BoardData? prompt?

			//Generate Board
			boardData = await _aiJsonGenerator.GetJsonBoard();

			//ERROR CONTROL
			if(boardData == null)
			{
				await _popupsController.ShowGenericMessage("error generando el tablero!\n Inténtalo de nuevo!", 7);
				Start();
				return;
			}

			CreateBoard(boardData);
			_popupsController.HideWelcome();
		}

		//BOARD INFO
		await _popupsController.ShowBoardInfoPopup(_boardController.BoardData);

		//EDIT BOARD
		await CheckEditMode();


		//PLAYER LIST
		List<Player> players = await _popupsController.PlayerCreationController.GetPlayers();

		//TURN CONTROLLER
		_turnController = new TurnController(players, _popupsController);

		while (true)
		{

			StartGame(players);
			
			await GameLoop();
			
			await FinishGame();

			//BOARD INFO
			await _popupsController.ShowBoardInfoPopup(_boardController.BoardData);

			await CheckEditMode();

		}
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
		_winEffects.gameObject.SetActive(true);

		while(!Input.anyKeyDown && !Input.GetMouseButtonDown(0))
			await _popupsController.ShowGenericMessage("Ha Ganado "+CurrentPlayer.Name+"!!!", 3);

		_winEffects.gameObject.SetActive(false);
	}

	private void StartGame(List<Player> players)
	{
		MovePlayerToInitialTile(players);
		_gameState = GameStateState.Playing;
	}

	private async Task GameLoop()
	{
		while(_gameState == GameStateState.Playing)
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
				if (!completed)
				{
					_turnController.NextTurn();
					continue;
				}
			}

			//Show player Turn
			await _popupsController.ShowPlayerTurn(CurrentPlayer);

			//Player on Question Tile
			if (CurrentPlayer.State == PlayerState.OnQuestion)
			{
				bool play = await _popupsController.ShowQuestion(CurrentTile.TileData.question);
				if (!play)
				{
					_turnController.NextTurn();
					continue;
				}
			}

			CurrentPlayer.State = PlayerState.Playing;

			//Roll Dice
			int diceValue = await _diceController.RollDice();
			await _popupsController.ShowPlayerDiceValue(diceValue);

			//Move Token
			Tile targetTile = await _boardController.MoveToken(CurrentPlayer, diceValue);

			bool playAgain = await ApplyTileEffect(targetTile);

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
				return false;

			case TileType.Question:
				bool playAgain = await _popupsController.ShowQuestion(targetTile.TileData.question);
				if (playAgain)
					CurrentPlayer.State = PlayerState.PlayAgain;
				else
					CurrentPlayer.State = PlayerState.OnQuestion;

				return playAgain;

			case TileType.TravelToTile:				
				await _popupsController.ShowGenericMessage("De pato a pato y tiro porque...\n CUACK!!", 2, CurrentPlayer.Token.Color);
				await _boardController.TravelToNextTravelTile(CurrentPlayer);
				CurrentPlayer.State = PlayerState.PlayAgain;
				return true;

			case TileType.LoseTurnsUntil:
				await _popupsController.ShowGenericMessage("Tu patito se ha perdido!!\n Pierdes un turno.", 5, Color.gray);
				CurrentPlayer.State = PlayerState.LostTurn;
				break;

			case TileType.RollDicesAgain:
				CurrentPlayer.State = PlayerState.PlayAgain;
				return true;

			case TileType.Die:
				await _popupsController.ShowGenericMessage("Casilla de la muerte.\n Vuelves al principio :(", 5, Color.black);
				await _boardController.JumptToTile(CurrentPlayer, 0);
				CurrentPlayer.State = PlayerState.Waiting;
				return false;

			case TileType.End:
				_gameState = GameStateState.EndGame;
				return false;
		}

		return false;
	}

	//TODO: Move to Board!!
	private void MovePlayerToInitialTile(List<Player> players)
	{
		foreach (Player player in players)
		{
			player.State = PlayerState.Waiting;
			player.Token.MoveToTile(_boardController.BoardTiles[0]);
		}
	}

	#endregion

	#region Private Methods

	private void CreateBoard(BoardData boardData)
	{
			_boardController = new BoardController(boardData, _boardCreator);
	}

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
}

