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
		string response = await GetGPTResponse(_prompt);
		bool error = false;
		GameData data = null;

		try
		{
			Debug.Log("Data Response:" + response);
			data = JsonUtility.FromJson<GameData>(response);
		}
		catch
		{
			error = true;
		}

		if (error)
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
		//"Responderás únicamente con un json, nada más. DAme una estructura json con una lista de nombres";
		return
		"Responderás únicamente con un json, nada más. La estructura del json es la siguiente:\n\n" +
		"\n" +
		"    string name;\n" +
		"    string proposal;\n" +
		"    List<string> players;\n" +
		"    List<string> challenges;\n" +
		"\n\n" +
		"Inventa el contenido del campo name y define una proposal basadas en estos intereses y deseos:\n" +
		_answer1 + "\n" +
		"Teniendo en cuenta que el/los participantes de este juego es/son:\n" +
		_answer2 + "\n" +
		"Responderás únicamente con un json, nada más. Sigue la estructura anterior. Genera 15 desafíos sencillos, que impliquen un tiempo entre 1 y 3 min, relacionados con la 'proposal'. Siempre piensa en ayudar a las personas con desafíos adaptados a los participantes y a la proposal, sencillos, divertidos y originales.\n" +
		"Intentarás rellenar la lista de players en base a esta información:\n" +
		_answer2 + "\n\n" +
		"Responderás únicamente con el json, nada más. Añade todas las comillas necesarias en el json, todo son strings.";
	}
}
