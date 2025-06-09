using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = System.Random;

[Serializable]
public class GameData
{
	public string tittle = "";
	public string proposal = "";
	public List<string> challengesTypes = new List<string>();
	public int questionsCount = 0;
	public int challengesCount = 0;

	public List<string> challenges = new List<string>();
	public List<QuestionData> questions = new List<QuestionData>();
	public string imageURL;

	public void ShuffleQuestionOptions()
	{
		Random random = new Random();

		foreach (QuestionData question in questions)
		{
			string correctAnswer = question.options[question.correctId];

			List<int> indices = new List<int>();
			for (int i = 0; i < question.options.Length; i++) indices.Add(i);
			for (int i = 0; i < indices.Count; i++)
			{
				int j = random.Next(i, indices.Count);
				int temp = indices[i];
				indices[i] = indices[j];
				indices[j] = temp;
			}

			string[] shuffledOptions = new string[question.options.Length];
			for (int i = 0; i < indices.Count; i++)
			{
				shuffledOptions[i] = question.options[indices[i]];
				if (question.options[indices[i]] == correctAnswer)
				{
					question.correctId = i;
				}
			}

			question.options = shuffledOptions;
		}
	}
}

[Serializable]
public class AIJsonGenerator
{
	[SerializeField] private List<string> _defaultChallengeTypes = new List<string>();

	private GPT4Mini _gpt;
	private DALLE2 _dalle;

	private string _basePrompt;
	private bool _tryAgain = true;

	public AIJsonGenerator(string apiKey)
	{
		if (string.IsNullOrEmpty(apiKey))
		{
			Debug.LogError("AIJsonGenerator: API Key is required for OpenAI services.");
			return;
		}

		_gpt = new GPT4Mini(apiKey);
		_dalle = new DALLE2(apiKey);
	}

	public async Task<GameData> CreateBaseGameData(string unserPromptAnswer)
	{
		if (_gpt == null)
		{
			Debug.LogError("AIJsonGenerator: GPT service not initialized. Please provide a valid API key.");
			return null;
		}

		GameData gameData = new GameData();
		gameData.challengesCount = 3;
		gameData.questionsCount = 3;
		gameData.challengesTypes = _defaultChallengeTypes;
		
		_basePrompt = CreateGameDataPrompt(unserPromptAnswer, gameData);

		gameData = await GetGameData(_basePrompt);

		if (gameData != null)
			gameData.challengesTypes = _defaultChallengeTypes;

		return gameData;
	}

	public async Task<string> GetGameDataEvaluation(GameData gameData)
	{
		if (_gpt == null)
		{
			Debug.LogError("AIJsonGenerator: GPT service not initialized. Please provide a valid API key.");
			return null;
		}

		bool hasChallenges = gameData.challengesCount > 0;
		bool hasQuestions = gameData.questionsCount > 0;

		string gameDataJson = JsonUtility.ToJson(gameData);
		string promptPhase1 = $@"data inicial: {gameDataJson}  Asume el rol de un profesor experto en la proposal y el title del tablero." +
			" Analiza detalladamente " +
			(hasQuestions? "las preguntas (en adelante: elementos),":"") +
			(hasChallenges? " los desafíos (en adelante: elementos)":"") +
			"de la data inicial, realiza lo siguiente:" +
			"1. * *Análisis de nivel de dificultad de cada elemento. **" +
			(hasQuestions? "   - Analiza las respuestas de cada pregunta, para entender las claves de una buena respuesta." : "") +
			(hasChallenges? " - Analiza la duración de los desafíos propuestos.":"") +
			"2. * *Propuesta Mejorada: **" +
			"\t - Revisa y mejora la propuesta inicial del juego." +
			"\t- Crea recomendaciones que permitan generar elementos de igual calidad." +

			(hasQuestions? "Genera "+gameData.questionsCount+" preguntas con 4 opciones de respuesta, indicando la correcta, sobre el tema propuesto." +
			"Tras cada pregunta, analiza pregunta y respuestas, comprobando la veracidad de las mismas. " +
			"Así como que solo una de las respuestas es correcta. " +
			"Para cada pregunta desarrolla estos 3 análisis, muy sintetizados: Veracidad, Obviedad y Dificultad." +
			"Solo si es necesario añade el punto Corrección propuesta, tras el análisis anaterior." +
			"Pide que se corrijan todas las preguntas de veracidad incorrecta."+
			"Pide que se modifique cada pregunta de dificultad baja u obviedad alta." +
			"":"") +
			"";


		string responsePhase1 = await GetGPTResponse(promptPhase1);

		gameData.questions = new List<QuestionData>(gameData.questions.Take(1));
		gameData.challenges = new List<string>(gameData.challenges.Take(1));
		gameData.challengesTypes = new List<string>(gameData.challengesTypes.Take(1));

		string promptPhase2 = "Basándote en la información clave: " + responsePhase1 + " y siguiendo esta estructura de ejemplo: " +
						  JsonUtility.ToJson(gameData) +
						  " genera una data nueva. Responde unicamente con una estructura como la del ejemplo, sin comillas de código ni snipet. " +
						  " Incluye todos los elementos(preguntas y/o desafíos) que aparezcan en la información clave!" +
						  (hasChallenges ? "Los desafíos son sencillos: una sola cosa cada vez, simples. " : "") +
						  (hasChallenges ? "Rellena el array challengesTypes con etiquetas con los tipos de desafíos." : "") +
						  "Genera " + gameData.questionsCount + " preguntas y " + gameData.challengesCount + " desafíos, siquiendo la estructura de ejemplo, " +
						  "asigna esos valores a los campos questionsCount y challengeCount. " +
						  "Usa el título propuesto y la proposal nuevos, no los del ejemplo. " +
						  "Usa género neutro en las preguntas y desafíos, siempre. " +
						  "Tono informal. " +
						  "";

		string responsePhase2 = await GetGPTResponse(promptPhase2);

#if UNITY_EDITOR
		BoardPromptLogger.LogBoardCreation(gameData.tittle, promptPhase1, responsePhase1, promptPhase2, responsePhase2);
#endif

		return responsePhase2;
	}

	public async Task<GameData> GetGameData(string prompt = null)
	{
		if (_gpt == null)
		{
			Debug.LogError("AIJsonGenerator: GPT service not initialized. Please provide a valid API key.");
			return null;
		}

		string response = "";
		if (!string.IsNullOrEmpty(prompt))
			response = await GetGPTResponse(prompt);

		if (response == null)
		{
			if (_tryAgain)
			{
				_tryAgain = false;
				return await GetGameData(prompt);
			}
			else
				return null;
		}

		response.Replace("```", string.Empty);

		GameData data = null;

		try
		{
			Debug.Log("Data Response:" + response);
			data = JsonUtility.FromJson<GameData>(response);
		}
		catch
		{
			if (_tryAgain)
			{
				_tryAgain = false;
				return await GetGameData(prompt);
			}
			else
				return null;
		}
		data.challengesTypes = _defaultChallengeTypes;
		data.challengesCount = data.challenges.Count;
		data.questionsCount = data.questions.Count;

		return data;
	}

	private string LoadDefaultData()
	{
		string filePath = Path.Combine(Application.streamingAssetsPath, "defaultboard.json");

		if (File.Exists(filePath))
		{
			return File.ReadAllText(filePath);
		}
		else
		{
			Debug.LogError("File not found: " + filePath);
			return string.Empty;
		}
	}

	public async Task<string> GetGPTResponse(string prompt)
	{
		if (_gpt == null)
		{
			Debug.LogError("AIJsonGenerator: GPT service not initialized. Please provide a valid API key.");
			return null;
		}

		string response = await _gpt.GetCompletion(prompt);
		return response;
	}

	private string CreateBoardPrompt(GameData gameData)
	{
		return CreateGameDataPrompt(gameData.proposal, gameData);
	}

	private string CreateGameDataPrompt(string boardProposal, GameData gameData)
	{
		int questionsCount = gameData.questionsCount == 0? 0 : gameData.questionsCount+2;
		int challengesCount = gameData.challengesCount == 0? 0 : gameData.challengesCount+2;
		List<string> challengeTypes = gameData.challengesTypes;

		if (challengeTypes == null)
			challengeTypes = _defaultChallengeTypes;

		string challengeTypesPrompt = string.Empty;
		foreach (string type in _defaultChallengeTypes)
			challengeTypesPrompt += " " + type + ",";

		if (challengeTypesPrompt == string.Empty)
			challengeTypesPrompt = "Extrae los tipos de desafío de los ejemplos que hay al final del prompt.";

		boardProposal = boardProposal.Replace("\n", "").Replace("\r", "");


		string prompt = "Responde únicamente con un JSON siguiendo esta estructura exacta: La clase principal tiene los siguientes campos:" +
			" tittle y proposal, de tipo string que son un título corto y una descripción basada en los intereses proporcionados aquí: "
			+ boardProposal + ". challenges es una lista de strings donde cada elemento es la descripción de un desafío " +
			"breve y claro, sncillos juegos psicomágicos o no, que persigan el proposal, siempre´y únicamente dentro de estos tipo(s) de desafío:"
			+ challengeTypesPrompt + ". Ejemplo de challenges: 'descripción breve y clara de un desafío corto, relevante a la proposal', sin mencionar la psicomagia. " +
			"questions es una lista de objetos, donde cada objeto tiene tres campos: " +
			"statement de tipo string que es el enunciado una pregunta dificil y formativa relacionada con la proposal. " +
			"options es una lista de cuatro strings representando las opciones de respuesta " +
			"y correctId que es un entero entre 0 y 3 indicando el índice de la respuesta correcta. Típica estructura de quiz. " +
			"Ejemplos de preguntas: { statement: 'enunciado de la dificil pregunta', " +
			"options: ['opción la correcta', 'opción probable, pero incorrecta', 'opción  trampa', 'opción cómoca'], correctId: 0 }. " +
			"La respuesta correcta no es siempre la 0, SERÁ DISTINTA CADA VEZ!! Genera " +
			challengesCount + " challenges y " +
			questionsCount + " questions. " +
			"El tono es divertido e informal. Recuerda: debes responder únicamente con un JSON siguiendo exactamente la estructura indicada, " +
			"sin comentarios adicionales ni explicaciones. Usa genero neutro siempre que puedas.  " +
			"Todo el contenido en castellano. En texto plano. Y NO CODE SNIPET!! " +
			"Te recuerdo que el prompt para la proposal es: "
			+ boardProposal + ", que será el tema de la sesión, en el que se basarán los challenges y las questions.";

		if (gameData.challenges.Count > 0)
		{
			prompt += " Inspírate en estos challenges: ";
			gameData.challenges = gameData.challenges.Take(5).ToList();
			foreach (string challenge in gameData.challenges)
				prompt += challenge + ",";
		}

		if (gameData.questions.Count > 0)
		{
			prompt += " Inspírate en estas questions: ";
			gameData.questions = gameData.questions.Take(3).ToList();
			foreach (QuestionData question in gameData.questions)
				prompt += JsonUtility.ToJson(question) + ",";

			prompt = prompt.Replace("\"", "");
		}

		return prompt;
	}
}