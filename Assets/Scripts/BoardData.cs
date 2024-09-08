using System;
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

