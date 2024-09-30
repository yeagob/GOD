using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChooseBoardPopup : MonoBehaviour
{
	#region Fields

	[SerializeField] private BoardUIElement _boardUIElementPrefab;
	[SerializeField] private ToggleGroup _toggleGroup;

	[SerializeField] private TextMeshProUGUI _selectedTitleText;
	[SerializeField] private TextMeshProUGUI _selectedProposalText;

	[SerializeField] private Button _playButton;
	[SerializeField] private Button _editButton;

	private BoardData _currentSelectedBoard;

	#endregion


	#region Unity Callbacks

	private void Start()
	{
		//PLAY
		_playButton.onClick.AddListener(() =>
		{
			GameController.GameState = GameStateState.Playing;
			gameObject.SetActive(false);
		});

		//EDIT
		_editButton.onClick.AddListener(() =>
		{
			GameController.GameState = GameStateState.Editing;
			gameObject.SetActive(false);
		});
	}

	#endregion

	#region Public Methods

	public async Task<BoardData> ShowAsync(List<BoardData> gameData)
	{
		// Set up the UI
		gameObject.SetActive(true);

		Initialize(gameData);

		while (gameObject.activeSelf)
			await Task.Yield();

		return _currentSelectedBoard;
	}

	#endregion

	#region Private Methods

	private async void Initialize(List<BoardData> boardDataList)
	{

		// Instantiate level items
		foreach (BoardData boardData in boardDataList)
		{
			BoardUIElement boardElement = Instantiate(_boardUIElementPrefab, _boardUIElementPrefab.transform.parent);

			Sprite image = await DALLE2.DownloadSprite(boardData.imageURL);  

			boardElement.Initialize(boardData, image, _toggleGroup);				

			boardElement.OnBoardSelected += OnBoardSelected;
		}

		OnBoardSelected(boardDataList[0]);
	}

	public void OnBoardSelected(BoardData board)
	{
		_currentSelectedBoard = board;		
		_selectedTitleText.text = board.tittle;
		_selectedProposalText.text = board.proposal;
	}

	#endregion
}
