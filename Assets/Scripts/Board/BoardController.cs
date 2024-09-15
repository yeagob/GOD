
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Manages the board by controlling its tiles and challenges.
/// </summary>
[Serializable]
public class BoardController
{
	#region Fields

	[SerializeField, ReadOnly] private BoardData _boardData;
	private List<Tile> _boardTiles = new List<Tile>();

	#endregion

	#region Properties

	public BoardData BoardData => _boardData;
	public List<Tile> BoardTiles { get => _boardTiles; }

	#endregion

	#region Constructors

	public BoardController(BoardData boardData, GameOfDuckBoardCreator boardCreator)
	{
		_boardData = boardData;
		_boardTiles = boardCreator.GetBoard(_boardData);
	}

	public BoardController(string jsonData, GameOfDuckBoardCreator boardCreator)
	{
		// Deserialize JSON and construct Board
		_boardData = new BoardData(jsonData);
		_boardTiles = boardCreator.GetBoard(_boardData);
	}

	public async Task<Tile> MoveToken(Player currentPlayer, int diceValue)
	{
		int currentTileID = currentPlayer.CurrentTile.TileData.id;
		int targetTileID = currentTileID + diceValue;

		float jumpPower = 2f;
		float jumpDuration = 0.7f;      

		Sequence tokenMoveSequence = DOTween.Sequence();

		for (int i = currentTileID+1; i <= targetTileID; i++)
		{
			Vector3 targetPosition = _boardTiles[i].transform.position;

			// Add each jump to the sequence
			tokenMoveSequence.Append(currentPlayer.Token.transform.DOJump(targetPosition, jumpPower, 1, jumpDuration).SetEase(Ease.OutQuad));
		}

		await tokenMoveSequence.AsyncWaitForCompletion();

		currentPlayer.Token.MoveToTile(_boardTiles[targetTileID]);

		return _boardTiles[targetTileID];
	}

	internal async Task TravelToNextTravelTile(Player currentPlayer)
	{
		int currentTileID = currentPlayer.CurrentTile.TileData.id;
		int nextTileID = 0;
		for (int i = currentTileID + 1; i <= _boardTiles.Count; i++)
		{
			if (_boardTiles[i].TileType == TileType.TravelToTile)
			{
				nextTileID = i;
				break;
			}
		}

		if (nextTileID != 0)
		{
			int value = nextTileID - currentTileID;
			await MoveToken(currentPlayer, value);
		}
	}
	#endregion

	#region Private Methods
	#endregion
}
