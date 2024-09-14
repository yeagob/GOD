using DG.Tweening;
using UnityEngine;

public class PlayerToken : MonoBehaviour
{
	#region Fields

	[SerializeField] private Renderer _tokenRender;
	private string _name;
	private Color _color;
	private Tile _currentTile;

	#endregion

	#region Properties
	public string Name => _name;
	public Color Color => _color;
	public Tile CurrentTile { get => _currentTile;  }
	#endregion

	#region Public Methods

	public void Initialize(string name, Color color)
	{
		_name = name;
		_color = color;
		_tokenRender.material.color = color;
	}

	public void MoveToTile(Tile tile)
	{
		_currentTile = tile;
		transform.position = tile.transform.position;
	}

	#endregion
}
