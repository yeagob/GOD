using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;  

public class GameOfDuckBoardCreator : MonoBehaviour
{
	[SerializeField] private Tile _tilePrefab;

	//BOARD DIMENSIONS
	private const float A4_WIDTH = 29.7f;
	private const float A4_HEIGHT = 21.0f;

	[SerializeField, ReadOnly] private int numberOfCells = 40;
	[SerializeField, ReadOnly] private float margin = 0.5f;  

	private List<Rect> _cellRects = new List<Rect>();
	private List<Tile> _boardTiles = new List<Tile>();

	public List<Tile>  GetBoard(BoardData data)
	{
		GenerateBoard(numberOfCells);
		InstantiateBoard(data);
		return _boardTiles;
	}

	private void GenerateBoard(int numCells)
	{
		_cellRects.Clear();  // Clear previous cells

		float boardWidth = A4_WIDTH - 2 * margin;
		float boardHeight = A4_HEIGHT - 2 * margin;

		// Start from top-left of the margin area
		float currentX = margin;
		float currentY = margin;

		// Steps for the initial full width and height
		float stepX = boardWidth / Mathf.Sqrt(numCells);
		float stepY = boardHeight / Mathf.Sqrt(numCells);

		// Boundaries that will shrink as we spiral inward
		float leftBound = margin;
		float rightBound = A4_WIDTH - margin;
		float topBound = margin;
		float bottomBound = A4_HEIGHT - margin;

		int direction = 0;  // 0 = right, 1 = down, 2 = left, 3 = up

		for (int i = 0; i < numCells; i++)
		{
			// Add current cell position and size
			_cellRects.Add(new Rect(currentX, currentY, stepX, stepY));

			// Move based on current direction
			switch (direction)
			{
				case 0:  // Right
					currentX += stepX;
					if (currentX + stepX >= rightBound)  // If we hit right boundary, change direction
					{
						direction = 1;  // Change to down
						topBound += stepY;  // Shrink the top boundary
					}
					break;
				case 1:  // Down
					currentY += stepY;
					if (currentY + stepY >= bottomBound)  // If we hit bottom boundary, change direction
					{
						direction = 2;  // Change to left
						rightBound -= stepX;  // Shrink the right boundary
					}
					break;
				case 2:  // Left
					currentX -= stepX;
					if (currentX - stepX <= leftBound)  // If we hit left boundary, change direction
					{
						direction = 3;  // Change to up
						bottomBound -= stepY;  // Shrink the bottom boundary
					}
					break;
				case 3:  // Up
					currentY -= stepY;
					if (currentY - stepY <= topBound)  // If we hit top boundary, change direction
					{
						direction = 0;  // Change to right
						leftBound += stepX;  // Shrink the left boundary
					}
					break;
			}
		}
	}

	private void InstantiateBoard(BoardData data)
	{
		if (_cellRects == null || _cellRects.Count == 0) return;


		for (int i = 0; i < _cellRects.Count; i++)
		{
			var rect = _cellRects[i];

			Tile tile = Instantiate(_tilePrefab, rect.position, Quaternion.identity);
			tile.Initialize(i);
			_boardTiles.Add(tile);
			if (i < data.tiles.Length)
			{
				tile.SetType(data.tiles[i]);
			}
		}
	}
}
