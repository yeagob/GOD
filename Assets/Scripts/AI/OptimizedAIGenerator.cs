using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class ContentGenerationRequest
{
    public string topic;
    public string proposal;
    public int questionsCount;
    public int challengesCount;
    public List<string> challengeTypes;
    public ContentSample samples;
}

[Serializable]
public class ContentSample
{
    public List<QuestionData> sampleQuestions = new List<QuestionData>();
    public List<string> sampleChallenges = new List<string>();
}

[Serializable]
public class GenerationResult
{
    public bool success;
    public GameData gameData;
    public string validationFeedback;
    public List<ContentIssue> issues;
}

[Serializable]
public class ContentIssue
{
    public string type;
    public string description;
    public int itemIndex;
    public string severity;
}

public class OptimizedAIGenerator
{
    private readonly GPT4Mini _gpt;
    private readonly List<string> _defaultChallengeTypes;
    private readonly ContentValidator _validator;
    private readonly PromptTemplateManager _promptManager;

    private const string API_KEY = "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";

    public OptimizedAIGenerator()
    {
        _gpt = new GPT4Mini(API_KEY);
        _defaultChallengeTypes = new List<string> 
        { 
            "Reflexión personal", "Acción física", "Creativo", "Social", "Mindfulness" 
        };
        _validator = new ContentValidator();
        _promptManager = new PromptTemplateManager();
    }

    public async Task<GenerationResult> GenerateOptimizedContent(ContentGenerationRequest request)
    {
        try
        {
            var parallelTasks = new List<Task>();
            GameData gameData = CreateBaseGameData(request);

            if (request.questionsCount > 0 && request.challengesCount > 0)
            {
                var questionsTask = GenerateQuestionsAsync(request);
                var challengesTask = GenerateChallengesAsync(request);
                
                await Task.WhenAll(questionsTask, challengesTask);
                
                gameData.questions = await questionsTask;
                gameData.challenges = await challengesTask;
            }
            else if (request.questionsCount > 0)
            {
                gameData.questions = await GenerateQuestionsAsync(request);
            }
            else if (request.challengesCount > 0)
            {
                gameData.challenges = await GenerateChallengesAsync(request);
            }

            var validationResult = await ValidateAndOptimizeContent(gameData);
            
            return new GenerationResult
            {
                success = true,
                gameData = validationResult.gameData,
                validationFeedback = validationResult.feedback,
                issues = validationResult.issues
            };
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in optimized generation: {ex.Message}");
            return new GenerationResult { success = false };
        }
    }

    private GameData CreateBaseGameData(ContentGenerationRequest request)
    {
        return new GameData
        {
            tittle = GenerateTitle(request.topic),
            proposal = request.proposal,
            questionsCount = request.questionsCount,
            challengesCount = request.challengesCount,
            challengesTypes = request.challengeTypes ?? _defaultChallengeTypes,
            questions = new List<QuestionData>(),
            challenges = new List<string>()
        };
    }

    private async Task<List<QuestionData>> GenerateQuestionsAsync(ContentGenerationRequest request)
    {
        string prompt = _promptManager.CreateQuestionsPrompt(request);
        string response = await _gpt.GetCompletion(prompt);
        
        return ParseQuestionsFromResponse(response);
    }

    private async Task<List<string>> GenerateChallengesAsync(ContentGenerationRequest request)
    {
        string prompt = _promptManager.CreateChallengesPrompt(request);
        string response = await _gpt.GetCompletion(prompt);
        
        return ParseChallengesFromResponse(response);
    }

    private async Task<(GameData gameData, string feedback, List<ContentIssue> issues)> ValidateAndOptimizeContent(GameData gameData)
    {
        var issues = _validator.ValidateContent(gameData);
        
        if (issues.Count == 0)
        {
            return (gameData, "Contenido validado correctamente", issues);
        }

        var criticalIssues = issues.FindAll(i => i.severity == "Critical");
        
        if (criticalIssues.Count > 0)
        {
            var fixedGameData = await FixCriticalIssues(gameData, criticalIssues);
            return (fixedGameData, "Contenido corregido automáticamente", issues);
        }

        return (gameData, "Contenido con issues menores", issues);
    }

    private async Task<GameData> FixCriticalIssues(GameData gameData, List<ContentIssue> criticalIssues)
    {
        foreach (var issue in criticalIssues)
        {
            if (issue.type == "Question")
            {
                var fixedQuestion = await FixQuestion(gameData.questions[issue.itemIndex], issue.description);
                gameData.questions[issue.itemIndex] = fixedQuestion;
            }
            else if (issue.type == "Challenge")
            {
                var fixedChallenge = await FixChallenge(gameData.challenges[issue.itemIndex], issue.description);
                gameData.challenges[issue.itemIndex] = fixedChallenge;
            }
        }

        return gameData;
    }

    private async Task<QuestionData> FixQuestion(QuestionData question, string issue)
    {
        string prompt = _promptManager.CreateQuestionFixPrompt(question, issue);
        string response = await _gpt.GetCompletion(prompt);
        
        return ParseSingleQuestionFromResponse(response) ?? question;
    }

    private async Task<string> FixChallenge(string challenge, string issue)
    {
        string prompt = _promptManager.CreateChallengeFixPrompt(challenge, issue);
        string response = await _gpt.GetCompletion(prompt);
        
        return ParseSingleChallengeFromResponse(response) ?? challenge;
    }

    private string GenerateTitle(string topic)
    {
        return $"Tablero de {topic}";
    }

    private List<QuestionData> ParseQuestionsFromResponse(string response)
    {
        try
        {
            response = CleanJsonResponse(response);
            var questionsWrapper = JsonUtility.FromJson<QuestionsWrapper>("{\"questions\":" + response + "}");
            return questionsWrapper.questions ?? new List<QuestionData>();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error parsing questions: {ex.Message}");
            return new List<QuestionData>();
        }
    }

    private List<string> ParseChallengesFromResponse(string response)
    {
        try
        {
            response = CleanJsonResponse(response);
            var challengesWrapper = JsonUtility.FromJson<ChallengesWrapper>("{\"challenges\":" + response + "}");
            return challengesWrapper.challenges ?? new List<string>();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error parsing challenges: {ex.Message}");
            return new List<string>();
        }
    }

    private QuestionData ParseSingleQuestionFromResponse(string response)
    {
        try
        {
            response = CleanJsonResponse(response);
            return JsonUtility.FromJson<QuestionData>(response);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error parsing single question: {ex.Message}");
            return null;
        }
    }

    private string ParseSingleChallengeFromResponse(string response)
    {
        try
        {
            response = CleanJsonResponse(response);
            var challengeWrapper = JsonUtility.FromJson<SingleChallengeWrapper>(response);
            return challengeWrapper.challenge;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error parsing single challenge: {ex.Message}");
            return null;
        }
    }

    private string CleanJsonResponse(string response)
    {
        return response.Replace("```json", "").Replace("```", "").Trim();
    }
}

[Serializable]
public class QuestionsWrapper
{
    public List<QuestionData> questions;
}

[Serializable]
public class ChallengesWrapper
{
    public List<string> challenges;
}

[Serializable]
public class SingleChallengeWrapper
{
    public string challenge;
}
