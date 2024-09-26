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
		return "Responde �nicamente con un JSON siguiendo esta estructura exacta: La clase principal tiene los siguientes campos: name y proposal, de tipo string que son un t�tulo y una descripci�n basada en los intereses proporcionados aqu�: "
			+ _answer1 + ". challenges es una lista de strings donde cada elemento es la descripci�n de un desaf�o breve y claro, peque�os juegos psicom�gicos, que persigan el proposal. Ejemplo de challenges: 'descripci�n breve y clara de un desaf�o relevante a la propuesta', sin mencionar la psicomagia. questions es una lista de objetos, donde cada objeto tiene tres campos: statement de tipo string que es el enunciado de la pregunta relacionada con la propuesta, options que es una lista de cuatro strings representando las opciones de respuesta, y correctId que es un entero entre 0 y 3 indicando el �ndice de la respuesta correcta. Ejemplos de preguntas: { statement: 'enunciado de la dificil pregunta', options: ['opci�n la correcta', 'opci�n probable pero incorrecta', 'opci�n  trampa', 'opci�n correcta en otro contexto'], correctId: 0 }. Genera 10 challenges y 10s questions muy dificiles, la respuesta correcta no es siempre la 0, ser� distinta cada vez. La proposal basada en: "
			+ _answer1 + ", que ser� el tema de la sesi�n, en el que se basar�n los challenges y las questions( que son de tipo quiz, pensadas para APRENDER en profundidad sobre el proposal!). Los challenges pueden ser pruebas de escribir, de dibujar, de hacer algo o incluso usar el movil. PERO CON EL ESP�RITU PSICOM�GICO. El tono es divertido e informal. Siempre se intentar� buscar el crecimiento en relaci�n a la proposal. Recuerda: debes responder �nicamente con un JSON siguiendo exactamente la estructura indicada, sin comentarios adicionales ni explicaciones. Usa genero con @(ej: jugador@s) siempre que puedas.  Todo el contenido en castellano. Y en texto normal, NO en snipet de c�digo!!";
	}
}
