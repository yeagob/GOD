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
	public string name;
	public string proposal;
	public string autor;
	public string imageURL;
	public TileData[] tiles;

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
		int[] rollAgainIds = new int[] { 2, 10, 17, 24, 29 };
		int[] travelToIds = new int[] { 1, 7, 13, 21, 25, 33 };
		int[] loseTurnIds = new int[] { 6, 18, 22, 30, 27, 34};

		// Asignar el name y proposal de GameData
		this.name = data.name;
		this.proposal = data.proposal;

		// Crear un array de 40 TileData
		tiles = new TileData[40];

		// La primera Tile es de tipo Start
		tiles[0] = new TileData
		{
			id = 0,
			type = TileType.Start.ToString(),
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

		// Asignar las tiles fijas y las restantes
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
			else
			{
				// Asignar aleatoriamente entre Question y Challenge, 50% cada uno
				if (availableChallenges.Count == 0 && availableQuestions.Count == 0)
				{
					string challengeDescription = data.challenges[UnityEngine.Random.Range(0, data.challenges.Count)];
					// Si no quedan challenges ni questions disponibles. Asignamos un challenge random extra.
					tiles[i] = new TileData
					{
						id = i,
						type = TileType.Challenge.ToString(),
						challenge = new ChallengeData
						{
							description = challengeDescription,
							//url_image = "" // Asignar URL si es necesario
						},
						question = null
					};				
				}
				else if (availableChallenges.Count == 0)
				{
					// Solo quedan questions
					AssignQuestionTile(i, availableQuestions);
				}
				else if (availableQuestions.Count == 0)
				{
					// Solo quedan challenges
					AssignChallengeTile(i, availableChallenges);
				}
				else
				{
					// Ambos disponibles
					System.Random rand = new System.Random();
					if (rand.Next(100) < 50)
					{
						AssignChallengeTile(i, availableChallenges);
					}
					else
					{
						AssignQuestionTile(i, availableQuestions);
					}
				}
			}
		}

		// La última 37 es de tipo Die
		tiles[37] = new TileData
		{
			id = 37,
			type = TileType.Die.ToString(),
		};

		Debug.Log("Board Data: " + JsonUtility.ToJson(this));
	}

	private void AssignChallengeTile(int index, List<string> availableChallenges)
	{
		// Seleccionar un challenge de la lista y asignarlo
		string challengeDescription = availableChallenges[0];
		availableChallenges.RemoveAt(0); // Remover el challenge usado

		tiles[index] = new TileData
		{
			id = index,
			type = TileType.Challenge.ToString(),
			challenge = new ChallengeData
			{
				description = challengeDescription,
				//url_image = "" // Asignar URL si es necesario
			},
			question = null
		};
	}

	private void AssignQuestionTile(int index, List<QuestionData> availableQuestions)
	{
		// Seleccionar una question de la lista y asignarla
		QuestionData question = availableQuestions[0];
		availableQuestions.RemoveAt(0); // Remover la question usada

		tiles[index] = new TileData
		{
			id = index,
			type = TileType.Question.ToString(),
			question = question,
			challenge = null
		};
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
	//	public float time_in_minutes;
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

