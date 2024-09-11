using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a game board containing tiles and managing its lifecycle.
/// </summary>
[Serializable]
public class BoardData
{
	public string name;
	public string proposal;
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
			challenge = null // No challenge for Start
		};

		// La última Tile es de tipo End
		tiles[39] = new TileData
		{
			id = 39,
			type = TileType.End.ToString(),
			challenge = null // No challenge for End
		};

		// Resto de Tiles aleatorias entre los otros tipos
		System.Random rand = new System.Random();
		List<string> availableChallenges = new List<string>(data.challenges);
		int challengeCount = 0;

		for (int i = 1; i < 39; i++) // Excluyendo Start y End
		{
			// Determinar si esta tile será de tipo Challenge (25% de probabilidades)
			bool isChallenge = rand.Next(100) < 25 && availableChallenges.Count > 0;

			if (isChallenge)
			{
				// Seleccionar un challenge de la lista y asignarlo
				string challengeDescription = availableChallenges[0];
				availableChallenges.RemoveAt(0); // Remover el challenge usado

				tiles[i] = new TileData
				{
					id = i,
					type = TileType.Challenge.ToString(),
					challenge = new ChallengeData
					{
						description = challengeDescription,
						url_image = "" // Asignar URL si es necesario
					}
				};

				challengeCount++;
			}
			else
			{
				// Tile aleatoria entre los otros tipos (sin contar Start y End)
				TileType randomType = GetRandomTileType();
				tiles[i] = new TileData
				{
					id = i,
					type = randomType.ToString(),
				};
			}
		}
	}

	private TileType GetRandomTileType()
	{
		// Excluyendo Start, End, y Challenge de las opciones aleatorias
		TileType[] tileTypes = new TileType[]
		{
			TileType.TravelToTile,
			TileType.LoseTurnsUntil,
			TileType.RollDicesAgain,
			TileType.Question
		};

		System.Random rand = new System.Random();
		return tileTypes[rand.Next(tileTypes.Length)];
	}
}

/// <summary>
/// Represents a tile within the board.
/// </summary>
[Serializable]
public class TileData
{
	public int id; // PK readonly
	public string type;
	public ChallengeData challenge;
	#region Future

	//public string url_image;
	//public byte next_tile_direction;
	#endregion


}

/// <summary>
/// Represents a challenge associated with a tile.
/// </summary>
[Serializable]
public class ChallengeData
{
	public string description;
	public string url_image;
	#region Future

	//public int id;
	//	public float time_in_minutes;
	#endregion
}

