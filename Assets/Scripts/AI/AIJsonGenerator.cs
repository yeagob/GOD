using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[Serializable]
public class GameData
{
	public string name;
	public string proposal;
	public List<string> players;
	public List<string> challenges;
	public List<QuestionData> questions;
}

[Serializable]
public class QuestionData
{
	public string statement;
	public List<string> options;
	public int correctId;
}

[Serializable]

public class PlayersBoardData
{
	public List<string> players;
	public BoardData board;
}

public class AIJsonGenerator 
{
	private const string _apiKey = "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";

	private GPT4Mini _gpt;
	private DALLE2 _dalle;

	private string _answer1;
	private string _answer2;

	private string _prompt;


	public AIJsonGenerator(string answer1, string answer2)
	{
		_answer1 = answer1;
		_answer2 = answer2;

		_prompt = CreatePrompt();

		_gpt = new GPT4Mini(_apiKey);
		_dalle = new DALLE2(_apiKey);
	}

	public async Task<PlayersBoardData> GetJsonBoardAndPlayers()
	{
		PlayersBoardData playersBoardData = new PlayersBoardData();
		string response = "";
		bool loadDefault = false;
		if (!string.IsNullOrEmpty(_answer1) || !string.IsNullOrEmpty(_answer2))
			response = await GetGPTResponse(_prompt);
		else
			loadDefault = true;

		GameData data = null;

		try
		{
			Debug.Log("Data Response:" + response);
			data = JsonUtility.FromJson<GameData>(response);
		}
		catch
		{
			//TODO: meter al menos 1 retry!!
			loadDefault = true;
		}

		if (loadDefault)
		{
			BoardData boardData = new BoardData(LoadDefaultData());
			playersBoardData.board = boardData;
		}
		else
		{
			playersBoardData.board = ProcessData(data);
			playersBoardData.players = data.players;
		}

		return playersBoardData;
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
	private string CreatePrompt()
	{
		return "Responde únicamente con un JSON siguiendo esta estructura exacta: La clase principal tiene los siguientes campos: name de tipo string, proposal de tipo string que es la descripción basada en los intereses y deseos proporcionados en: "
			+ _answer1 + ". players es una lista de strings que contiene el/los nombres de el/los jugador(es) basado en este input: "
			+ _answer2 + ". challenges es una lista de strings donde cada elemento es la descripción de un desafío breve y claro, de entre 1 y 3 minutos de duración. Ejemplos de challenges: 'descripción breve y clara de un desafío relevante a la propuesta', 'descripción breve y clara de un deafío relevante a la propuesta)'. questions es una lista de objetos, donde cada objeto tiene tres campos: statement de tipo string que es el enunciado de la pregunta relacionada con la propuesta, options que es una lista de cuatro strings representando las opciones de respuesta, y correctId que es un entero entre 0 y 3 indicando el índice de la respuesta correcta. Ejemplos de preguntas: { statement: 'enunciado de la pregunta 1', options: ['opción 1', 'opción 2', 'opción 3', 'opción 4'], correctId: índice de la respuesta correcta }. Genera 10 challenges y 10 questions siguiendo esta estructura. La proposal basada en: "
			+ _answer1 + ", será el tema de la sesión, en el que se basarán los challenges y las questions. Los challenges pueden implicar a otros jugadores(solo si los hay!), pueden ser pruebas de dibujar, actuar, hacer algo o incluso ver un video de internet. El tono es divertido e informal. Siempre se intentará buscar el crecimiento en relación a la proposal. Recuerda: debes responder únicamente con un JSON siguiendo exactamente la estructura indicada, sin comentarios adicionales ni explicaciones. Todo el contenido en castellano y en texto normal, no snipet de código!";

	}
}
