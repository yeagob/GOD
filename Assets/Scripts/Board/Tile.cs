using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Threading.Tasks;

public class Tile : MonoBehaviour
{
	[SerializeField, ReadOnly] private TileType _tileType;
	[SerializeField] private TextMeshProUGUI _tileDescriptionText;
	[SerializeField] private TextMeshProUGUI _tileNumberText;
	[SerializeField] private Image _tileImage;
	[SerializeField] private Button _tileButton;
	
	private TileData _tileData;

	// Images
	[Header("Start Tiles")]
	[SerializeField] private Sprite[] _startSpriteTiles;
	[Header("Lost Turn")]
	[SerializeField] private Sprite[] _turnSpriteTiles;
	[Header("Dice Again")]
	[SerializeField] private Sprite[] _diceSpriteTiles;
	[Header("Travel")]
	[SerializeField] private Sprite[] _travelSpriteTiles;

	private int _tileID;

	public TileType TileType { get => _tileType; }
	public TileData TileData { get => _tileData;  }

	public void Initialize(int i)
	{
		_tileID = i;
		_tileNumberText.text = i.ToString();
	}

	internal void SetTileData(TileData tileData)
	{
		_tileData = tileData;
		_tileType = EnumConverter.StringToTileType(tileData.type);
		_tileDescriptionText.text = "";

		switch (_tileType)
		{
			case TileType.Start:
				_tileImage.sprite = _startSpriteTiles[UnityEngine.Random.Range(0, _startSpriteTiles.Length)];
				break;
			case TileType.Challenge:
				//GetURLImage(tileData.challenge.url_image);
				_tileDescriptionText.text = tileData.challenge.description;
				break;
			case TileType.Question:
				//GetURLImage(tileData.challenge.url_image);
				_tileDescriptionText.text = tileData.question.statement;
				break;
			case TileType.LoseTurnsUntil:
				_tileImage.sprite = _turnSpriteTiles[UnityEngine.Random.Range(0, _turnSpriteTiles.Length)];
				break;
			case TileType.RollDicesAgain:
				_tileImage.sprite = _diceSpriteTiles[UnityEngine.Random.Range(0, _diceSpriteTiles.Length)];
				break;
			case TileType.TravelToTile:
				_tileImage.sprite = _travelSpriteTiles[UnityEngine.Random.Range(0, _travelSpriteTiles.Length)];
				break;
			case TileType.End:
				// Assuming no image for End or handled differently
				break;
		}
	}

	private async Task GetURLImage(string url_image)
	{
		Sprite sprite = await DALLE2.DownloadSprite(url_image);  // Assuming DALLE2 is a hypothetical API call
		_tileImage.sprite = sprite;
	}
}
