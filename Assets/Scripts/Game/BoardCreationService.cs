using System.Threading.Tasks;
using UnityEngine;

public class BoardCreationService
{
    private readonly AIJsonGenerator _aiJsonGenerator;
    private bool _first = true;

    public BoardCreationService()
    {
        _aiJsonGenerator = new AIJsonGenerator();
    }

    public async Task<GameData> CreateBaseGameData(string promptBase)
    {
        return await _aiJsonGenerator.CreateBaseGameData(promptBase);
    }

    public async Task<BoardData> CreateBoardFromGamedata(GameData initialGameData)
    {
        string responseDataEvaluation = await _aiJsonGenerator.GetGameDataEvaluation(initialGameData);
        GameData gameData = null;
        
        try
        {
            gameData = JsonUtility.FromJson<GameData>(responseDataEvaluation);
        }
        catch
        {
            if (_first)
            {
                _first = false;
                return await CreateBoardFromGamedata(initialGameData);
            }
            else
            {
                return null;
            }
        }

        return new BoardData(gameData);
    }

    public BoardController CreateBoard(BoardData boardData, GameOfDuckBoardCreator boardCreator)
    {
        return new BoardController(boardData, boardCreator);
    }
}
