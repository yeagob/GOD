using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public abstract class OpenAIBase
{
	public static Dictionary<string, string> ImagesTagsUrls = new Dictionary<string, string>();
	public static string LastImageUrl;

	protected const string API_URL = "https://api.openai.com/v1/";
	protected string _apiKey;

	protected OpenAIBase(string apiKey)
	{
		this._apiKey = apiKey;
	}

	[System.Serializable]
	public class DALLEResponse
	{
		public Data[] data;
	}

	[System.Serializable]
	public class Data
	{
		public string url;
	}
	protected async Task<string> SendRequest(string endpoint, string jsonData)
	{
		using (UnityWebRequest request = new UnityWebRequest(API_URL + endpoint, "POST"))
		{
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


			Debug.Log("Response: " + request.downloadHandler.text);

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

	[System.Serializable]
	public class Message
	{
		public string role;
		public string content;
	}

	[System.Serializable]
	public class OpenAIRequest
	{
		public string model;
		public float temperature;
		public List<Message> messages;
	}

	public async Task<string> GetCompletion(string prompt)
	{
		List<Message> messages = new List<Message>
			{
				new Message
				{
					role = "system",
					content = "Eres un módulo de un juego llamado Game Of Duck, inspirado en el Juego de la Oca, " +
					"pero con preguntas y desafíos personalizables, que se encarga de analizar y generar tableros de juego."
				},
				new Message
				{
					role = "user",
					content = prompt
				}
			};


		OpenAIRequest requestBody = new OpenAIRequest
		{
			model = "gpt-4o-mini",
			temperature = 0.2f,
			messages = messages
		};

		string jsonData = JsonUtility.ToJson(requestBody);


		//string jsonData = "{\"model\": \"gpt-4o-mini\", \"temperature\": 0.2, \"messages\": [{\"role\":\"system\",\"content\": " +
		//	"				\"Eres un módulo de un juego llamado Game Of Duck, " +
		//					"inspirado en el Juego de la Oca, pero con preguntas y desafíos personalizables," +
		//					" que se encarga de analizar y generar tableros de juego.\"}, " +
		//					"{\"role\": \"user\", \"content\": \"" +
		//					prompt + "\"} ]}";

		string response = await SendRequest("chat/completions", jsonData);

		DataResponse dataResponse = JsonUtility.FromJson<DataResponse>(response);

		if (dataResponse == null)
			return null;

		return dataResponse.choices[0].message.content;
	}
}


public class DALLE2 : OpenAIBase
{
	private const string _apiKey = "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";

	public DALLE2(string apiKey) : base(apiKey) { }
	public DALLE2() : base(_apiKey) { }

	public async Task<Sprite> GenerateImage(string prompt, string imageID = "default")
	{
		string jsonData = $"{{\"prompt\": \"{prompt}\", \"n\": 1, \"size\": \"512x512\"}}";////267x322
		string response = await SendRequest("images/generations", jsonData);

		// Procesar la respuesta JSON y extraer la URL de la imagen generada
		DALLEResponse dalleResponse = JsonUtility.FromJson<DALLEResponse>(response);

		if (dalleResponse == null || dalleResponse.data == null || dalleResponse.data.Length == 0)
		{
			Debug.LogError("Failed to parse DALL-E response or no image data received.");
			return null;
		}

		LastImageUrl = dalleResponse.data[0].url;

		if (ImagesTagsUrls.ContainsKey(imageID))
			ImagesTagsUrls[imageID] = LastImageUrl;
		else
			ImagesTagsUrls.Add(imageID, LastImageUrl);

		// Descargar la imagen y convertirla en un Sprite
		return await DownloadSprite(LastImageUrl);
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

	/// <summary>
	/// Loads an image asynchronously from StreamingAssets and returns it as a Sprite.
	/// </summary>
	public static async Task<Sprite> LoadSpriteFromStreamigAssets(string fileName)
	{
		string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

		// For WebGL, we need to access the file via UnityWebRequest
		if (filePath.Contains("://") || filePath.Contains(":///"))
		{
			using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(filePath))
			{
				var request = www.SendWebRequest();

				// Wait until the request is done
				while (!request.isDone)
				{
					await Task.Yield();
				}

				if (www.result != UnityWebRequest.Result.Success)
				{
					Debug.LogError("Error loading image: " + www.error);
					return null;
				}
				else
				{
					Texture2D texture = DownloadHandlerTexture.GetContent(www);
					return ConvertToSprite(texture);
				}
			}
		}
		else
		{
			// For non-WebGL platforms, we can read directly from the file system
			if (File.Exists(filePath))
			{
				byte[] imageData = await Task.Run(() => File.ReadAllBytes(filePath));
				Texture2D texture = new Texture2D(2, 2);
				texture.LoadImage(imageData); // Load the image into the texture
				return ConvertToSprite(texture);
			}
			else
			{
				Debug.LogError("File not found: " + filePath);
				return null;
			}
		}
	}

	/// <summary>
	/// Converts a Texture2D to a Sprite.
	/// </summary>
	public static Sprite ConvertToSprite(Texture2D texture)
	{
		return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
	}
}
