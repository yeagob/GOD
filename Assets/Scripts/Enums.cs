using System;

public enum GameStateState
{
	Welcome,
	Creating,
	Editing,
	Playing,
	EndGame,
}

public enum PlayerState
{
	Waiting,
	OnChallenge,
	OnQuestion,
	Playing,
	PlayAgain,
	LostTurn
}

/// <summary>
/// Defines the various effects a tile can have.
/// </summary>
public enum TileType
{
	Empty,
	Start,
	Challenge,
	TravelToTile,
	LoseTurnsUntil,
	RollDicesAgain,
	Question,
	Bridge,
	Die,
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