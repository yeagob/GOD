using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public abstract class OpenAIBase
{
	public static Dictionary<int, string> TilesUrls = new Dictionary<int, string>();

	protected const string API_URL = "https://api.openai.com/v1/";
	protected string _apiKey ;

	protected OpenAIBase(string apiKey)
	{
		this._apiKey = apiKey;
	}

	[System.Serializable]
	public class DALLEResponse
	{
		public Data[] Data;
	}

	[System.Serializable]
	public class Data
	{
		public string Url;
	}
	protected async Task<string> SendRequest(string endpoint, string jsonData)
	{
		using (UnityWebRequest request = new UnityWebRequest(API_URL + endpoint, "POST"))
		{
			//Debug.Log("Request: " + API_URL + endpoint);
			Debug.Log("Json: " + jsonData);

			byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
			request.uploadHandler = new UploadHandlerRaw(bodyRaw);
			request.downloadHandler = new DownloadHandlerBuffer();
			request.SetRequestHeader("Content-Type", "application/json");
			request.SetRequestHeader("Authorization", $"Bearer {_apiKey}");

			var operation = request.SendWebRequest();

			while (!operation.isDone)
				await Task.Yield();

			if (request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError($"Error: {request.error}");
				return null;
			}

			return request.downloadHandler.text;
		}
	}
}

[Serializable]
public class DataResponse
{
	public string id;
	public long created;
	public string model;
	public List<Choice> choices;

}

[Serializable]
public class Choice
{
	public int index;
	public Message message;

}

[Serializable]
public class Message
{
	public string role;
	public string content;
}


public class GPT4Mini : OpenAIBase
	{
		public GPT4Mini(string apiKey) : base(apiKey) { }
	public async Task<string> GetCompletion(string prompt)
	{
		string jsonData = "{\"model\": \"gpt-4o-mini\", \"messages\": [{\"role\": \"user\", \"content\": \""+prompt+"\"}, {\"role\":\"system\",\"content\": \"Eres un sistema de generación de estructuras json.\"}]}";
		string response = await SendRequest("chat/completions", jsonData);
		DataResponse dataResponse = JsonUtility.FromJson<DataResponse>(response);
			return dataResponse.choices[0].message.content;
		}
	}


	public class DALLE2 : OpenAIBase
	{
		public DALLE2(string apiKey) : base(apiKey) { }

		public async Task<Sprite> GenerateImage(string prompt, int tileID)
		{
			string jsonData = $"{{\"prompt\": \"{prompt}\", \"n\": 1, \"size\": \"1024x1024\"}}";
			string response = await SendRequest("images/generations", jsonData);

			// Procesar la respuesta JSON y extraer la URL de la imagen generada
			DALLEResponse dalleResponse = JsonUtility.FromJson<DALLEResponse>(response);

			if (dalleResponse == null || dalleResponse.Data == null || dalleResponse.Data.Length == 0)
			{
				Debug.LogError("Failed to parse DALL-E response or no image data received.");
				return null;
			}

			string imageUrl = dalleResponse.Data[0].Url;
		
			TilesUrls.Add(tileID, imageUrl);

			// Descargar la imagen y convertirla en un Sprite
			return await DownloadSprite(imageUrl);
		}

		public static async Task<Sprite> DownloadSprite(string url)
		{
			using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
			{
				var operation = request.SendWebRequest();

				while (!operation.isDone)
				{
					await Task.Yield();
				}

				if (request.result != UnityWebRequest.Result.Success)
				{
					Debug.LogError($"Error al descargar la imagen: {request.error}");
					return null;
				}

				Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;

				// Crear un Sprite a partir de la textura descargada
				Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
				return sprite;
			}
		}
	}
