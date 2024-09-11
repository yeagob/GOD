using UnityEngine;

public class PlayerToken : MonoBehaviour
{
	#region Fields
	private string _name;
	private Color _color;
	private Sprite _spriteImage;
	#endregion

	#region Properties
	public string Name => _name;
	public Color Color => _color;
	public Sprite SpriteImage => _spriteImage;
	#endregion

	#region Public Methods
	public PlayerToken(string name, Color color, Sprite sprite)
	{
		_name = name;
		_color = color;
		_spriteImage = sprite;
	}
	#endregion
}
