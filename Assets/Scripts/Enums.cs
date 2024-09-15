using System;

public enum PlayerState
{
	OnChallenge,
	OnQuestion,
	Waiting,
	Playing,
	LostTurn
}

/// <summary>
/// Defines the various effects a tile can have.
/// </summary>
public enum TileType
{
	Start,
	Challenge,
	TravelToTile,
	LoseTurnsUntil,
	RollDicesAgain,
	Question,
	End
}

public class EnumConverter
{
	public static TileType StringToTileType(string input)
	{
		if (Enum.TryParse(input, true, out TileType result))
		{
			return result;
		}
		else
		{
			throw new ArgumentException("Invalid input for TileType enum.");
		}
	}
}