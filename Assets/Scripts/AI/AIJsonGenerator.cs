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
	public List<string> challenges;
	public List<QuestionData> questions;
}


public class AIJsonGenerator 
{
	private const string _apiKey = "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";

	private GPT4Mini _gpt;
	private DALLE2 _dalle;

	private string _answer1;
	private string _prompt;


	public AIJsonGenerator(string answer1)
	{
		_answer1 = answer1;

		_prompt = CreatePrompt();

		_gpt = new GPT4Mini(_apiKey);
		_dalle = new DALLE2(_apiKey);
	}

	public async Task<BoardData> GetJsonBoard()
	{
		bool loadDefault = false;

		BoardData boardData;

		string response = "";
		if (!string.IsNullOrEmpty(_answer1))
			response = await GetGPTResponse(_prompt);

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
			boardData = new BoardData(LoadDefaultData());
		else
			boardData = ProcessData(data);

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

	private string CreatePrompt()
	{
		return "Responde únicamente con un JSON siguiendo esta estructura exacta: La clase principal tiene los siguientes campos: name y proposal, de tipo string que son un título y una descripción basada en los intereses proporcionados aquí: "
			+ _answer1 + ". challenges es una lista de strings donde cada elemento es la descripción de un desafío breve y claro, pequeños juegos psicomágicos, que persigan el proposal. Ejemplo de challenges: 'descripción breve y clara de un desafío relevante a la propuesta', sin mencionar la psicomagia. questions es una lista de objetos, donde cada objeto tiene tres campos: statement de tipo string que es el enunciado de la pregunta relacionada con la propuesta, options que es una lista de cuatro strings representando las opciones de respuesta, y correctId que es un entero entre 0 y 3 indicando el índice de la respuesta correcta. Ejemplos de preguntas: { statement: 'enunciado de la dificil pregunta', options: ['opción la correcta', 'opción probable pero incorrecta', 'opción  trampa', 'opción correcta en otro contexto'], correctId: 0 }. Genera 10 challenges y 10s questions muy dificiles, la respuesta correcta no es siempre la 0, será distinta cada vez. La proposal basada en: "
			+ _answer1 + ", que será el tema de la sesión, en el que se basarán los challenges y las questions( que son de tipo quiz, pensadas para APRENDER en profundidad sobre el proposal!). Los challenges pueden ser pruebas de escribir, de dibujar, de hacer algo o incluso usar el movil. PERO CON EL ESPÍRITU PSICOMÁGICO. El tono es divertido e informal. Siempre se intentará buscar el crecimiento en relación a la proposal. Recuerda: debes responder únicamente con un JSON siguiendo exactamente la estructura indicada, sin comentarios adicionales ni explicaciones. Usa genero con @(ej: jugador@s) siempre que puedas.  Todo el contenido en castellano. Y en texto normal, NO en snipet de código!!";
	}
}
