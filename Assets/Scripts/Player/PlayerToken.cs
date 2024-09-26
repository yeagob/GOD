using DG.Tweening;
using System;
using UnityEngine;

public class PlayerToken : MonoBehaviour
{
	#region Fields

	[SerializeField] private Renderer _tokenRender;
	[SerializeField] private GameObject _winEffect;
	[SerializeField] private GameObject _losseEffect;
	private string _name;
	private Color _color;
	private Tile _currentTile;

	#endregion

	#region Properties
	public string Name => _name;
	public Color Color => _color;
	public Tile CurrentTile { get => _currentTile;  }
	
	#endregion

	#region Private Methods

	private void ResetDuckState()
	{
		DOTween.Kill(transform);
		HideEffects();
	}

	private void HideEffects()
	{
		_winEffect.SetActive(false);
		_losseEffect.SetActive(false);
	}

	private void AnimateDuckDance()
	{
		// Sequence to chain multiple Tweens
		Sequence danceSequence = DOTween.Sequence();

		// Rotation: Duck bobs head back (up in world, back relative to the duck)
		danceSequence.Append(transform.DORotate(new Vector3(-30, 0, 0), 0.5f).SetEase(Ease.InOutQuad)); // Backwards rotation
		danceSequence.Append(transform.DORotate(new Vector3(30, 0, 0), 0.5f).SetEase(Ease.InOutQuad));  // Forward rotation

		// Translation: Duck moves forward and back in the defined axis
		danceSequence.Append(transform.DOMove(transform.position + transform.forward * -0.5f, 0.5f).SetEase(Ease.InOutQuad)); // Moves back (relative forward)
		danceSequence.Append(transform.DOMove(transform.position + transform.forward * 0.5f, 0.5f).SetEase(Ease.InOutQuad));  // Moves forward (relative back)

		// Rotation around Y axis for some twist
		danceSequence.Append(transform.DORotate(new Vector3(0, 45, 0), 0.5f).SetEase(Ease.InOutQuad));  // Twist right
		danceSequence.Append(transform.DORotate(new Vector3(0, -45, 0), 0.5f).SetEase(Ease.InOutQuad)); // Twist left

		// Loop the sequence
		danceSequence.SetLoops(-1, LoopType.Yoyo);
	}

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

		ResetDuckState();		
	}


	//TODO ESTO DEBERÍA IR EN PLAYER, NO AQUI!
	public void Win()
	{
		_winEffect.SetActive(true);
		AnimateDuckDance();
	}

	public void Loose()
	{
		_losseEffect.SetActive(false);
	}

	#endregion

}
