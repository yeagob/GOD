using GOD.Utils;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Network.Models;
using Network.Infrastructure;

public class GameController : MonoBehaviour
{
    #region Fields

    [SerializeField] private string _defaultBoard = "parent.json";

    [Header("TEMP Elements")]
    [SerializeField] private bool _loadDefault = false;
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _downloadButton;
    [SerializeField] private Button _shareButton;
    [SerializeField] private Camera _downloadCamera;
    [SerializeField] private GameObject _winEffects;
    [SerializeField] private MusicController _musicController;

    [Header("Controllers")]
    [SerializeField] private DiceController _diceController;
    [SerializeField] private GameOfDuckBoardCreator _boardCreator;
    [SerializeField] private PopupsController _popupsController;
    [SerializeField] private EmailSender _emailSender;
    [SerializeField] private VolumeControl _volumeControl;

    [Header("Debug Multiplayer")]
    [SerializeField] private bool _debugMultiplayerMode = false;
    [SerializeField] private string _debugMatchId = "";
    [SerializeField] private string _debugBoardName = "parent.json";

    [SerializeField, ReadOnly] private TurnController _turnController;

    private GameStateManager _gameStateManager;
    private URLParameterHandler _urlParameterHandler;
    private BoardDataService _boardDataService;
    private ScreenshotService _screenshotService;
    private BoardEditModeHandler _boardEditModeHandler;
    private GameFlowController _gameFlowController;
    private BoardCreationService _boardCreationService;
    private ShareService _shareService;
    private IMatchModel _matchModel;

    private BoardController _boardController;

    public static bool JumpToCreateNew;
    public static int SelectedIndex = -1;

    #endregion

    #region Properties

    public BoardController BoardController => _boardController;
    public TurnController TurnController => _turnController;
    private Player CurrentPlayer => _turnController?.CurrentPlayer;

    public static GameStateState GameState
    {
        get => GameStateManager.GameState;
        set => GameStateManager.GameState = value;
    }

    public event Action OnCuack;
    public event Action OnHappy;
    public event Action OnSad;
    public event Action OnRollDices;
    public event Action OnGameStarts;

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        InitializeServices();
        SetupUI();
        _urlParameterHandler.LogCurrentURL();
    }

    private void Update()
    {
        _downloadButton.gameObject.SetActive(_gameStateManager.IsInState(GameStateState.Playing));
    }

    private async void Start()
    {
        await InitializeGame();
    }

    #endregion

    #region Initialization

    private void InitializeServices()
    {
        _gameStateManager = new GameStateManager();
        _urlParameterHandler = new URLParameterHandler();
        _boardDataService = new BoardDataService();
        _screenshotService = gameObject.AddComponent<ScreenshotService>();
        _screenshotService.Initialize(_downloadCamera);
        _boardEditModeHandler = new BoardEditModeHandler(_popupsController, _gameStateManager);
        _boardCreationService = new BoardCreationService();
        _shareService = new ShareService(_popupsController, _emailSender, _urlParameterHandler);

        InitializeMultiplayerServices();
    }

    private void InitializeMultiplayerServices()
    {
        try
        {
            _matchModel = NetworkInstaller.Resolve<IMatchModel>();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Network services not available: {ex.Message}");
        }
    }

    private void SetupUI()
    {
        _saveButton.onClick.AddListener(SaveBoard);
        _shareButton.onClick.AddListener(ShareBoard);
        _downloadButton.onClick.AddListener(DownloadBoard);
        _downloadCamera.gameObject.SetActive(false);
    }

    private async Task InitializeGame()
    {
        SelectedIndex = -1;
        _saveButton.gameObject.SetActive(false);
        _shareButton.gameObject.SetActive(false);

        _gameStateManager.SetGameState(GameStateState.Welcome);
        _volumeControl.Initialize();
        _musicController.PlayBase();

        BoardData boardData = await LoadInitialBoard();

        if (boardData == null)
        {
            OnSad?.Invoke();
            await _popupsController.ShowGenericMessage("Error generando el tablero!\\n Inténtalo de nuevo!", 7);
            OnCuack?.Invoke();
            Start();
            return;
        }

        if (IsMultiplayerMode())
        {
            await HandleMultiplayerFlow(boardData);
            return;
        }

        BoardData editedBoardData = await _boardEditModeHandler.HandleEditMode(boardData);
        if (editedBoardData != null)
        {
            boardData = await ProcessEditedBoard(editedBoardData);
        }
        else
        {
            _gameStateManager.SetGameState(GameStateState.Welcome);
        }

        CreateBoard(boardData);
        OnCuack?.Invoke();

        _boardController.OnMoveStep += () => CurrentPlayer?.Token?.Fart();

        await StartGameLoop();
    }

    #endregion

    #region Multiplayer Flow

    private bool IsMultiplayerMode()
    {
        return _debugMultiplayerMode || _urlParameterHandler.IsMultiplayerMode;
    }

    private string GetMatchId()
    {
        if (_debugMultiplayerMode && !string.IsNullOrEmpty(_debugMatchId))
        {
            return _debugMatchId;
        }
        return _urlParameterHandler.GetMatchParameter();
    }

    private string GetBoardName()
    {
        if (_debugMultiplayerMode && !string.IsNullOrEmpty(_debugBoardName))
        {
            return _debugBoardName;
        }
        return _urlParameterHandler.GetBoardParameter();
    }

    private async Task HandleMultiplayerFlow(BoardData boardData)
    {
        CreateBoard(boardData);
        OnCuack?.Invoke();

        string matchId = GetMatchId();
        
        if (_matchModel != null && !string.IsNullOrEmpty(matchId))
        {
            MatchData existingMatch = await GetMatchDataAsync(matchId);
            if (existingMatch._id != null)
            {
                _matchModel.SetAsClient(existingMatch);
                Debug.Log($"Debug Multiplayer: Set as client for match {matchId}");
            }
        }

        _popupsController.ShowMultiplayerPanel(matchId);
    }

    private async Task<MatchData> GetMatchDataAsync(string matchId)
    {
        TaskCompletionSource<MatchData> tcs = new TaskCompletionSource<MatchData>();
        
        _matchModel.GetMatch(matchId, matchData => {
            tcs.SetResult(matchData);
        });

        return await tcs.Task;
    }

    #endregion

    #region Board Loading

    private async Task<BoardData> LoadInitialBoard()
    {
        if (_loadDefault)
        {
            BoardData boardData = await _boardDataService.LoadDefaultBoard(_defaultBoard);
            await _popupsController.ShowBoardInfoPopup(boardData);
            return boardData;
        }
        else if (_urlParameterHandler.ShouldLoadFromURL || _debugMultiplayerMode)
        {
            string boardParam = GetBoardName();
            _shareButton.gameObject.SetActive(true);
            BoardData boardData = await _boardDataService.LoadBoardData(boardParam);
            await _popupsController.ShowBoardInfoPopup(boardData);
            return boardData;
        }
        else
        {
            return await HandleBoardSelection();
        }
    }

    private async Task<BoardData> HandleBoardSelection()
    {
        bool creating = true;

        if (!JumpToCreateNew)
        {
            await _popupsController.ShowWelcome();
            OnCuack?.Invoke();
            creating = await _popupsController.ShowCreateOrChooseBoard();
            OnCuack?.Invoke();
        }

        if (creating)
        {
            return await CreateNewBoard();
        }
        else
        {
            return await SelectExistingBoard();
        }
    }

    private async Task<BoardData> CreateNewBoard()
    {
        JumpToCreateNew = false;

        string promptBase = await _popupsController.ShowCreateBoardQuestionPopup();
        OnCuack?.Invoke();

        if (promptBase == "")
        {
            Start();
            return null;
        }

        _popupsController.PatoCienciaPopup.Show("Preparando la propuesta...");

        GameData gameData = await _boardCreationService.CreateBaseGameData(promptBase);
        _popupsController.PatoCienciaPopup.Hide();

        if (gameData == null)
        {
            await _popupsController.ShowGenericMessage("Error Creando tablero. Intentalo de nuevo!", 5);
            JumpToCreateNew = true;
            Start();
            return null;
        }

        GameData initialGameData = await _popupsController.ShowEditBoardPopup(gameData);
        _popupsController.PatoCienciaPopup.Show("Creando el tablero...");

        BoardData boardData = await _boardCreationService.CreateBoardFromGamedata(initialGameData);
        _popupsController.PatoCienciaPopup.Hide();
        _saveButton.gameObject.SetActive(true);

        return boardData;
    }

    private async Task<BoardData> SelectExistingBoard()
    {
        List<BoardData> boards = await _boardDataService.LoadBoardsData();
        BoardData boardData = await _popupsController.ShowChooseBoardPopup(boards);
        OnCuack?.Invoke();

        if (boardData == null)
        {
            Start();
            return null;
        }

        _shareButton.gameObject.SetActive(true);
        return boardData;
    }

    #endregion

    #region Game Flow

    private async Task StartGameLoop()
    {
        List<Player> players = await _popupsController.PlayerCreationController.ShowAsync(false);
        OnCuack?.Invoke();

        _turnController = new TurnController(players, _popupsController);
        InitializeGameFlowController();

        while (true)
        {
            _gameFlowController.StartGame(players);
            await _gameFlowController.GameLoop();

            BoardData boardDataEdited = await _boardEditModeHandler.HandleEditMode(_boardController.BoardData);

            if (boardDataEdited != null)
            {
                await ProcessEditedBoardDuringGame(boardDataEdited);
                continue;
            }

            if (_gameStateManager.IsInState(GameStateState.EndGame))
            {
                await FinishGame();
            }

            if (_gameStateManager.IsInState(GameStateState.Welcome))
            {
                _turnController.DestroyPlayerTokens();
                break;
            }
        }

        Start();
    }

    private void InitializeGameFlowController()
    {
        _gameFlowController = new GameFlowController(
            _boardController,
            _diceController,
            _popupsController,
            _turnController,
            _gameStateManager,
            _emailSender,
            _musicController);

        _gameFlowController.OnCuack += () => OnCuack?.Invoke();
        _gameFlowController.OnHappy += () => OnHappy?.Invoke();
        _gameFlowController.OnSad += () => OnSad?.Invoke();
        _gameFlowController.OnRollDices += () => OnRollDices?.Invoke();
        _gameFlowController.OnGameStarts += () => OnGameStarts?.Invoke();
    }

    #endregion

    #region Board Management

    private async Task<BoardData> ProcessEditedBoard(BoardData editedBoardData)
    {
        if (_gameStateManager.IsInState(GameStateState.Creating))
        {
            GameData initialGameData = EditBoardPopup.ConvertBoardDataToGameData(editedBoardData, editedBoardData.challengeTypes);
            _popupsController.PatoCienciaPopup.Show("Creando el tablero...");
            BoardData boardData = await _boardCreationService.CreateBoardFromGamedata(initialGameData);
            _popupsController.PatoCienciaPopup.Hide();
            return boardData;
        }
        return editedBoardData;
    }

    private async Task ProcessEditedBoardDuringGame(BoardData boardDataEdited)
    {
        if (_gameStateManager.IsInState(GameStateState.Creating))
        {
            GameData initialGameData = EditBoardPopup.ConvertBoardDataToGameData(boardDataEdited, boardDataEdited.challengeTypes);
            _popupsController.PatoCienciaPopup.Show("Creando el tablero...");
            BoardData boardData = await _boardCreationService.CreateBoardFromGamedata(initialGameData);
            _popupsController.PatoCienciaPopup.Hide();
            _boardController.ResetBoard();
            CreateBoard(boardData);
        }
        else
        {
            _boardController.UpdateBoard(boardDataEdited);
            _gameStateManager.SetGameState(GameStateState.Playing);
        }
    }

    private void CreateBoard(BoardData boardData)
    {
        _boardController = _boardCreationService.CreateBoard(boardData, _boardCreator);
    }

    #endregion

    #region Game End

    private async Task FinishGame()
    {
        _musicController.PlayDrumBass();
        _winEffects.gameObject.SetActive(true);

        foreach (Player player in _turnController.Players)
        {
            if (player.CurrentTile.TileType == TileType.End)
            {
                player.Token.Win();
            }
            else
            {
                player.Token.Loose();
            }
        }

        bool userInteraction = false;
        while (!userInteraction)
        {
            OnHappy?.Invoke();
            userInteraction = await _popupsController.ShowGenericMessage("Ha Ganado " + CurrentPlayer.Name + "!!!", 2);
            float time = 1;
            while (time > 0)
            {
                time -= Time.deltaTime;
                await Task.Yield();
            }
        }

        _musicController.PlayBase();
        _winEffects.gameObject.SetActive(false);
        await _popupsController.ShowSettings();
    }

    #endregion

    #region UI Event Handlers

    private void SaveBoard()
    {
        _shareService.PublishBoard(_boardController.BoardData).WrapErrors();
    }

    private void DownloadBoard()
    {
        _screenshotService.DownloadBoard(_boardController);
    }

    private void ShareBoard()
    {
        _shareService.ShareBoard(_boardDataService.GetBoardNames(), SelectedIndex);
    }

    #endregion

    #region Public Methods

    public async Task RerollGame()
    {
        _gameStateManager.SetGameState(GameStateState.Creating);
        _popupsController.HideAll();

        GameData initialGameData = EditBoardPopup.ConvertBoardDataToGameData(_boardController.BoardData, _boardController.BoardData.challengeTypes);
        _popupsController.PatoCienciaPopup.Show("Regenerando el tablero...");

        BoardData boardData = await _boardCreationService.CreateBoardFromGamedata(initialGameData);
        _popupsController.PatoCienciaPopup.Hide();

        _boardController.ResetBoard();
        CreateBoard(boardData);
        _gameStateManager.SetGameState(GameStateState.Playing);
    }

    public void BackToMainMenu()
    {
        if (_gameStateManager.IsInState(GameStateState.Editing))
        {
            Debug.Log("Salir del modo edición??");
        }

        _gameFlowController?.BackToMainMenu();
    }

    public void RestartGame()
    {
        _gameFlowController?.RestartGame();
    }

    public async Task EditPlayers()
    {
        List<Player> players = await _popupsController.PlayerCreationController.ShowAsync(false, _turnController.Players);
        _turnController.Players = players;
        OnCuack?.Invoke();
    }

    #endregion

    #region Editor Methods

    [Button]
    public async void MoveCurrentPLayer(int tileId)
    {
        if (_gameFlowController != null)
        {
            await _gameFlowController.MoveCurrentPlayer(tileId);
        }
    }

    internal void EnterEditMode()
    {
        _boardEditModeHandler.EnterEditMode();
    }

    #endregion
}
