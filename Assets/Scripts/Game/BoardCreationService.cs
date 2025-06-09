using System.Threading.Tasks;
using UnityEngine;

public class BoardCreationService
{

    private AIJsonGenerator _aiJsonGenerator;

    private readonly OptimizedBoardCreationService _optimizedService;

    private bool _first = true;
    private readonly string _apiKey;
    private readonly bool _hasApiKey;

    public BoardCreationService(string apiKey)
    {

        _apiKey = apiKey;
        _hasApiKey = !string.IsNullOrEmpty(_apiKey);
        
        if (_hasApiKey)
        {
            _aiJsonGenerator = new AIJsonGenerator(_apiKey);
        }
        else
        {
            Debug.Log("BoardCreationService: No API Key provided. AI board generation will be disabled.");
        }

        _optimizedService = new OptimizedBoardCreationService();

    }

    public async Task<GameData> CreateBaseGameData(string promptBase)
    {

        if (!_hasApiKey || _aiJsonGenerator == null)
        {
            Debug.LogWarning("BoardCreationService: AI service not available. Please provide an API key to enable AI board generation.");
            return null;
        }

        return await _aiJsonGenerator.CreateBaseGameData(promptBase);
    }

    public async Task<BoardData> CreateBoardFromGamedata(GameData initialGameData)
    {

        if (!_hasApiKey || _aiJsonGenerator == null)
        {
            Debug.LogWarning("BoardCreationService: AI service not available. Please provide an API key to enable AI board generation.");
            return null;
        }

        string responseDataEvaluation = await _aiJsonGenerator.GetGameDataEvaluation(initialGameData);
        GameData gameData = null;

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

    public bool HasAICapability => _hasApiKey && _aiJsonGenerator != null;
}