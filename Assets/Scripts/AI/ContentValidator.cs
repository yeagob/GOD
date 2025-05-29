using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ContentValidator
{
    private readonly List<string> _commonWrongAnswers = new List<string> 
    { 
        "todas las anteriores", "ninguna de las anteriores", "no sé", "quizás" 
    };

    public List<ContentIssue> ValidateContent(GameData gameData)
    {
        var issues = new List<ContentIssue>();
        
        issues.AddRange(ValidateQuestions(gameData.questions));
        issues.AddRange(ValidateChallenges(gameData.challenges));
        issues.AddRange(ValidateGeneralStructure(gameData));
        
        return issues;
    }

    private List<ContentIssue> ValidateQuestions(List<QuestionData> questions)
    {
        var issues = new List<ContentIssue>();
        
        for (int i = 0; i < questions.Count; i++)
        {
            var question = questions[i];
            
            if (string.IsNullOrEmpty(question.statement))
            {
                issues.Add(CreateIssue("Question", i, "Pregunta vacía", "Critical"));
                continue;
            }
            
            if (question.options == null || question.options.Length != 4)
            {
                issues.Add(CreateIssue("Question", i, "Debe tener exactamente 4 opciones", "Critical"));
                continue;
            }
            
            if (question.correctId < 0 || question.correctId >= question.options.Length)
            {
                issues.Add(CreateIssue("Question", i, "Índice de respuesta correcta inválido", "Critical"));
                continue;
            }
            
            if (HasDuplicateOptions(question.options))
            {
                issues.Add(CreateIssue("Question", i, "Opciones duplicadas detectadas", "Medium"));
            }
            
            if (IsQuestionTooShort(question.statement))
            {
                issues.Add(CreateIssue("Question", i, "Pregunta demasiado corta o poco específica", "Medium"));
            }
            
            if (HasObviousAnswer(question))
            {
                issues.Add(CreateIssue("Question", i, "Respuesta demasiado obvia", "Medium"));
            }
            
            if (HasCommonWrongAnswerPatterns(question.options))
            {
                issues.Add(CreateIssue("Question", i, "Contiene patrones de respuesta incorrecta comunes", "Low"));
            }
        }
        
        return issues;
    }

    private List<ContentIssue> ValidateChallenges(List<string> challenges)
    {
        var issues = new List<ContentIssue>();
        
        for (int i = 0; i < challenges.Count; i++)
        {
            var challenge = challenges[i];
            
            if (string.IsNullOrEmpty(challenge))
            {
                issues.Add(CreateIssue("Challenge", i, "Desafío vacío", "Critical"));
                continue;
            }
            
            if (IsChallengeTooBrief(challenge))
            {
                issues.Add(CreateIssue("Challenge", i, "Desafío demasiado breve o vago", "Medium"));
            }
            
            if (IsChallengeTooLong(challenge))
            {
                issues.Add(CreateIssue("Challenge", i, "Desafío demasiado largo o complejo", "Medium"));
            }
            
            if (HasInappropriateContent(challenge))
            {
                issues.Add(CreateIssue("Challenge", i, "Contenido potencialmente inapropiado", "High"));
            }
        }
        
        return issues;
    }

    private List<ContentIssue> ValidateGeneralStructure(GameData gameData)
    {
        var issues = new List<ContentIssue>();
        
        if (string.IsNullOrEmpty(gameData.tittle))
        {
            issues.Add(CreateIssue("General", -1, "Título vacío", "Medium"));
        }
        
        if (string.IsNullOrEmpty(gameData.proposal))
        {
            issues.Add(CreateIssue("General", -1, "Propuesta vacía", "Critical"));
        }
        
        if (gameData.questions.Count != gameData.questionsCount)
        {
            issues.Add(CreateIssue("General", -1, "Número de preguntas no coincide", "Medium"));
        }
        
        if (gameData.challenges.Count != gameData.challengesCount)
        {
            issues.Add(CreateIssue("General", -1, "Número de desafíos no coincide", "Medium"));
        }
        
        return issues;
    }

    private ContentIssue CreateIssue(string type, int itemIndex, string description, string severity)
    {
        return new ContentIssue
        {
            type = type,
            itemIndex = itemIndex,
            description = description,
            severity = severity
        };
    }

    private bool HasDuplicateOptions(string[] options)
    {
        return options.GroupBy(x => x.ToLower().Trim()).Any(g => g.Count() > 1);
    }

    private bool IsQuestionTooShort(string statement)
    {
        return statement.Length < 20 || statement.Split(' ').Length < 5;
    }

    private bool HasObviousAnswer(QuestionData question)
    {
        string correctAnswer = question.options[question.correctId].ToLower();
        string statement = question.statement.ToLower();
        
        return statement.Contains(correctAnswer) || correctAnswer.Length < 5;
    }

    private bool HasCommonWrongAnswerPatterns(string[] options)
    {
        return options.Any(option => 
            _commonWrongAnswers.Any(pattern => 
                option.ToLower().Contains(pattern.ToLower())));
    }

    private bool IsChallengeTooBrief(string challenge)
    {
        return challenge.Length < 15 || challenge.Split(' ').Length < 4;
    }

    private bool IsChallengeTooLong(string challenge)
    {
        return challenge.Length > 200 || challenge.Split(' ').Length > 30;
    }

    private bool HasInappropriateContent(string challenge)
    {
        var inappropriateWords = new List<string> 
        { 
            "peligroso", "arriesgado", "ilegal", "violento", "dañino" 
        };
        
        return inappropriateWords.Any(word => 
            challenge.ToLower().Contains(word.ToLower()));
    }
}
