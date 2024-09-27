using DG.Tweening;
using System;
using UnityEngine;

public class PlayerToken : MonoBehaviour
{
	#region Fields

	[SerializeField] private Renderer _tokenRender;
	[SerializeField] private GameObject _winEffect;
	[SerializeField] private GameObject _losseEffect;
	[SerializeField] private ParticleSystem _fartSystem;
	private string _name;
	private Color _color;
	private Tile _currentTile;
	private Quaternion _initialRotation;
	private Sequence _danceSequence;

	#endregion

	#region Properties

	public string Name => _name;
	public Color Color => _color;
	public Tile CurrentTile { get => _currentTile;  }

	#endregion

	#region Unity Callbacks

	private void Start()
	{
		_initialRotation = transform.rotation;
	}

	#endregion

	#region Private Methods

	private void AnimateDuckDance()
	{
		// Reiniciar rotación a la identidad
		transform.rotation = Quaternion.identity;

		_danceSequence = DOTween.Sequence();

		// Movimiento 1: El pato inclina la cabeza hacia atrás y se mueve hacia atrás
		_danceSequence.Append(transform.DORotate(new Vector3(-30, 0, 0), 0.5f)
			.SetRelative()
			.SetEase(Ease.InOutSine));
		_danceSequence.Join(transform.DOMove(transform.position + transform.forward * -0.5f, 0.5f)
			.SetEase(Ease.InOutSine));

		// Movimiento 2: El pato inclina la cabeza hacia adelante y se mueve hacia adelante
		_danceSequence.Append(transform.DORotate(new Vector3(30, 0, 0), 0.5f)
			.SetRelative()
			.SetEase(Ease.InOutSine));
		_danceSequence.Join(transform.DOMove(transform.position + transform.forward * 0.5f, 0.5f)
			.SetEase(Ease.InOutSine));

		// Movimiento 3: El pato gira a la derecha
		_danceSequence.Append(transform.DORotate(new Vector3(0, 45, 0), 0.5f)
			.SetRelative()
			.SetEase(Ease.InOutSine));

		// Movimiento 4: El pato gira a la izquierda
		_danceSequence.Append(transform.DORotate(new Vector3(0, -90, 0), 1.0f)
			.SetRelative()
			.SetEase(Ease.InOutSine));

		// Movimiento 5: El pato vuelve al centro
		_danceSequence.Append(transform.DORotate(new Vector3(0, 45, 0), 0.5f)
			.SetRelative()
			.SetEase(Ease.InOutSine));

		// Bucle infinito sin pausas
		_danceSequence.SetLoops(-1, LoopType.Restart);
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
	
	}

	public void ResetState()
	{
		_danceSequence.Pause();
		_danceSequence.Kill();
		_danceSequence = null;
		transform.DOKill();
		_winEffect.SetActive(false);
		_losseEffect.SetActive(false);
		transform.rotation = _initialRotation;
	}

	//TODO ESTO DEBERÍA IR EN PLAYER, NO AQUI!
	public void Win()
	{
		_winEffect.SetActive(true);
		AnimateDuckDance();
	}

	public void Loose()
	{
		_losseEffect.SetActive(true);
	}

	public void Fart()
	{
		_fartSystem.Emit(1);
	}
	#endregion

}
