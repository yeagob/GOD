using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[Serializable]
public class GameData
{
	//Config
	public string tittle = "";
	public string proposal = "";
	public List<string> challengesTypes = new List<string>();
	public int questionsCount = 0;
	public int challengesCount = 0;

	//Board Data
	public List<string> challenges = new List<string>();
	public List<QuestionData> questions = new List<QuestionData>();
	public string imageURL;

	//TODO: Separate Config from BoardData in a diferent clases
}

[Serializable]
public class AIJsonGenerator
{
	[SerializeField] private List<string> _defaultChallengeTypes = new List<string>();

	private const string _apiKey = "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";

	private GPT4Mini _gpt;
	private DALLE2 _dalle;//NOT IN USE!!!

	private string _basePrompt;

	private bool _tryAgain = true;


	public AIJsonGenerator()
	{
		_gpt = new GPT4Mini(_apiKey);
		_dalle = new DALLE2(_apiKey);
	}


	public async Task<GameData> CreateBaseGameData(string unserPromptAnswer)
	{
		GameData gameData = new GameData();
		gameData.challengesCount = 3;
		gameData.questionsCount = 3;
		gameData.challengesTypes = _defaultChallengeTypes;
		
		_basePrompt = CreateGameDataPrompt(unserPromptAnswer, gameData);

		gameData = await GetGameData(_basePrompt);
		gameData.challengesCount = 10;
		gameData.challengesTypes = _defaultChallengeTypes;

		return gameData;
	}

	public async Task<GameData> GetGameData(string prompt = null)
	{
		string response = "";
		if (!string.IsNullOrEmpty(prompt))
			response = await GetGPTResponse(prompt);

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

	public async Task<BoardData> GetJsonBoard(GameData gameData)
	{
		BoardData boardData = null;
		bool loadDefault = gameData == null;

		if (!loadDefault)
		{
			//To avoid overrides
			string boardTittle = gameData.tittle;
			string boardProposal = gameData.proposal;
			string imageURL = gameData.imageURL;

			string boardPrompt = CreateBoardPrompt(gameData);
			GameData data = await GetGameData(boardPrompt);

			boardData = ProcessData(data);

			//Set previous title & Proposal
			boardData.tittle = boardTittle;
			boardData.proposal = boardProposal;
			boardData.imageURL = imageURL;
		}
		else
			//TODO No me gusta cargar el tablero por defecto, me gustaría que diese error y te tirase para atrás!
			boardData = new BoardData(LoadDefaultData());

		return boardData;
	}

	private BoardData ProcessData(GameData data)
	{
		return new BoardData(data);
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
		string response = await _gpt.GetCompletion(prompt);
		return response;
	}

	//public async void GenerateAndDisplayImage(string prompt, int tileID)
	//{
	//	Sprite sprite = await _dalle.GenerateImage(prompt, tileID);
	//	_spriteRenderer.sprite = sprite;
	//}

	private string CreateBoardPrompt(GameData gameData)
	{
		return CreateGameDataPrompt(gameData.proposal, gameData);
	}

	private string CreateGameDataPrompt(string boardProposal, GameData gameData)
	{
		int questionsCount = gameData.questionsCount;
		int challengesCount = gameData.challengesCount;
		List<string> challengeTypes = gameData.challengesTypes;

		if (challengeTypes == null)
			challengeTypes = _defaultChallengeTypes;

		string challengeTypesPrompt = string.Empty;
		foreach (string type in _defaultChallengeTypes)
		{
			challengeTypesPrompt += " " + type + ",";
		}

		string prompt = "Responde únicamente con un JSON siguiendo esta estructura exacta: La clase principal tiene los siguientes campos:" +
			" tittle y proposal, de tipo string que son un título corto y una descripción basada en los intereses proporcionados aquí: "
			+ boardProposal + ". challenges es una lista de strings donde cada elemento es la descripción de un desafío " +
			"breve y claro, sncillos juegos psicomágicos o no, que persigan el proposal, siempre´y únicamente dentro de estos tipos de desafío:"
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

		//Challenge inspiration
		if (gameData.challenges.Count > 0)
		{
			prompt += " Inspírate en estos challenges: ";
			gameData.challenges = gameData.challenges.Take(3).ToList();
			foreach (string challenge in gameData.challenges)
				prompt += challenge + ",";
		}

		//Questions inspiration
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

