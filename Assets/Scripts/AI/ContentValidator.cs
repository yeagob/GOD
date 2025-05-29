using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ContentValidator
{
    private readonly List<string> _commonWrongAnswers = new List<string> 
    { 
        "todas las anteriores", "ninguna de las anteriores", "no sé", "quizás", "no lo sé", "depende"
    };

    private readonly List<string> _obviousAnswerWords = new List<string>
    {
        "siempre", "nunca", "todo", "nada", "todos", "nadie", "imposible", "seguro"
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
                issues.Add(CreateIssue("Question", i, "Opciones duplicadas detectadas", "High"));
            }
            
            if (IsQuestionTooShort(question.statement))
            {
                issues.Add(CreateIssue("Question", i, "Pregunta demasiado corta o poco específica", "Medium"));
            }
            
            if (HasObviousAnswer(question))
            {
                issues.Add(CreateIssue("Question", i, "Respuesta demasiado obvia", "High"));
            }
            
            if (HasCommonWrongAnswerPatterns(question.options))
            {
                issues.Add(CreateIssue("Question", i, "Contiene patrones de respuesta incorrecta comunes", "Medium"));
            }

            if (HasPotentiallyIncorrectAnswer(question))
            {
                issues.Add(CreateIssue("Question", i, "La respuesta correcta parece incorrecta o dudosa", "Critical"));
            }

            if (AreAllAnswersSimilar(question.options))
            {
                issues.Add(CreateIssue("Question", i, "Todas las opciones son muy similares", "High"));
            }

            if (HasVagueOrAmbiguousOptions(question.options))
            {
                issues.Add(CreateIssue("Question", i, "Opciones vagas o ambiguas", "Medium"));
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

            if (IsChallengeVague(challenge))
            {
                issues.Add(CreateIssue("Challenge", i, "Desafío demasiado vago o genérico", "Medium"));
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
        
        // Verificar si la respuesta está contenida en la pregunta
        if (statement.Contains(correctAnswer) && correctAnswer.Length > 3)
        {
            return true;
        }

        // Verificar respuestas muy cortas
        if (correctAnswer.Length < 4)
        {
            return true;
        }

        // Verificar palabras obvias
        foreach (var word in _obviousAnswerWords)
        {
            if (correctAnswer.Contains(word))
            {
                return true;
            }
        }

        // Verificar si solo una opción es diferente del resto
        var optionLengths = question.options.Select(o => o.Length).ToList();
        var correctLength = question.options[question.correctId].Length;
        var otherLengths = optionLengths.Where((length, index) => index != question.correctId);
        
        if (correctLength > otherLengths.Max() * 2 || correctLength < otherLengths.Min() / 2)
        {
            return true;
        }

        return false;
    }

    private bool HasCommonWrongAnswerPatterns(string[] options)
    {
        return options.Any(option => 
            _commonWrongAnswers.Any(pattern => 
                option.ToLower().Contains(pattern.ToLower())));
    }

    private bool HasPotentiallyIncorrectAnswer(QuestionData question)
    {
        string statement = question.statement.ToLower();
        string correctAnswer = question.options[question.correctId].ToLower();

        // Detectar contradicciones obvias
        if (statement.Contains("no") && !correctAnswer.Contains("no") && correctAnswer.Contains("sí"))
        {
            return true;
        }

        // Detectar respuestas que contradicen la pregunta
        var questionWords = statement.Split(' ').Where(w => w.Length > 3).ToList();
        var answerWords = correctAnswer.Split(' ').Where(w => w.Length > 3).ToList();

        var contradictions = new Dictionary<string, string[]>
        {
            { "mayor", new[] { "menor", "pequeño", "bajo" } },
            { "menor", new[] { "mayor", "grande", "alto" } },
            { "verdadero", new[] { "falso", "incorrecto" } },
            { "falso", new[] { "verdadero", "correcto" } },
            { "antes", new[] { "después", "posterior" } },
            { "después", new[] { "antes", "anterior" } }
        };

        foreach (var qWord in questionWords)
        {
            if (contradictions.ContainsKey(qWord))
            {
                var contradictoryWords = contradictions[qWord];
                if (answerWords.Any(aWord => contradictoryWords.Contains(aWord)))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool AreAllAnswersSimilar(string[] options)
    {
        if (options.Length < 2) return false;

        var words = options.Select(o => o.ToLower().Split(' ')).ToList();
        var commonWords = words[0].ToList();

        foreach (var wordList in words.Skip(1))
        {
            commonWords = commonWords.Intersect(wordList).ToList();
        }

        // Si más del 60% de las palabras son comunes en todas las opciones
        var averageWordsPerOption = words.Average(w => w.Length);
        return commonWords.Count > averageWordsPerOption * 0.6;
    }

    private bool HasVagueOrAmbiguousOptions(string[] options)
    {
        var vagueWords = new[] { "quizás", "tal vez", "posiblemente", "probablemente", "puede ser", "depende" };
        
        return options.Any(option => 
            vagueWords.Any(vague => option.ToLower().Contains(vague)));
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

    private bool IsChallengeVague(string challenge)
    {
        var vagueWords = new[] { "algo", "cualquier", "haz algo", "piensa en", "reflexiona sobre" };
        var challengeLower = challenge.ToLower();
        
        return vagueWords.Any(vague => challengeLower.Contains(vague)) ||
               challenge.Split(' ').Length < 6;
    }
}
