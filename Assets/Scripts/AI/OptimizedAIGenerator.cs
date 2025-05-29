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

            // Generar descripción automática basada en el contenido
            gameData.proposal = await GenerateDescription(gameData);

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
            proposal = request.proposal, // Se reemplazará con la descripción generada
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

    private async Task<string> GenerateDescription(GameData gameData)
    {
        if (gameData.questions.Count == 0 && gameData.challenges.Count == 0)
        {
            return gameData.proposal; // Fallback to original if no content
        }

        try
        {
            string prompt = _promptManager.CreateDescriptionGenerationPrompt(gameData);
            string response = await _gpt.GetCompletion(prompt);
            
            string cleanResponse = response.Trim().Replace("\"", "");
            
            // Validar que la descripción no sea muy corta o genérica
            if (cleanResponse.Length < 50 || cleanResponse.ToLower().Contains("este tablero"))
            {
                return $"Explora {gameData.tittle.ToLower()} a través de preguntas desafiantes y actividades prácticas que refuerzan tu aprendizaje.";
            }
            
            return cleanResponse;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Error generating description: {ex.Message}");
            return $"Descubre y aprende sobre {gameData.tittle.ToLower()} mediante preguntas interactivas y desafíos educativos.";
        }
    }

    private async Task<(GameData gameData, string feedback, List<ContentIssue> issues)> ValidateAndOptimizeContent(GameData gameData)
    {
        var issues = _validator.ValidateContent(gameData);
        
        if (issues.Count == 0)
        {
            return (gameData, "Contenido validado correctamente", issues);
        }

        var criticalIssues = issues.FindAll(i => i.severity == "Critical");
        var highIssues = issues.FindAll(i => i.severity == "High");
        
        // Intentar corregir issues críticos y de alta prioridad
        if (criticalIssues.Count > 0 || highIssues.Count > 0)
        {
            var fixedGameData = await FixCriticalAndHighIssues(gameData, criticalIssues.Count > 0 ? criticalIssues : highIssues);
            
            // Re-validar después de las correcciones
            var newIssues = _validator.ValidateContent(fixedGameData);
            string feedback = $"Contenido corregido automáticamente. Issues resueltos: {issues.Count - newIssues.Count}";
            
            return (fixedGameData, feedback, newIssues);
        }

        return (gameData, "Contenido con issues menores", issues);
    }

    private async Task<GameData> FixCriticalAndHighIssues(GameData gameData, List<ContentIssue> issuesToFix)
    {
        var fixedGameData = gameData;
        int maxRetries = 2; // Límite para evitar loops infinitos
        
        foreach (var issue in issuesToFix)
        {
            int retryCount = 0;
            bool fixed = false;
            
            while (!fixed && retryCount < maxRetries)
            {
                try
                {
                    if (issue.type == "Question" && issue.itemIndex < fixedGameData.questions.Count)
                    {
                        var fixedQuestion = await FixQuestion(fixedGameData.questions[issue.itemIndex], issue.description);
                        if (fixedQuestion != null)
                        {
                            fixedGameData.questions[issue.itemIndex] = fixedQuestion;
                            fixed = true;
                        }
                    }
                    else if (issue.type == "Challenge" && issue.itemIndex < fixedGameData.challenges.Count)
                    {
                        var fixedChallenge = await FixChallenge(fixedGameData.challenges[issue.itemIndex], issue.description);
                        if (!string.IsNullOrEmpty(fixedChallenge))
                        {
                            fixedGameData.challenges[issue.itemIndex] = fixedChallenge;
                            fixed = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Error fixing issue: {ex.Message}");
                }
                
                retryCount++;
            }
            
            if (!fixed)
            {
                Debug.LogWarning($"Could not fix issue: {issue.description} for {issue.type} {issue.itemIndex}");
            }
        }

        return fixedGameData;
    }

    private async Task<QuestionData> FixQuestion(QuestionData question, string issue)
    {
        try
        {
            string prompt = _promptManager.CreateQuestionFixPrompt(question, issue);
            string response = await _gpt.GetCompletion(prompt);
            
            var fixedQuestion = ParseSingleQuestionFromResponse(response);
            
            // Validar que la corrección sea válida
            if (fixedQuestion != null && 
                !string.IsNullOrEmpty(fixedQuestion.statement) && 
                fixedQuestion.options != null && 
                fixedQuestion.options.Length == 4 &&
                fixedQuestion.correctId >= 0 && 
                fixedQuestion.correctId < 4)
            {
                return fixedQuestion;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Error fixing question: {ex.Message}");
        }
        
        return question; // Fallback to original if fix fails
    }

    private async Task<string> FixChallenge(string challenge, string issue)
    {
        try
        {
            string prompt = _promptManager.CreateChallengeFixPrompt(challenge, issue);
            string response = await _gpt.GetCompletion(prompt);
            
            var fixedChallenge = ParseSingleChallengeFromResponse(response);
            
            // Validar que la corrección sea válida
            if (!string.IsNullOrEmpty(fixedChallenge) && fixedChallenge.Length > 10)
            {
                return fixedChallenge;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Error fixing challenge: {ex.Message}");
        }
        
        return challenge; // Fallback to original if fix fails
    }

    private string GenerateTitle(string topic)
    {
        if (string.IsNullOrEmpty(topic))
        {
            return "Tablero Educativo";
        }
        
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
