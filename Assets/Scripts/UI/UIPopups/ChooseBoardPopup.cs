using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChooseBoardPopup : MonoBehaviour
{
	#region Fields

	[SerializeField] private BoardUIElement _boardUIElementPrefab;
	[SerializeField] private ToggleGroup _toggleGroup;
	[SerializeField] private RectTransform _contentScroll;

	[SerializeField] private TextMeshProUGUI _selectedTitleText;
	[SerializeField] private TextMeshProUGUI _selectedProposalText;

	[SerializeField] private Button _playButton;
	[SerializeField] private Button _editButton;
	[SerializeField] private Button _backButton;

	private BoardData _currentSelectedBoard;
	private List<GameObject> _boardElements = new List<GameObject>();

	#endregion

	#region Unity Callbacks

	private void Start()
	{
		//PLAY
		_playButton.onClick.AddListener(() =>
		{
			gameObject.SetActive(false);
		});

		//EDIT
		_editButton.onClick.AddListener(() =>
		{
			GameController.GameState = GameStateState.Editing;
			gameObject.SetActive(false);
		});

		_backButton.onClick.AddListener(() =>
		{
			_currentSelectedBoard = null;
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
		if (_boardElements.Count > 0)
		{
			foreach (GameObject board in _boardElements) 
				Destroy(board);
			_boardElements.Clear();
		}

		// Instantiate level items
		int i = 0;
		foreach (BoardData boardData in boardDataList)
		{
			BoardUIElement boardElement = Instantiate(_boardUIElementPrefab, _boardUIElementPrefab.transform.parent);

			Sprite image = await DALLE2.DownloadSprite(boardData.imageURL);  
			if (image == null)
			{
				string url = Path.Combine(Application.streamingAssetsPath, "DefaultBoardImage.png");
				image = await DALLE2.DownloadSprite(url);
			}
			boardElement.Initialize(boardData, image, _toggleGroup, i);				

			boardElement.OnBoardSelected += OnBoardSelected;

			_boardElements.Add(boardElement.gameObject);
			i++;
		}

		float newH = 400 * ((int)(boardDataList.Count / 3) + 1);
		_contentScroll.sizeDelta = new Vector2(_contentScroll.sizeDelta.x, newH);


		OnBoardSelected(boardDataList[0]);
	}

	public void OnBoardSelected(BoardData board)
	{
		_currentSelectedBoard = board;		
		_selectedTitleText.text = board.tittle;
		int q = board.questionsCount;
		int ch = board.challengesCount;
		if (q > 0 || ch > 0)
		_selectedProposalText.text = board.proposal + " ( "+q+" Preguntas / "+ch+" Desaf�os )";
		else
		_selectedProposalText.text = board.proposal ;
	}

	#endregion
}
