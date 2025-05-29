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

REGLAS CRÍTICAS PARA CALIDAD:
- VERACIDAD: La respuesta correcta debe ser 100% verdadera y verificable
- NO OBVIEDAD: La respuesta correcta NO debe estar contenida en la pregunta
- OPCIONES PLAUSIBLES: Las incorrectas deben ser creíbles pero claramente diferentes
- NIVEL INTERMEDIO: Ni muy fácil ni muy difícil
- RESPUESTA ALEATORIA: correctId debe variar entre 0-3 (no siempre 0)
- LONGITUD SIMILAR: Todas las opciones deben tener longitud similar
- SIN AMBIGÜEDAD: Evitar palabras como ""siempre"", ""nunca"", ""depende""
- SIN CONTRADICCIONES: Las opciones no deben contradecir la pregunta

EJEMPLOS DE MALA CALIDAD A EVITAR:
❌ ""¿Cuál es la capital de Francia?"" → ""París"" (demasiado obvio)
❌ ""¿Qué significa democracia?"" → ""Todas las anteriores"" (patrón malo)
❌ Pregunta sobre matemáticas → respuesta sobre historia (incoherente)

EJEMPLOS DE BUENA CALIDAD:
✅ Pregunta específica con datos concretos
✅ Opciones que requieren conocimiento pero son diferenciables
✅ Respuesta correcta verificable y no obvia

Tema específico: {request.proposal}
Tono informal, género neutro, castellano

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

REGLAS PARA DESAFÍOS DE CALIDAD:
- ESPECÍFICOS: Acción clara y concreta, no vaga
- REALIZABLES: Que se puedan hacer en 2-5 minutos
- RELACIONADOS: Conectados con la temática: {request.proposal}
- EDUCATIVOS: Que refuercen el aprendizaje del tema
- VARIADOS: Diferentes tipos de actividades
- SIN GENÉRICOS: Evitar ""piensa en..."", ""reflexiona sobre...""

EJEMPLOS ESPECÍFICOS POR TIPO:
- Reflexión personal: ""Escribe 3 situaciones donde aplicarías [concepto específico]""
- Acción física: ""Dibuja un diagrama que muestre [proceso específico]""
- Creativo: ""Inventa una historia de 50 palabras usando [términos específicos]""
- Social: ""Explica a un compañero [concepto] usando solo ejemplos cotidianos""
- Mindfulness: ""Respira profundo y visualiza [escenario relacionado con el tema]""

EVITAR:
❌ ""Haz algo relacionado con..."" (muy vago)
❌ ""Piensa en el tema"" (genérico)
❌ Desafíos que no requieren conocimiento del tema

Tono informal, género neutro, castellano

{sampleChallengesJson}";
    }

    public string CreateQuestionFixPrompt(QuestionData question, string issue)
    {
        return $@"Corrige esta pregunta con problema de calidad: {JsonUtility.ToJson(question)}

PROBLEMA DETECTADO: {issue}

INSTRUCCIONES ESPECÍFICAS DE CORRECCIÓN:
- Si es ""respuesta obvia"": Reformula para que la respuesta no esté en la pregunta
- Si es ""respuesta incorrecta"": Verifica la veracidad y corrige la respuesta correcta
- Si es ""opciones similares"": Haz las opciones más diferentes entre sí
- Si es ""opciones vagas"": Sé más específico en todas las opciones

FORMATO REQUERIDO - Responde únicamente con el JSON corregido:
{{
  ""statement"": ""Pregunta corregida específica y clara"",
  ""options"": [""Opción específica 1"", ""Opción específica 2"", ""Opción específica 3"", ""Opción específica 4""],
  ""correctId"": [número 0-3]
}}

VERIFICACIONES FINALES:
- ✅ ¿La respuesta es 100% verdadera?
- ✅ ¿Las opciones son plausibles pero diferentes?
- ✅ ¿No hay obviedad en la respuesta?
- ✅ ¿Las opciones tienen longitud similar?

Mantén la temática original, mejora la calidad.";
    }

    public string CreateChallengeFixPrompt(string challenge, string issue)
    {
        return $@"Corrige este desafío con problema: ""{challenge}""

PROBLEMA DETECTADO: {issue}

INSTRUCCIONES DE CORRECCIÓN:
- Si es ""vago"": Sé más específico en la acción requerida
- Si es ""demasiado breve"": Agrega detalles concretos
- Si es ""demasiado largo"": Simplifica manteniendo la esencia

FORMATO REQUERIDO - Responde únicamente con el JSON:
{{
  ""challenge"": ""Desafío corregido específico y claro""
}}

CRITERIOS DE CALIDAD:
- Acción específica y medible
- Relacionado con la temática original
- Realizable en 2-5 minutos
- Educativo y engaging

Mantén la temática original, mejora la especificidad.";
    }

    public string CreateDescriptionGenerationPrompt(GameData gameData)
    {
        var topicsFromQuestions = gameData.questions.Take(3).Select(q => ExtractTopicFromQuestion(q.statement));
        var challengeTypes = gameData.challenges.Take(3).Select(c => ExtractActivityFromChallenge(c));

        return $@"Basándote en este contenido educativo, genera una descripción atractiva del tablero:

TÍTULO: {gameData.tittle}
TEMA ORIGINAL: {gameData.proposal}

PREGUNTAS INCLUIDAS:
{string.Join("\n", gameData.questions.Take(3).Select(q => $"- {q.statement}"))}

DESAFÍOS INCLUIDOS:
{string.Join("\n", gameData.challenges.Take(3).Select(c => $"- {c}"))}

GENERA una descripción de 2-3 oraciones que:
- Resuma el contenido del tablero de forma atractiva
- Mencione los tipos de conocimientos que se van a adquirir
- Incluya los tipos de actividades que se realizarán
- Sea motivadora y específica (no genérica)
- Use tono informal y educativo

FORMATO REQUERIDO - Responde únicamente con el texto:
[Tu descripción aquí]

EJEMPLO:
""Descubre los secretos de las matemáticas a través de problemas prácticos y desafíos creativos. Aprenderás sobre fracciones, geometría y álgebra mientras resuelves enigmas y creas tus propias representaciones matemáticas.""

NO incluyas frases genéricas como ""Este tablero es sobre..."" o ""Aprenderás muchas cosas"".";
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
  ""proposal"": ""Descripción breve y atractiva del tablero basada en el contenido generado"",
  {questionsSection}
  {challengesSection}
  ""questionsCount"": {request.questionsCount},
  ""challengesCount"": {request.challengesCount}
}}

ESPECIFICACIONES DE CALIDAD:
- Preguntas: nivel intermedio, veracidad 100%, opciones plausibles, correctId aleatorio
- Desafíos: específicos, realizables, educativos, tipos: {string.Join(", ", request.challengeTypes)}
- Proposal: descripción atractiva basada en el contenido real generado (no copiar input)
- Tono informal, género neutro, castellano

CRITERIOS DE EXCELENCIA:
- NO respuestas obvias ni contradictorias
- NO opciones vagas como ""depende"" o ""todas las anteriores""
- SÍ desafíos específicos y medibles
- SÍ description que refleje el contenido real

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

    private string ExtractTopicFromQuestion(string question)
    {
        var words = question.Split(' ').Where(w => w.Length > 4).Take(2);
        return string.Join(" ", words);
    }

    private string ExtractActivityFromChallenge(string challenge)
    {
        if (challenge.ToLower().Contains("dibuja")) return "dibujo";
        if (challenge.ToLower().Contains("escribe")) return "escritura";
        if (challenge.ToLower().Contains("explica")) return "explicación";
        if (challenge.ToLower().Contains("respira")) return "mindfulness";
        return "actividad práctica";
    }
}
