
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

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

	float _jumpPower = 2f;
	float _jumpDuration = 0.7f;

	#endregion

	#region Properties

	public BoardData BoardData => _boardData;
	public List<Tile> BoardTiles { get => _boardTiles; }

	public event Action<Player> OnPlayerEndsBoard;
	public event Action OnMoveStep;

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

	#endregion

	#region public Methods
	//TODO: dividir en métodos

	public async Task<Tile> JumptToTile(Player currentPlayer, int targetTileID)
	{
		if (_boardTiles == null || _boardTiles.Count <= targetTileID || _boardTiles[targetTileID] == null)
			return null;

		Vector3 targetPosition = _boardTiles[targetTileID].transform.position;
		Sequence tokenMoveSequence = DOTween.Sequence();

		tokenMoveSequence.Append(currentPlayer.Token.transform.DOJump(targetPosition, _jumpPower * 3, 1, _jumpDuration*2).SetEase(Ease.OutQuad));

		await tokenMoveSequence.AsyncWaitForCompletion();

		// Actualizamos la posición del token y la casilla actual del jugador
		currentPlayer.Token.MoveToTile(_boardTiles[targetTileID]);

		return _boardTiles[targetTileID];
	}

	//TODO: dividir en métodos
	public async Task<Tile> MoveToken(Player currentPlayer, int diceValue)
	{
		//PArchecillo
		if (currentPlayer.CurrentTile == null)
			currentPlayer.Token.MoveToTile(_boardTiles[0]);

		int currentTileID = currentPlayer.CurrentTile.TileData.id;
		int targetTileID = currentTileID + diceValue;

		//Duck Look At
		if(targetTileID < _boardTiles.Count)
		{
			Vector3 direction = _boardTiles[targetTileID].transform.position - _boardTiles[targetTileID-1].transform.position;
			currentPlayer.Token.transform.right = direction;
			currentPlayer.Token.transform.localRotation = Quaternion.Euler(0, 0, currentPlayer.Token.transform.rotation.eulerAngles.z);
		}


		Sequence tokenMoveSequence = DOTween.Sequence();

		// Primero, movemos el token hacia adelante hasta la casilla de meta o hasta donde pueda
		int forwardEndTileID = Mathf.Min(targetTileID, FinishTileID);

		for (int i = currentTileID + 1; i <= forwardEndTileID; i++)
		{
			Vector3 targetPosition = _boardTiles[i].transform.position;

			// Invocamos el evento OnMoveStep antes de que el token salte hacia atrás
			tokenMoveSequence.AppendCallback(() => OnMoveStep?.Invoke());

			// Agregamos cada salto hacia adelante a la secuencia
			tokenMoveSequence.Append(currentPlayer.Token.transform.DOJump(targetPosition, _jumpPower, 1, _jumpDuration).SetEase(Ease.OutQuad));
		}

		// Verificamos si el jugador se pasa de la meta + 1
		if (targetTileID > FinishTileID + 1)
		{
			int excess = targetTileID - FinishTileID;
			int bounceBackTileID = FinishTileID - excess;
			
			// Rebota: Calculamos el exceso y retrocedemos esa cantidad desde la meta + 1
			for (int i = FinishTileID; i >= bounceBackTileID; i--)
			{
				Vector3 targetPosition = _boardTiles[i].transform.position;

				// Agregamos cada salto a la secuencia
				tokenMoveSequence.Append(currentPlayer.Token.transform.DOJump(targetPosition, _jumpPower, 1, _jumpDuration).SetEase(Ease.OutQuad));
			}

			targetTileID = bounceBackTileID; // Actualizamos la casilla objetivo
		}
		else if (targetTileID == FinishTileID || targetTileID == FinishTileID + 1)
		{
			// El jugador llega exactamente a la meta o a la meta + 1
			// Emitimos el evento de fin de juego
			OnPlayerEndsBoard?.Invoke(currentPlayer);
			targetTileID = FinishTileID; // Aseguramos que el jugador esté en la casilla de meta
		}

		await tokenMoveSequence.AsyncWaitForCompletion();

		//Reset board on th middle of a movement
		if (_boardTiles == null || _boardTiles.Count == 0)
			return null;

		// Actualizamos la posición del token y la casilla actual del jugador
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
			await JumptToTile (currentPlayer, nextTileID);
		}
	}

	internal async Task TravelToBridge(Player currentPlayer)
	{
		int currentTileID = currentPlayer.CurrentTile.TileData.id;
		int nextTileID = 0;
		for (int i = currentTileID + 1; i < _boardTiles.Count; i++)
		{
			if (_boardTiles[i].TileType == TileType.Bridge && i != currentTileID)
			{
				nextTileID = i;
				break;
			}
		}

		if (nextTileID != 0)
		{
			await JumptToTile(currentPlayer, nextTileID);
		}
	}

	public void ResetBoard()
	{
		_boardData = null;
		foreach (Tile tile in _boardTiles)
			GameObject.DestroyImmediate(tile);
		Debug.Break();
		_boardTiles.Clear();
	}

	internal void RefreshChallenge(Tile currentTile)
	{
		if (_boardData.ExtraChallenges?.Count > 0)
			currentTile.TileData.challenge.description = _boardData.ExtraChallenges.Dequeue();
	}

	internal void RefreshQuestion(Tile currentTile)
	{
		if (_boardData.ExtraQuestions.Count > 0)
			currentTile.TileData.question= _boardData.ExtraQuestions.Dequeue();
	}


	#endregion

	#region Private Methods

	#endregion

}
