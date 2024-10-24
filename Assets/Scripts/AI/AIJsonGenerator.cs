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

		if (gameData != null)
			gameData.challengesTypes = _defaultChallengeTypes;

		return gameData;
	}

	public async Task<string> GetGameDataEvaluation(GameData gameData)
	{
		bool hasChallenges = gameData.challengesCount > 0;
		bool hasQuestions = gameData.questionsCount > 0;

		string gameDataJson = JsonUtility.ToJson(gameData);
		string promptPhase1 = $@"data inicial: {gameDataJson}  Asume el rol de un profesor experto en la proposal y el title del tablero." +
			" Bas�ndote en las preguntas y desaf�os generados previamente, realiza lo siguiente:" +
			"1. * *An�lisis de las Preguntas/Desaf�os: **" +
			"	- Identifica y enumera brevemente las caracter�sticas clave que hacen que estas preguntas/desaf�os sean de alta calidad." +
			(hasQuestions? "   - Analiza y valora la dificultad y el grado de conocimientos necesarios para respoder a las preguntas.":"") +
			"2. * *Caracter�sticas de Buenas Preguntas/Desaf�os: **" +
			"	 -Genera una lista breve de las caracter�sticas que deben tener las preguntas para ser igual de buenas." +
			"   -Explica brevemente c�mo cada caracter�stica contribuye a la calidad y eficacia de las /desaf�os." +
			"   -En caso de preguntas, analiza las respuestas y los tipos de respuestas que hay" +
			"3. * *Propuesta Mejorada: **" +
			"	 -Revisa y mejora la propuesta inicial del juego." +
			"	- A�ade recomendaciones y refinamientos que aumenten su atractivo y valor educativo." +
			//" FORMATO ESPERADO: informa que tiene que comportarse como profesor en la materia y describe " +
			//"como tiene que generar las preguntas/desaf�os, cuales son sus caracter�sticas clave y cuales son las pautas para generar " +
			//"preguntas/desaf�os que sigan lo m�s fielmente posible la misma l�nea que las de la data inicial. " +
			//"Adjunta las preguntas y desaf�os de la data inicial como ejemplos a seguir, a dem�s deben ser incluidos/as." +
			"Indica el n�mero de preguntas y/o desaf�os que habr� que generar, en base a la data inicial antes, " +
			"en los campos questionsCount y challengesCount. " +
			"Indica cual es la nueva descripci�n para el t�tulo y la proposal. " +

			(hasQuestions? "Genera "+gameData.questionsCount+" preguntas con 4 opciones de respuesta, indicando la correcta, sobre el tema propuesto." +
			"Desordena las respuestas para que la respuesta correcta no est� siempre en la misma posici�n." +
			"Tras cada pregunta, analiza pregunta y respuestas, comprobando la veracidad de las mismas. " +
			"As� como que solo una de las respuestas es correcta. " +
			//"Revisa tambi�n si las preguntas son obvias o redundantes y si est�n correstamente expresadas. A dem�s analiza si son demasiado f�ciles(para estudiantes b�sicos) y como podr�an complicarse, si fuera necesario." +
			"Prop�n correcci�n en todas las preguntas de veracidad incorrecta."+
			"Prop�n correcci�n en todas las preguntas de dificultad baja u obviedad alta." +
			"Describe como corregir las preguntas y respuestas." +
			"Para cada pregunta desarrolla estos 3 an�lisis: Veracidad, Obviedad y Dificultad." +
			"Solo si es necesario a�ade el punto Correcci�n propuesta: " +
			"":"") +
			//"Por ultimo, genera una secuencia aleatoria de valores de 0-3 de questionCount cantidad de valores, " +
			//"con la etiqueta RangoAleatorio(ej): Q1(0), Q2(2), etc" +
			"";


		//Debug.Log("Prompt Phase1: " + promptPhase1);

		string responsePhase1 = await GetGPTResponse(promptPhase1);

		//Debug.Log("Response Phase1: " + promptPhase1);

		gameData.questions = new List<QuestionData>(gameData.questions.Take(1));
		gameData.challenges = new List<string>(gameData.challenges.Take(1));
		gameData.challengesTypes = new List<string>(gameData.challengesTypes.Take(1));

		string promptPhase2 = "Bas�ndote en la informaci�n clave: " + responsePhase1 + " y siguiendo esta estructura de ejemplo: " +
							  JsonUtility.ToJson(gameData) +
							  " genera una data nueva. Responde unicamente con una estructura como la del ejemplo, sin comillas de c�digo ni snipet. " +
							  " Incluye las preguntas y/o desaf�os que aparezcan en la informaci�n clave�." +
							 (hasQuestions? "Usa los valores de RangoAleatorio para decidir cual ser� la respuesta correcta, de cada pregunta, al generarlas (ccorectId). ":"") +
					//		  "Solo si challengeCount > 0 dale un toque psicom�gico, oculto, a los desaf�os. " +
							  (hasChallenges ? "Los desaf�os son sencillos: una sola cosa cada vez, simples. " : "") +
							  (hasChallenges ? "Rellena el array challengesTypes con etiquetas con los tipos de desaf�os." : "") +
							  "Genera " + gameData.questionsCount + " preguntas y " + gameData.challengesCount + " desaf�os, siquiendo la estructura de ejemplo, " +
							  "asigna esos valores a los campos questionsCount y challengeCount. " +
							  "Usa el t�tulo propuesto y la proposal nuevos, no los del ejemplo. " +
							  "Usa g�nero neutro en las preguntas y desaf�os, siempre. " +
							  "Tono informal. " +
							  "";

		//Debug.Log("Prompt Phase2: " + promptPhase2);

		string responsePhase2 = await GetGPTResponse(promptPhase2);

		//Debug.Log("Response Phase2: " + promptPhase2);

		return responsePhase2;
	}

	public async Task<GameData> GetGameData(string prompt = null)
	{
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
		int questionsCount = gameData.questionsCount == 0? 0 : gameData.questionsCount+2; //Extra Q
		int challengesCount = gameData.challengesCount == 0? 0 : gameData.challengesCount+2; //Extra ch
		List<string> challengeTypes = gameData.challengesTypes;

		if (challengeTypes == null)
			challengeTypes = _defaultChallengeTypes;

		string challengeTypesPrompt = string.Empty;
		foreach (string type in _defaultChallengeTypes)
			challengeTypesPrompt += " " + type + ",";

		if (challengeTypesPrompt == string.Empty)
			challengeTypesPrompt = "Extrae los tipos de desaf�o de los ejemplos que hay al final del prompt.";

		//Clear saltos de linea, rompen la petici�n
		boardProposal = boardProposal.Replace("\n", "").Replace("\r", "");


		string prompt = "Responde �nicamente con un JSON siguiendo esta estructura exacta: La clase principal tiene los siguientes campos:" +
			" tittle y proposal, de tipo string que son un t�tulo corto y una descripci�n basada en los intereses proporcionados aqu�: "
			+ boardProposal + ". challenges es una lista de strings donde cada elemento es la descripci�n de un desaf�o " +
			"breve y claro, sncillos juegos psicom�gicos o no, que persigan el proposal, siempre�y �nicamente dentro de estos tipo(s) de desaf�o:"
			+ challengeTypesPrompt + ". Ejemplo de challenges: 'descripci�n breve y clara de un desaf�o corto, relevante a la proposal', sin mencionar la psicomagia. " +
			"questions es una lista de objetos, donde cada objeto tiene tres campos: " +
			"statement de tipo string que es el enunciado una pregunta dificil y formativa relacionada con la proposal. " +
			"options es una lista de cuatro strings representando las opciones de respuesta " +
			"y correctId que es un entero entre 0 y 3 indicando el �ndice de la respuesta correcta. T�pica estructura de quiz. " +
			"Ejemplos de preguntas: { statement: 'enunciado de la dificil pregunta', " +
			"options: ['opci�n la correcta', 'opci�n probable, pero incorrecta', 'opci�n  trampa', 'opci�n c�moca'], correctId: 0 }. " +
			"La respuesta correcta no es siempre la 0, SER� DISTINTA CADA VEZ!! Genera " +
			challengesCount + " challenges y " +
			questionsCount + " questions. " +
			"El tono es divertido e informal. Recuerda: debes responder �nicamente con un JSON siguiendo exactamente la estructura indicada, " +
			"sin comentarios adicionales ni explicaciones. Usa genero neutro siempre que puedas.  " +
			"Todo el contenido en castellano. En texto plano. Y NO CODE SNIPET!! " +
			"Te recuerdo que el prompt para la proposal es: "
			+ boardProposal + ", que ser� el tema de la sesi�n, en el que se basar�n los challenges y las questions.";

		//Challenge inspiration
		if (gameData.challenges.Count > 0)
		{
			prompt += " Insp�rate en estos challenges: ";
			gameData.challenges = gameData.challenges.Take(5).ToList();//5
			foreach (string challenge in gameData.challenges)
				prompt += challenge + ",";
		}

		//Questions inspiration
		if (gameData.questions.Count > 0)
		{
			prompt += " Insp�rate en estas questions: ";
			gameData.questions = gameData.questions.Take(3).ToList();//3
			foreach (QuestionData question in gameData.questions)
				prompt += JsonUtility.ToJson(question) + ",";

			prompt = prompt.Replace("\"", "");
		}

		return prompt;
	}
}

