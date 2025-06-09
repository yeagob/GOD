using System.Threading.Tasks;
using UnityEngine;

public class BoardCreationService
{
    private AIJsonGenerator _aiJsonGenerator;
    private bool _first = true;
    private readonly string _apiKey;

    public BoardCreationService(string apiKey)
    {
        _apiKey = apiKey;
        
        if (string.IsNullOrEmpty(_apiKey))
        {
            Debug.LogError("BoardCreationService: API Key is required for AI board generation.");
            return;
        }

        _aiJsonGenerator = new AIJsonGenerator(_apiKey);
    }

    public async Task<GameData> CreateBaseGameData(string promptBase)
    {
        if (_aiJsonGenerator == null)
        {
            Debug.LogError("BoardCreationService: AI service not initialized. Please provide a valid API key.");
            return null;
        }

        return await _aiJsonGenerator.CreateBaseGameData(promptBase);
    }

    public async Task<BoardData> CreateBoardFromGamedata(GameData initialGameData)
    {
        if (_aiJsonGenerator == null)
        {
            Debug.LogError("BoardCreationService: AI service not initialized. Please provide a valid API key.");
            return null;
        }

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

    public bool IsValidService => _aiJsonGenerator != null && !string.IsNullOrEmpty(_apiKey);
}