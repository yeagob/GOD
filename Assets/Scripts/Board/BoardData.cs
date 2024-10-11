using System;
using System.Collections.Generic;
using UnityEngine;
using static Michsky.DreamOS.GameHubData;

/// <summary>
/// Representa un tablero de juego que contiene tiles y gestiona su ciclo de vida.
/// </summary>
[Serializable]
public class BoardData
{
	public string tittle;
	public string proposal;
	public string autor;
	public string imageURL;
	public int questionsCount;
	public int challengesCount;
	public TileData[] tiles;
	public List<string> challengeTypes = new List<string>();

	#region Extra questions / challenges
	// Fields for serialized lists
	[SerializeField] private List<QuestionData> _serializedQuestions;
	[SerializeField] private List<string> _serializedChallenges;

	// Internal queues for runtime use
	private Queue<QuestionData> _extraQuestions;
	private Queue<string> _extraChallenges;

	// Properties to expose the queues
	public Queue<string> ExtraChallenges
	{
		get
		{
			if (_extraChallenges == null)
				_extraChallenges = new Queue<string>(_serializedChallenges);
			return _extraChallenges;
		}
	}

	public Queue<QuestionData> ExtraQuestions
	{
		get
		{
			if (_extraQuestions == null)
				_extraQuestions = new Queue<QuestionData>(_serializedQuestions);
			return _extraQuestions;
		}
	}

	// Method to update serialized lists before saving
	public void UpdateSerializedData()
	{
		_serializedQuestions = new List<QuestionData>(_extraQuestions);
		_serializedChallenges = new List<string>(_extraChallenges);
	}
	#endregion

	#region Future
	//public int id; // PK readonly
	//public string guid; // SK readonly
	//public string url_image;
	//public DateTime last_time; // Updated or played
	//public int number_of_interactions;
	//public int next_board_id;
	#endregion

	public BoardData(string jsonData)
	{
		JsonUtility.FromJsonOverwrite(jsonData, this);
	}

	public BoardData(GameData data)
	{
		Debug.Log("DAta: " +JsonUtility.ToJson(data));	
		int[] rollAgainIds = new int[] { 2, 10, 17, 24, 29 };
		int[] travelToIds = new int[] { 1, 7, 13, 21, 25, 33 };
		int[] loseTurnIds = new int[] { 6, 18, 22, 30, 27, 34 };
		int[] bridgeIds = new int[] { 20, 35};

		// Asignar el name y proposal de GameData
		this.tittle = data.tittle;
		this.proposal = data.proposal;
		this.imageURL = data.imageURL;
		this.challengesCount = data.challengesCount;
		this.questionsCount = data.questionsCount;
		this.challengeTypes = new List<string>(data.challengesTypes);

		// Crear un array de 40 TileData
		tiles = new TileData[40];

		// La primera Tile es de tipo Start
		tiles[0] = new TileData
		{
			id = 0,
			type = TileType.Start.ToString(),
		};

		// La de tipo Die
		tiles[37] = new TileData
		{
			id = 37,
			type = TileType.Die.ToString(),
		};

		// La última Tile es de tipo End
		tiles[39] = new TileData
		{
			id = 39,
			type = TileType.End.ToString(),
		};


		// Listas de challenges y questions disponibles
		List<string> availableChallenges = new List<string>(data.challenges);
		List<QuestionData> availableQuestions = new List<QuestionData>(data.questions);

		// Mezclar las listas para asegurar aleatoriedad
		Shuffle(availableChallenges);
		Shuffle(availableQuestions);

		// Asignar las tiles fijas (Roll Again, Travel To, Lose Turn)
		for (int i = 1; i < 39; i++) // Excluyendo Start y End
		{
			if (Array.Exists(rollAgainIds, id => id == i))
			{
				tiles[i] = new TileData
				{
					id = i,
					type = TileType.RollDicesAgain.ToString()
				};
			}
			else if (Array.Exists(travelToIds, id => id == i))
			{
				tiles[i] = new TileData
				{
					id = i,
					type = TileType.TravelToTile.ToString()
				};
			}
			else if (Array.Exists(loseTurnIds, id => id == i))
			{
				tiles[i] = new TileData
				{
					id = i,
					type = TileType.LoseTurnsUntil.ToString()
				};
			}
			else if (Array.Exists(bridgeIds, id => id == i))
			{
				tiles[i] = new TileData
				{
					id = i,
					type = TileType.Bridge.ToString()
				};
			}
			else
			{
				tiles[i] = null; // Marcar las casillas que quedan como vacantes
			}
		}

		// Distribuir aleatoriamente las preguntas y desafíos
		System.Random rand = new System.Random();

		// Obtener las posiciones vacantes
		List<int> availablePositions = new List<int>();
		for (int i = 1; i < 39; i++)
		{
			if (tiles[i] == null) // Solo agregar posiciones vacías (excluyendo las especiales)
			{
				availablePositions.Add(i);
			}
		}

		// Mezclar las posiciones vacantes para aleatoriedad
		Shuffle(availablePositions);

		// Distribuir desafíos (challengesCount)
		for (int i = 0; i < data.challengesCount && availableChallenges.Count > 0 && availablePositions.Count > 0; i++)
		{
			int position = availablePositions[0];
			availablePositions.RemoveAt(0);

			string challengeDescription = availableChallenges[0];
			availableChallenges.RemoveAt(0);

			tiles[position] = new TileData
			{
				id = position,
				type = TileType.Challenge.ToString(),
				challenge = new ChallengeData
				{
					description = challengeDescription
				},
				question = null
			};
		}

		// Distribuir preguntas (questionsCount)
		for (int i = 0; i < data.questionsCount && availableQuestions.Count > 0 && availablePositions.Count > 0; i++)
		{
			int position = availablePositions[0];
			availablePositions.RemoveAt(0);

			QuestionData question = availableQuestions[0];
			availableQuestions.RemoveAt(0);

			tiles[position] = new TileData
			{
				id = position,
				type = TileType.Question.ToString(),
				question = question,
				challenge = null
			};
		}

		if (availablePositions.Count > 0)
			Debug.LogWarning("Hay casillas vacías!!");

		_serializedChallenges = new List<string>(availableChallenges);
		_serializedQuestions = new List<QuestionData>(availableQuestions);

		Debug.Log("Board Data: " + JsonUtility.ToJson(this));
	}


	// Método para mezclar listas
	private void Shuffle<T>(List<T> list)
	{
		System.Random rand = new System.Random();
		int n = list.Count;
		while (n > 1)
		{
			int k = rand.Next(n--);
			T temp = list[n];
			list[n] = list[k];
			list[k] = temp;
		}
	}
}

/// <summary>
/// Representa una tile dentro del tablero.
/// </summary>
[Serializable]
public class TileData
{
	public int id; // PK readonly
	public string type;
	public ChallengeData challenge;
	public QuestionData question;

	#region Future

	//public string url_image;
	//public byte next_tile_direction;
	#endregion
}

/// <summary>
/// Representa un challenge asociado a una tile.
/// </summary>
[Serializable]
public class ChallengeData
{
	public string description;
	//public string url_image;
	#region Future

	//public int id;
	//	public float time_in_seconds;
	#endregion
}

/// <summary>
/// Representa una pregunta asociada a una tile.
/// </summary>
[Serializable]
public class QuestionData
{
	public string statement;
	public string[] options;
	public int correctId;
}

