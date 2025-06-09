using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;

public class GameOfDuckBoardCreator : MonoBehaviour
{
	#region Fields
	[SerializeField] private Tile _tilePrefab;
	[SerializeField] private LineRenderer _lineRenderer;

	[Title("OpenAI Configuration")]
	[SerializeField]
	[InfoBox("Optional: OpenAI Configuration for AI-powered board generation. Leave empty to disable AI features.")]
	private OpenAIConfigurationData _openAIConfig;

	private const float A4_WIDTH = 29.7f;
	private const float A4_HEIGHT = 21.0f;

	[SerializeField, ReadOnly] private int numberOfCells = 40;
	[SerializeField, ReadOnly] private float margin = 1f;

	private List<Rect> _cellRects = new List<Rect>();
	private List<Tile> _boardTiles = new List<Tile>();
	#endregion

	#region Properties
	public OpenAIConfigurationData OpenAIConfig 
	{ 
		get => _openAIConfig; 
		set => _openAIConfig = value; 
	}

	public bool HasOpenAIConfig => _openAIConfig != null && _openAIConfig.HasApiKey;
	#endregion

	#region Public Methods
	public List<Tile> GetBoard(BoardData data)
	{
		GenerateBoard(numberOfCells);
		InstantiateBoard(data);
		return _boardTiles;
	}

	public string GetOpenAIApiKey()
	{
		if (_openAIConfig == null || !_openAIConfig.HasApiKey)
		{
			return string.Empty;
		}

		return _openAIConfig.ApiKey;
	}
	#endregion

	#region Private Methods
	private void GenerateBoard(int numCells)
	{
		_cellRects.Clear();

		float boardWidth = A4_WIDTH - 2 * margin;
		float boardHeight = A4_HEIGHT - 2 * margin;

		float currentX = margin;
		float currentY = margin;

		float stepX = boardWidth / Mathf.Sqrt(numCells);
		float stepY = boardHeight / Mathf.Sqrt(numCells);

		float leftBound = margin;
		float rightBound = A4_WIDTH - margin;
		float topBound = margin;
		float bottomBound = A4_HEIGHT - margin;

		int direction = 0;

		for (int i = 0; i < numCells; i++)
		{
			_cellRects.Add(new Rect(currentX, currentY, stepX, stepY));

			switch (direction)
			{
				case 0:
					currentX += stepX;
					if (currentX + stepX >= rightBound)
					{
						direction = 1;
						topBound += stepY;
					}
					break;
				case 1:
					currentY += stepY;
					if (currentY + stepY >= bottomBound)
					{
						direction = 2;
						rightBound -= stepX;
					}
					break;
				case 2:
					currentX -= stepX;
					if (currentX - stepX <= leftBound)
					{
						direction = 3;
						bottomBound -= stepY;
					}
					break;
				case 3:
					currentY -= stepY;
					if (currentY - stepY <= topBound)
					{
						direction = 0;
						leftBound += stepX;
					}
					break;
			}
		}
	}

	private void InstantiateBoard(BoardData data)
	{
		if (_cellRects == null || _cellRects.Count == 0) return;

		_lineRenderer.positionCount = _cellRects.Count;

		for (int i = 0; i < _cellRects.Count; i++)
		{
			var rect = _cellRects[i];

			Tile tile = Instantiate(_tilePrefab, rect.position, Quaternion.identity);
			tile.Initialize(i);
			_boardTiles.Add(tile);

			_lineRenderer.SetPosition(i, tile.transform.position);
			
			if (i < data.tiles.Length)
			{
				tile.SetTileData(data.tiles[i]);
			}
		}
	}
	#endregion
}