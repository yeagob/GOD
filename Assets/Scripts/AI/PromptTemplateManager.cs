using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PromptTemplateManager
{
    public string CreateQuestionsPrompt(ContentGenerationRequest request)
    {
        string sampleQuestionsJson = "";
        if (request.samples.sampleQuestions.Count > 0)
        {
            var limitedSamples = request.samples.sampleQuestions.Take(2).ToList();
            sampleQuestionsJson = $"Inspírate en estos ejemplos: {JsonUtility.ToJson(new { questions = limitedSamples })}";
        }

        return $@"Genera exactamente {request.questionsCount} preguntas de quiz sobre: {request.proposal}

FORMATO REQUERIDO - Responde únicamente con un array JSON:
[
  {{
    ""statement"": ""Pregunta clara y específica"",
    ""options"": [""Opción correcta"", ""Opción incorrecta 1"", ""Opción incorrecta 2"", ""Opción incorrecta 3""],
    ""correctId"": 0
  }}
]

REGLAS:
- Preguntas formativas y desafiantes (nivel intermedio-avanzado)
- 4 opciones por pregunta, solo una correcta
- Respuesta correcta en posición aleatoria (0-3)
- Opciones plausibles pero claramente diferenciables
- Tono informal, género neutro
- Castellano

{sampleQuestionsJson}";
    }

    public string CreateChallengesPrompt(ContentGenerationRequest request)
    {
        string challengeTypesText = string.Join(", ", request.challengeTypes);
        string sampleChallengesJson = "";
        
        if (request.samples.sampleChallenges.Count > 0)
        {
            var limitedSamples = request.samples.sampleChallenges.Take(2).ToList();
            sampleChallengesJson = $"Inspírate en estos ejemplos: {string.Join(", ", limitedSamples.Select(c => $"\"{c}\""))}";
        }

        return $@"Genera exactamente {request.challengesCount} desafíos sobre: {request.proposal}

FORMATO REQUERIDO - Responde únicamente con un array JSON:
[""Desafío 1"", ""Desafío 2"", ""Desafío 3""]

TIPOS PERMITIDOS: {challengeTypesText}

REGLAS:
- Desafíos breves y claros (1-2 frases máximo)
- Actividades simples y realizables
- Relacionados con la temática: {request.proposal}
- Tono informal, género neutro
- Castellano
- NO mencionar ""psicomagia"" explícitamente

{sampleChallengesJson}";
    }

    public string CreateQuestionFixPrompt(QuestionData question, string issue)
    {
        return $@"Corrige esta pregunta: {JsonUtility.ToJson(question)}

PROBLEMA DETECTADO: {issue}

FORMATO REQUERIDO - Responde únicamente con el JSON corregido:
{{
  ""statement"": ""Pregunta corregida"",
  ""options"": [""Opción 1"", ""Opción 2"", ""Opción 3"", ""Opción 4""],
  ""correctId"": 0
}}

REGLAS:
- Mantén la temática original
- Asegura que solo una respuesta sea correcta
- Opciones plausibles y diferenciables
- Tono informal, género neutro";
    }

    public string CreateChallengeFixPrompt(string challenge, string issue)
    {
        return $@"Corrige este desafío: ""{challenge}""

PROBLEMA DETECTADO: {issue}

FORMATO REQUERIDO - Responde únicamente con el JSON:
{{
  ""challenge"": ""Desafío corregido""
}}

REGLAS:
- Mantén la temática original
- Actividad breve y clara
- Realizable y apropiada
- Tono informal, género neutro";
    }

    public string CreateStreamlinedGenerationPrompt(ContentGenerationRequest request)
    {
        var sampleData = PrepareOptimizedSamples(request.samples);
        
        string questionsSection = request.questionsCount > 0 
            ? $@"""questions"": [/* {request.questionsCount} preguntas con formato estándar */],"
            : "";
            
        string challengesSection = request.challengesCount > 0 
            ? $@"""challenges"": [/* {request.challengesCount} desafíos como strings */],"
            : "";

        return $@"Genera contenido para tablero educativo sobre: {request.proposal}

FORMATO REQUERIDO - Responde únicamente con JSON:
{{
  ""tittle"": ""Título conciso del tablero"",
  ""proposal"": ""{request.proposal}"",
  {questionsSection}
  {challengesSection}
  ""questionsCount"": {request.questionsCount},
  ""challengesCount"": {request.challengesCount}
}}

ESPECIFICACIONES:
- Preguntas: nivel intermedio, 4 opciones, correctId aleatorio
- Desafíos: breves, claros, tipos permitidos: {string.Join(", ", request.challengeTypes)}
- Tono informal, género neutro, castellano
- Contenido educativo y apropiado

{sampleData}";
    }

    private string PrepareOptimizedSamples(ContentSample samples)
    {
        if (samples.sampleQuestions.Count == 0 && samples.sampleChallenges.Count == 0)
        {
            return "";
        }

        string result = "EJEMPLOS PARA INSPIRARTE:\n";
        
        if (samples.sampleQuestions.Count > 0)
        {
            var limitedQuestions = samples.sampleQuestions.Take(1).ToList();
            result += $"Pregunta ejemplo: {JsonUtility.ToJson(limitedQuestions[0])}\n";
        }
        
        if (samples.sampleChallenges.Count > 0)
        {
            var limitedChallenges = samples.sampleChallenges.Take(1).ToList();
            result += $"Desafío ejemplo: \"{limitedChallenges[0]}\"\n";
        }
        
        return result;
    }
}
