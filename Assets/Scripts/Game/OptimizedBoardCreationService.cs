using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class OptimizedBoardCreationService
{
    private readonly OptimizedAIGenerator _aiGenerator;
    private readonly AIJsonGenerator _fallbackGenerator;
    private bool _useOptimized = true;

    public OptimizedBoardCreationService()
    {
        _aiGenerator = new OptimizedAIGenerator();
        _fallbackGenerator = new AIJsonGenerator();
    }

    public async Task<GameData> CreateBaseGameData(string promptBase)
    {
        if (_useOptimized)
        {
            var request = CreateOptimizedRequest(promptBase);
            var result = await _aiGenerator.GenerateOptimizedContent(request);
            
            if (result.success)
            {
                result.gameData.ShuffleQuestionOptions();
                LogOptimizationResults(result);
                return result.gameData;
            }
            
            Debug.LogWarning("Optimized generation failed, falling back to legacy system");
            _useOptimized = false;
        }

        return await _fallbackGenerator.CreateBaseGameData(promptBase);
    }

    public async Task<BoardData> CreateBoardFromGamedata(GameData initialGameData)
    {
        if (_useOptimized)
        {
            var enhancedRequest = CreateEnhancementRequest(initialGameData);
            var result = await _aiGenerator.GenerateOptimizedContent(enhancedRequest);
            
            if (result.success)
            {
                result.gameData.ShuffleQuestionOptions();
                LogOptimizationResults(result);
                return new BoardData(result.gameData);
            }
            
            Debug.LogWarning("Optimized enhancement failed, falling back to legacy system");
        }

        string responseDataEvaluation = await _fallbackGenerator.GetGameDataEvaluation(initialGameData);
        GameData gameData = null;
        
        try
        {
            gameData = JsonUtility.FromJson<GameData>(responseDataEvaluation);
        }
        catch
        {
            return null;
        }

        return new BoardData(gameData);
    }

    public BoardController CreateBoard(BoardData boardData, GameOfDuckBoardCreator boardCreator)
    {
        return new BoardController(boardData, boardCreator);
    }

    private ContentGenerationRequest CreateOptimizedRequest(string promptBase)
    {
        return new ContentGenerationRequest
        {
            topic = ExtractTopicFromPrompt(promptBase),
            proposal = promptBase,
            questionsCount = 3,
            challengesCount = 3,
            challengeTypes = GetDefaultChallengeTypes(),
            samples = new ContentSample()
        };
    }

    private ContentGenerationRequest CreateEnhancementRequest(GameData initialGameData)
    {
        return new ContentGenerationRequest
        {
            topic = initialGameData.tittle,
            proposal = initialGameData.proposal,
            questionsCount = initialGameData.questionsCount > 0 ? initialGameData.questionsCount + 2 : 0,
            challengesCount = initialGameData.challengesCount > 0 ? initialGameData.challengesCount + 2 : 0,
            challengeTypes = initialGameData.challengesTypes,
            samples = new ContentSample
            {
                sampleQuestions = initialGameData.questions,
                sampleChallenges = initialGameData.challenges
            }
        };
    }

    private string ExtractTopicFromPrompt(string prompt)
    {
        if (string.IsNullOrEmpty(prompt))
        {
            return "Tema General";
        }

        var words = prompt.Split(' ');
        if (words.Length > 3)
        {
            return string.Join(" ", words[0], words[1], words[2]);
        }

        return prompt.Length > 30 ? prompt.Substring(0, 30) + "..." : prompt;
    }

    private List<string> GetDefaultChallengeTypes()
    {
        return new List<string> 
        { 
            "Reflexión personal", 
            "Acción física", 
            "Creativo", 
            "Social", 
            "Mindfulness" 
        };
    }

    private void LogOptimizationResults(GenerationResult result)
    {
        if (result.issues.Count > 0)
        {
            Debug.Log($"Generated content with {result.issues.Count} issues detected and handled");
            
            foreach (var issue in result.issues)
            {
                Debug.Log($"Issue: {issue.type} - {issue.description} (Severity: {issue.severity})");
            }
        }
        else
        {
            Debug.Log("Content generated successfully with no issues detected");
        }

        if (!string.IsNullOrEmpty(result.validationFeedback))
        {
            Debug.Log($"Validation feedback: {result.validationFeedback}");
        }
    }
}
