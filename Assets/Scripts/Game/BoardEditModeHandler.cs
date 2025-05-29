using System.Threading.Tasks;

public class BoardEditModeHandler
{
    private readonly PopupsController _popupsController;
    private readonly GameStateManager _gameStateManager;

    public BoardEditModeHandler(PopupsController popupsController, GameStateManager gameStateManager)
    {
        _popupsController = popupsController;
        _gameStateManager = gameStateManager;
    }

    public async Task<BoardData> HandleEditMode(BoardData boardData)
    {
        if (!_gameStateManager.IsInState(GameStateState.Editing))
        {
            return null;
        }

        _popupsController.HideAll();
        boardData = await _popupsController.ShowEditBoardPopup(boardData);

        if (boardData != null)
        {
            return boardData;
        }
        else
        {
            _gameStateManager.SetGameState(GameStateState.Playing);
            return null;
        }
    }

    public void EnterEditMode()
    {
        _gameStateManager.SetGameState(GameStateState.Editing);
        _popupsController.HideAll();
    }

    public bool IsInEditMode()
    {
        return _gameStateManager.IsInState(GameStateState.Editing);
    }
}
