
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
	const int FinishTileID = 39;
	#region Fields

	[SerializeField, ReadOnly] private BoardData _boardData;
	private List<Tile> _boardTiles = new List<Tile>();

	#endregion

	#region Properties

	public BoardData BoardData => _boardData;
	public List<Tile> BoardTiles { get => _boardTiles; }

	public event Action<Player> OnPlayerEndsBoard;

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

		// Primero, movemos el token hacia adelante hasta la casilla de meta o hasta donde pueda
		int forwardEndTileID = Mathf.Min(targetTileID, FinishTileID);

		for (int i = currentTileID + 1; i <= forwardEndTileID; i++)
		{
			Vector3 targetPosition = _boardTiles[i].transform.position;

			// Agregamos cada salto hacia adelante a la secuencia
			tokenMoveSequence.Append(currentPlayer.Token.transform.DOJump(targetPosition, jumpPower, 1, jumpDuration).SetEase(Ease.OutQuad));
		}

		// Verificamos si el jugador se pasa de la meta + 1
		if (targetTileID > FinishTileID + 1)
		{
			// Rebota: Calculamos el exceso y retrocedemos esa cantidad desde la meta + 1
			int excess = targetTileID - (FinishTileID + 1);
			int bounceBackTileID = (FinishTileID + 1) - excess;

			// Movemos el token hacia atr�s desde la meta hasta la casilla correcta
			for (int i = FinishTileID; i >= bounceBackTileID; i--)
			{
				Vector3 targetPosition = _boardTiles[i].transform.position;

				// Agregamos cada salto hacia atr�s a la secuencia
				tokenMoveSequence.Append(currentPlayer.Token.transform.DOJump(targetPosition, jumpPower, 1, jumpDuration).SetEase(Ease.OutQuad));
			}

			targetTileID = bounceBackTileID; // Actualizamos la casilla objetivo
		}
		else if (targetTileID == FinishTileID || targetTileID == FinishTileID + 1)
		{
			// El jugador llega exactamente a la meta o a la meta + 1
			// Emitimos el evento de fin de juego
			OnPlayerEndsBoard?.Invoke(currentPlayer);
			targetTileID = FinishTileID; // Aseguramos que el jugador est� en la casilla de meta
		}

		await tokenMoveSequence.AsyncWaitForCompletion();

		// Actualizamos la posici�n del token y la casilla actual del jugador
		currentPlayer.Token.MoveToTile(_boardTiles[targetTileID]);

		return _boardTiles[targetTileID];
	}

	public async Task TravelToNextTravelTile(Player currentPlayer)
	{
		int currentTileID = currentPlayer.CurrentTile.TileData.id;
		int nextTileID = 0;
		for (int i = currentTileID + 1; i < _boardTiles.Count; i++)
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
