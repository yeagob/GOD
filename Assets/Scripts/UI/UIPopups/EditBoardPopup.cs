using DG.Tweening;
using GOD.Utils;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditBoardPopup : MonoBehaviour
{
	#region Fields

	[Header("Header")]
	[SerializeField] private TMP_InputField _tittleInput;
	[SerializeField] private TMP_InputField _proposalInput;
	[SerializeField] private Image _boardImage;//267x322
	[SerializeField] private Button _playButton;
	[SerializeField] private Button _createNewButton;
	[SerializeField] private Button _closeButton;
	[SerializeField] private Button _rerollImageButton;

	[Header("Questions-Challenges Slider ")]
	[SerializeField] private Slider _questionsChallengesSlider;
	[SerializeField] private TextMeshProUGUI _questionsCountText;
	[SerializeField] private TextMeshProUGUI _challengesCountText;

	// Sub Controllers for Questions
	[Header("Questions")]
	[SerializeField] private GameObject _questionsPanel;
	[SerializeField] private TextMeshProUGUI _questionsToValidateText;
	[SerializeField] private TMP_InputField _statementInput;
	[SerializeField] private TMP_InputField[] _answersText;
	[SerializeField] private Toggle[] _correctAnswerToggle;
	[SerializeField] private Button _validateQuestionButton;
	[SerializeField] private Button _discardQuestionButton;
	[SerializeField] private Button _nextQuestionButton;
	[SerializeField] private Button _prevQuestionButton;

	// Sub Controllers for Challenges
	[Header("Challenges")]
	[SerializeField] private GameObject _challengesPanel;
	[SerializeField] private TextMeshProUGUI _challengesToValidateText;
	[SerializeField] private List<Button> _challengeTypesButtons;
	[SerializeField] private Button _challengeTypeButtonPrefab;
	[SerializeField] private Button _validateChallengeButton;
	[SerializeField] private Button _discardChallengeButton;
	[SerializeField] private Button _nextChallengeButton;
	[SerializeField] private Button _prevChallengeButton;

	[Header("Challenges Types")]
	[SerializeField] private List<string> _defaultChallengeTypes = new List<string>();
	[SerializeField] private GameObject _challengeTypesPanel;
	[SerializeField] private TMP_InputField _challengeTypeInput;
	[SerializeField] private TMP_InputField _challengeDescriptionInput;
	[SerializeField] private Button _addChallengeType;

	private GameData _gameData = new GameData();
	private int _validatedQuestionsCount = 0;
	private int _validatedChallengesCount = 0;
	private int _currentQuestionIndex = 0;
	private int _currentChallengeIndex = 0;

	#endregion

	#region Properties

	public GameData GameData => _gameData;

	#endregion

	#region Unity Callbacks

	private void Start()
	{
		//Play
		_playButton.onClick.AddListener(CreateOrPlay);

		//Create New
		_createNewButton.onClick.AddListener(CreateNewBoardFromCurrent);

		//Back
		_closeButton.onClick.AddListener(Close);

		//Reroll Image
		_rerollImageButton.onClick.AddListener(() => RerollImage().WrapErrors());

		//Add Challenge
		_addChallengeType.onClick.AddListener(AddChallengeType);

		//Validations
		_validateChallengeButton.onClick.AddListener(ValidateChallenge);
		_validateQuestionButton.onClick.AddListener(ValidateQuestion);

		//Discards
		_discardChallengeButton.onClick.AddListener(DiscardChallenge);
		_discardQuestionButton.onClick.AddListener(DiscardQuestion);

		//Prev/Next Buttons
		_nextChallengeButton.onClick.AddListener(LoadNextChallenge);
		_prevChallengeButton.onClick.AddListener(LoadPrevChallenge);
		_nextQuestionButton.onClick.AddListener(LoadNextQuestion);
		_prevQuestionButton.onClick.AddListener(LoadPrevQuestion);

		//Slider Update!
		_questionsChallengesSlider.onValueChanged.AddListener(SliderUpdated);
	}

	private void CreateNewBoardFromCurrent()
	{
		GameController.GameState = GameStateState.Creating;
		ShowAsync(_gameData).WrapErrors();
	}

	#endregion

	#region Public Methods

	public async Task<BoardData> ShowAsync(BoardData boardData)
	{
		// Convert BoardData to GameData
		GameData gameData = ConvertBoardDataToGameData(boardData, _defaultChallengeTypes);

		ShowAsync(gameData).WrapErrors();

		SetEditMode();
		
		while (gameObject.activeSelf)
		{
			await Task.Yield();
		}

		if (_gameData == null)
			return null;

		return new BoardData(_gameData);
	}

	private void SetEditMode()
	{
		_questionsChallengesSlider.gameObject.SetActive(false);
		_validateChallengeButton.gameObject.SetActive(false);
		_validateQuestionButton.gameObject.SetActive(false);
		_discardChallengeButton.gameObject.SetActive(false);
		_discardQuestionButton.gameObject.SetActive(false);
		_challengeTypesPanel.gameObject.SetActive(false);
		_playButton.GetComponentInChildren<TextMeshProUGUI>().text = "Jugar";
		_questionsToValidateText.text = $"1/{_gameData.questions.Count} Preguntas";
		_challengesToValidateText.text = $"1/{_gameData.challenges.Count} Desafíos";
		_questionsCountText.text = "";
		_challengesCountText.text = "";
		_questionsToValidateText.color = Color.black;
		_challengesToValidateText.color = Color.black;

		_createNewButton.gameObject.SetActive(true);
		_prevChallengeButton.gameObject.SetActive(true);
		_nextChallengeButton.gameObject.SetActive(true);
		_prevQuestionButton.gameObject.SetActive(true);
		_nextQuestionButton.gameObject.SetActive(true);

		_playButton.interactable = true;
	}

	private void SetCreateMode()
	{
		_questionsChallengesSlider.gameObject.SetActive(true);
		_validateChallengeButton.gameObject.SetActive(true);
		_validateQuestionButton.gameObject.SetActive(true);
		_discardChallengeButton.gameObject.SetActive(true);
		_discardQuestionButton.gameObject.SetActive(true);
		_challengeTypesPanel.gameObject.SetActive(true);
		_playButton.GetComponentInChildren<TextMeshProUGUI>().text = "Crear";
		_questionsToValidateText.text = $"0/{_gameData.questions.Count} Preguntas Corregidas y Validadas";
		_challengesToValidateText.text = $"0/{_gameData.challenges.Count} Desafíos Corregidos y Validados";
		_validateChallengeButton.interactable = true;
		_discardChallengeButton.interactable = true;
		_validateQuestionButton.interactable = true;
		_discardQuestionButton.interactable = true;

		_createNewButton.gameObject.SetActive(false);
		_prevChallengeButton.gameObject.SetActive(false);
		_nextChallengeButton.gameObject.SetActive(false);
		_prevQuestionButton.gameObject.SetActive(false);
		_nextQuestionButton.gameObject.SetActive(false);

		_playButton.interactable = false;

		_validatedQuestionsCount = 0;
		_validatedChallengesCount = 0;
	}

	public async Task<GameData> ShowAsync(GameData gameData)
	{
		_gameData = gameData;

		// Show popup
		gameObject.SetActive(true);

		// Set data in the UI elements
		_tittleInput.text = gameData.tittle;
		_proposalInput.text = gameData.proposal;
		_questionsToValidateText.color = Color.red;
		_challengesToValidateText.color = Color.red;

		_tittleInput.onEndEdit.AddListener(UpdateTittleData);
		_proposalInput.onEndEdit.AddListener(UpdateProposalData);

		// Set questions and challenges for the slider
		_questionsChallengesSlider.minValue = 0;
		_questionsChallengesSlider.maxValue = 25;
		if (gameData.challengesCount + gameData.questionsCount < 20)
			_questionsChallengesSlider.value = 13;
		else
			_questionsChallengesSlider.value = gameData.challengesCount;

		SliderUpdated(_questionsChallengesSlider.value);


		// Place the first challenge and question into the UI
		if (gameData.challenges.Count > 0)
		{
			LoadChallenge(gameData.challenges[0], 0);
			_currentChallengeIndex = 0;
		}

		if (gameData.questions.Count > 0)
		{
			LoadQuestion(gameData.questions[0], 0);
			_currentQuestionIndex = 0;
		}

		// Load challenge types as buttons
		if (gameData.challengesTypes.Count > 0)
			LoadChallengeTypes(gameData.challengesTypes);
		else
			LoadChallengeTypes(_defaultChallengeTypes);

		RefreshValidations();

		SetCreateMode();

		//Load Board Image
		if (gameData.imageURL.IsNullOrWhitespace())
			_boardImage.sprite = await DALLE2.LoadSpriteFromStreamigAssets("DefaultBoardImage.png");
		else
			_boardImage.sprite = await DALLE2.DownloadSprite(gameData.imageURL);

		//Whait...
		while (gameObject.activeSelf)
		{
			await Task.Yield();
		}

		return _gameData;
	}

	private void UpdateTittleData(string data)
	{
		_gameData.tittle = data;
	}

	private void UpdateProposalData(string data)
	{
		_gameData.proposal = data;

	}

	public static GameData ConvertBoardDataToGameData(BoardData boardData, List<string> challengeTypes)
	{
		//PARCHE!!
		if (challengeTypes == null)
			challengeTypes = new List<string>();

		if (boardData.questionsCount + boardData.challengesCount < 20)
		{
			boardData.challengesCount = boardData.tiles.Count(tile => tile.type == "Challenge");
			boardData.questionsCount = boardData.tiles.Count(tile => tile.type == "Question");
		}

		GameData newGameData = new GameData
		{
			tittle = boardData.tittle,
			proposal = boardData.proposal,
			questionsCount = boardData.questionsCount,
			challengesCount = boardData.challengesCount,
			challengesTypes = challengeTypes,
			imageURL = boardData.imageURL,
			challenges = new List<string>(),
			questions = new List<QuestionData>(),
		};

		// Iterate through the tiles and populate questions and challenges
		foreach (TileData tile in boardData.tiles)
		{
			if (tile.type == "Challenge" && tile.challenge != null)
			{
				newGameData.challenges.Add(tile.challenge.description);
			}
			if (tile.type == "Question" && tile.question != null)
			{
				newGameData.questions.Add(tile.question);
			}
		}

		return newGameData;
	}

	#endregion

	#region Private Methods	

	private void CreateOrPlay()
	{
		if (GameController.GameState != GameStateState.Editing)
			_gameData = CreateGameData();

		gameObject.SetActive(false);
	}

	private void Close()
	{
		_gameData = null;
		gameObject.SetActive(false);
	}

	private GameData CreateGameData()
	{
		GameData newGameData = new GameData();

		// Set title and proposal
		newGameData.tittle = _tittleInput.text;
		newGameData.proposal = _proposalInput.text;

		// Set question and challenge counts based on slider
		newGameData.challengesCount = Mathf.RoundToInt(_questionsChallengesSlider.value);
		newGameData.questionsCount = (int)_questionsChallengesSlider.maxValue - newGameData.challengesCount;

		//Challenges Types
		foreach (Button challengeButton in _challengeTypesButtons)
		{
			if (challengeButton == null)
				continue;

			string type = challengeButton.GetComponentInChildren<TextMeshProUGUI>().text;
			type = type.Substring(2); //Eliminamos el texto 'x '
			newGameData.challengesTypes.Add(type);
		}

		// Challenges
		newGameData.challenges = _gameData.challenges.Take(_validatedChallengesCount).ToList();

		//Questions
		newGameData.questions = _gameData.questions.Take(_validatedQuestionsCount).ToList();

		newGameData.imageURL = _gameData.imageURL;

		return newGameData;
	}


	private void SliderUpdated(float value)
	{
		_gameData.challengesCount = Mathf.RoundToInt(_questionsChallengesSlider.value);
		_gameData.questionsCount = (int)_questionsChallengesSlider.maxValue - _gameData.challengesCount;

		_challengesCountText.text = "Desafíos: " + _gameData.challengesCount;
		_questionsCountText.text = "Preguntas: " + _gameData.questionsCount;

		if (_gameData.questionsCount == 0)
			_questionsPanel.SetActive(false);
		else
			_questionsPanel.SetActive(true);

		if (_gameData.challengesCount == 0)
			_challengesPanel.SetActive(false);
		else
			_challengesPanel.SetActive(true);

		RefreshPlayButton();
	}


	private void LoadChallengeTypes(List<string> challengeTypes)
	{
		foreach (Button challengeTypeButton in _challengeTypesButtons)
			Destroy(challengeTypeButton.gameObject);

		_challengeTypesButtons.Clear();

		foreach (string challengeType in challengeTypes)
		{
			Button newButton = CreateChallengeTypeButton(challengeType);
			newButton.onClick.AddListener(() => RemoveChallengeType(newButton, challengeType));
		}
	}

	private Button CreateChallengeTypeButton(string challengeType)
	{
		Button newButton = Instantiate(_challengeTypeButtonPrefab, _challengeTypeButtonPrefab.transform.parent);
		newButton.gameObject.SetActive(true);
		newButton.transform.SetSiblingIndex(1);
		newButton.GetComponentInChildren<TextMeshProUGUI>().text = "x " + challengeType;
		newButton.onClick.AddListener(() => RemoveChallengeType(newButton, challengeType));

		_challengeTypesButtons.Add(newButton);

		return newButton;
	}

	private void AddChallengeType()
	{
		string challengeType = _challengeTypeInput.text;
		_challengeTypeInput.text = "";
		if (!string.IsNullOrEmpty(challengeType))
		{
			Button newButton = CreateChallengeTypeButton(challengeType);
			_gameData.challengesTypes.Add(challengeType);
		}
	}

	private void RemoveChallengeType(Button button, string challengeType)
	{
		_challengeTypesButtons.Remove(button);
		Destroy(button.gameObject);
		_gameData.challengesTypes.Remove(challengeType);
	}

	private async Task RerollImage()
	{
		_rerollImageButton.interactable = false;
		_rerollImageButton.transform.DORotate(new Vector3(0, 0, -360), 1f, RotateMode.FastBeyond360)
			.SetLoops(-1, LoopType.Restart)
			.SetEase(Ease.Linear);

		DALLE2 dalle2 = new DALLE2();
		string prompt = "Crea una imagen que represente un juego de tablero con este título: " + _tittleInput.text + ". El/Los protagonistas de la imagen es/son patitos de goma amarillos.";
		//string prompt = "Crea una imagen de un patito de goma.";
		Sprite image = await dalle2.GenerateImage(prompt);
		_boardImage.sprite = image;
		_gameData.imageURL = DALLE2.LastImageUrl;

		_rerollImageButton.transform.DOKill();
		_rerollImageButton.interactable = true;


	}

	private void LoadPrevQuestion()
	{
		SetEditDataQuestion();

		_currentQuestionIndex--;

		if (_currentQuestionIndex < 0)
			_currentQuestionIndex = _gameData.questions.Count-1;

		LoadQuestion(_gameData.questions[_currentQuestionIndex], _validatedQuestionsCount);
	}

	private void LoadNextQuestion()
	{
		SetEditDataQuestion();
		_currentQuestionIndex++;

		if (_currentQuestionIndex >= _gameData.questions.Count)
			_currentQuestionIndex = 0;

		LoadQuestion(_gameData.questions[_currentQuestionIndex], _validatedQuestionsCount);
	}

	private void LoadPrevChallenge()
	{
		if (GameController.GameState == GameStateState.Editing)
			SetEditDataChallenge();

		_currentChallengeIndex--;

		if (_currentChallengeIndex < 0)
			_currentChallengeIndex = _gameData.challenges.Count-1;

		LoadChallenge(_gameData.challenges[_currentChallengeIndex], _validatedChallengesCount);
	}


	private void LoadNextChallenge()
	{		
		if (GameController.GameState == GameStateState.Editing)
			SetEditDataChallenge();

		_currentChallengeIndex++;

		if (_currentChallengeIndex >= _gameData.challenges.Count)
			_currentChallengeIndex = 0;

		LoadChallenge(_gameData.challenges[_currentChallengeIndex], _validatedChallengesCount);
	}

	private void SetEditDataQuestion()
	{
		// Collect the first question data
		QuestionData question = new QuestionData
		{
			statement = _statementInput.text,
			options = new string[_answersText.Length],
			correctId = Array.FindIndex(_correctAnswerToggle, t => t.isOn)
		};

		for (int i = 0; i < _answersText.Length; i++)
		{
			question.options[i] = _answersText[i].text;
		}

		_gameData.questions[_currentQuestionIndex] = question;
	}
	private void SetEditDataChallenge()
	{
		_gameData.challenges[_currentChallengeIndex] = _challengeDescriptionInput.text;
	}

	private void ValidateQuestion()
	{
		// Collect the first question data
		QuestionData question = new QuestionData
		{
			statement = _statementInput.text,
			options = new string[_answersText.Length],
			correctId = Array.FindIndex(_correctAnswerToggle, t => t.isOn)
		};

		for (int i = 0; i < _answersText.Length; i++)
		{
			question.options[i] = _answersText[i].text;
		}

		_gameData.questions[_validatedQuestionsCount] = question;

		_validatedQuestionsCount++;

		RefreshValidations();

		if (_validatedQuestionsCount == 1)
			_questionsToValidateText.color = Color.yellow;
		if (_validatedQuestionsCount == 2)
			_questionsToValidateText.color = Color.green;
		if (_validatedQuestionsCount == 3)
			_questionsToValidateText.color = Color.blue;

		if (_validatedQuestionsCount < _gameData.questions.Count)
			LoadNextQuestion();
		else
		{
			_validateQuestionButton.interactable = false;
			_discardQuestionButton.interactable = false;

			_questionsToValidateText.text = "Validación Completada!";
		}

		RefreshPlayButton();
	}

	private void DiscardQuestion()
	{
		_gameData.questions.RemoveAt(_currentQuestionIndex);
		if (_validatedQuestionsCount < _gameData.questions.Count)
		{
			LoadNextQuestion();
			if (_currentQuestionIndex > 0)
				_currentQuestionIndex--;
			_questionsToValidateText.text = $"{_currentQuestionIndex}/{_gameData.questions.Count} Preguntas";
		}
		else
		{
			_validateQuestionButton.interactable = false;
			_discardQuestionButton.interactable = false;

			_questionsToValidateText.text = "Validación Completada!";
		}
	}

	private void LoadQuestion(QuestionData questionData, int index)
	{
		_statementInput.text = questionData.statement;
		for (int i = 0; i < _answersText.Length; i++)
		{
			_answersText[i].text = questionData.options[i];
			_correctAnswerToggle[i].isOn = (i == questionData.correctId);
		}

		if (GameController.GameState == GameStateState.Editing)
			_questionsToValidateText.text = $"{_currentQuestionIndex}/{_gameData.questions.Count} Preguntas";				
	}

	int _validationChallengeIndex = 0;
	private void ValidateChallenge()
	{
		_gameData.challenges[_validatedChallengesCount] = _challengeDescriptionInput.text;

		_validatedChallengesCount++;

		RefreshValidations();

		if (_validatedChallengesCount == 1)
			_challengesToValidateText.color = Color.yellow;
		if (_validatedChallengesCount == 2)
			_challengesToValidateText.color = Color.green;
		if (_validatedChallengesCount == 3)
			_challengesToValidateText.color = Color.blue;

		if (_validatedChallengesCount < _gameData.challenges.Count)
			LoadNextChallenge();
		else
		{
			_validateChallengeButton.interactable = false;
			_discardChallengeButton.interactable = false;

			_challengesToValidateText.text = "Validación Completada!";

		}

		RefreshPlayButton();
	}

	private void DiscardChallenge()
	{
		_gameData.challenges.RemoveAt(_currentChallengeIndex);
		if (_validatedChallengesCount < _gameData.challenges.Count)
		{
			LoadNextChallenge();
			if (_currentChallengeIndex > 0)
				_currentChallengeIndex--;
			_challengesToValidateText.text = $"{_currentChallengeIndex}/{_gameData.challenges.Count} Preguntas";
		}
		else
		{
			_validateChallengeButton.interactable = false;
			_discardChallengeButton.interactable = false;

			_challengesToValidateText.text = "Validación Completada!";
		}
	}

	private void LoadChallenge(string challenge, int i)
	{
		_challengeDescriptionInput.text = challenge;

		if (GameController.GameState == GameStateState.Editing)
			_challengesToValidateText.text = $"{_currentChallengeIndex}/{_gameData.challenges.Count} Desafíos";
	}

	private void RefreshValidations()
	{
			_questionsToValidateText.text = $"{_validatedQuestionsCount}/{_gameData.questions.Count} Preguntas Corregidas y Validadas";
			_challengesToValidateText.text = $"{_validatedChallengesCount}/{_gameData.challenges.Count} Desafíos Corregidos y Validados";		

		if (_validatedChallengesCount < 3)
			_validateChallengeButton.interactable = true;

		if (_validatedQuestionsCount < 3)
			_validateQuestionButton.interactable = true;
	}

	private void RefreshPlayButton()
	{
		if (GameController.GameState == GameStateState.Editing)
			return;

		bool buttonState = true;
		int challenges = (int)_questionsChallengesSlider.value;
		int questions = 25 - challenges;

		if (questions > _validatedQuestionsCount && _validatedQuestionsCount < 2)
			buttonState = false;

		if (challenges > _validatedChallengesCount && _validatedChallengesCount < 2)
			buttonState = false;

		_playButton.interactable = buttonState;
	}
	#endregion
}
