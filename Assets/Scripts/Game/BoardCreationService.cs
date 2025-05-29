using System.Threading.Tasks;
using UnityEngine;

public class BoardCreationService
{
    private readonly OptimizedBoardCreationService _optimizedService;
    private bool _first = true;

    public BoardCreationService()
    {
        _optimizedService = new OptimizedBoardCreationService();
    }

    public async Task<GameData> CreateBaseGameData(string promptBase)
    {
        return await _optimizedService.CreateBaseGameData(promptBase);
    }

    public async Task<BoardData> CreateBoardFromGamedata(GameData initialGameData)
    {
        var boardData = await _optimizedService.CreateBoardFromGamedata(initialGameData);
        
        if (boardData == null && _first)
        {
            _first = false;
            return await CreateBoardFromGamedata(initialGameData);
        }
        
        return boardData;
    }

    public BoardController CreateBoard(BoardData boardData, GameOfDuckBoardCreator boardCreator)
    {
        return _optimizedService.CreateBoard(boardData, boardCreator);
    }
}
