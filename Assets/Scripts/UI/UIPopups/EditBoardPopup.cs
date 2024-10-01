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
	[SerializeField] private Button _backButton;
	[SerializeField] private Button _rerollButton;

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
	[SerializeField] private Button _validateQuestion;

	// Sub Controllers for Challenges
	[Header("Challenges")]
	[SerializeField] private GameObject _challengesPanel;
	[SerializeField] private TextMeshProUGUI _challengesToValidateText;
	[SerializeField] private List<Button> _challengeTypesButtons; 
	[SerializeField] private Button _challengeTypeButtonPrefab; 

	[SerializeField] private TMP_InputField _challengeTypeInput;
	[SerializeField] private TMP_InputField _challengeDescriptionInput;
	[SerializeField] private Button _addChallengeType;
	[SerializeField] private Button _validateChallenge;
	[SerializeField] private List<string> _defaultChallengeTypes = new List<string>();


	private GameData _gameData = new GameData();
	private int _validatedQuestionsCount = 0;
	private int _validatedChallengesCount = 0;

	#endregion

	#region Properties

	public GameData GameData => _gameData;

	#endregion

	#region Unity Callbacks

	private void Start()
	{
		//Play
		_playButton.onClick.AddListener(Play);
		_playButton.interactable = false;

		//Back
		_backButton.onClick.AddListener(Back);

		//Reroll Image
		_rerollButton.onClick.AddListener(() => RerollImage().WrapErrors());

		//Add Challenge
		_addChallengeType.onClick.AddListener(AddChallengeType);

		//Validations
		_validateChallenge.onClick.AddListener(ValidateChallenge);
		_validateQuestion.onClick.AddListener(ValidateQuestion);

		//Slider Update!
		_questionsChallengesSlider.onValueChanged.AddListener(SliderUpdated);
	}


	#endregion

	#region Public Methods

	public async Task<GameData> ShowAsync(BoardData boardData)
	{
		// Convert BoardData to GameData
		GameData gameData = ConvertBoardDataToGameData(boardData);

		_playButton.interactable = true;

		// Pass the converted GameData to the second ShowAsync
		return await ShowAsync(gameData);
	}

	public async Task<GameData> ShowAsync(GameData gameData)
	{
		_gameData = gameData;
		GameController.GameState = GameStateState.Editing;

		// Show popup
		gameObject.SetActive(true);

		// Set data in the UI elements
		_tittleInput.text = gameData.tittle;
		_proposalInput.text = gameData.proposal;

		// Set question and challenge counts from the slider
		_questionsChallengesSlider.minValue = 0;
		_questionsChallengesSlider.maxValue = 25;
		if (gameData.challengesCount + gameData.questionsCount < 25)
			_questionsChallengesSlider.value = 13;
		else
			_questionsChallengesSlider.value = gameData.challengesCount;

		// Place the first challenge and question into the UI
		if (gameData.challenges.Count > 0)
		{
			LoadChallenge(gameData.challenges[0]);
		}

		if (gameData.questions.Count > 0)
		{
			LoadQuestion(gameData.questions[0]);
		}

		// Load challenge types as buttons
		LoadChallengeTypes(gameData.challengesTypes);

		RefreshValidations();

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

		_playButton.interactable = false;

		return _gameData;
	}

	#endregion

	#region Private Methods	
	private void Play()
	{
		_gameData = CreateGameData();
		gameObject.SetActive(false);
	}

	private void Back()
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
		newGameData.questionsCount = 25 - newGameData.challengesCount;

		//Challenges Types
		foreach (Button challengeButton in _challengeTypesButtons)
		{
			string type = challengeButton.GetComponentInChildren<TextMeshProUGUI>().text;
			type = type.Substring(2); //Eliminamos el texto 'x '
			newGameData.challengesTypes.Add(type);
		}

		// Challenges
		newGameData.challenges = _gameData.challenges.Take(3).ToList();

		//Questions
		newGameData.questions = _gameData.questions.Take(3).ToList();

		newGameData.imageURL = _gameData.imageURL;

		return newGameData;
	}

	private GameData ConvertBoardDataToGameData(BoardData boardData)
	{
		GameData newGameData = new GameData
		{
			tittle = boardData.tittle,
			proposal = boardData.proposal,
			questionsCount = boardData.questionsCount,
			challengesCount = boardData.challengesCount,
			challengesTypes = _defaultChallengeTypes,
			imageURL = boardData.imageURL,
			challenges = new List<string>(),
			questions = new List<QuestionData>()
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


	private void SliderUpdated(float value)
	{
		_gameData.challengesCount = Mathf.RoundToInt(_questionsChallengesSlider.value);
		_gameData.questionsCount = 25 - _gameData.challengesCount;

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
		if (!string.IsNullOrEmpty(challengeType))
		{
			Button newButton = CreateChallengeTypeButton(challengeType);
			_gameData.challengesTypes.Add(challengeType);
		}
	}

	private void RemoveChallengeType(Button button, string challengeType)
	{
		Destroy(button.gameObject);
		_gameData.challengesTypes.Remove(challengeType);
	}

	private async Task RerollImage()
	{
		_rerollButton.interactable = false;
		_rerollButton.transform.DORotate(new Vector3(0, 0, 360), 1f, RotateMode.FastBeyond360)
			.SetLoops(-1, LoopType.Restart)
			.SetEase(Ease.Linear);

		DALLE2 dalle2 = new DALLE2();
		string prompt = "Crea una imagen que represente un juego de tablero con este título: " + _tittleInput.text + ". El/Los protagonistas de la imagen es/son patitos de goma amarillos.";
		//string prompt = "Crea una imagen de un patito de goma.";
		Sprite image = await dalle2.GenerateImage(prompt);
		_boardImage.sprite = image;
		_gameData.imageURL = DALLE2.LastImageUrl;

		_rerollButton.transform.DOKill();
		_rerollButton.interactable = true;


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
			_questionsToValidateText.color = Color.blue;
		if (_validatedQuestionsCount == 3)
			_questionsToValidateText.color = Color.green;

		if (_validatedQuestionsCount < _gameData.questions.Count)
			LoadQuestion(_gameData.questions[_validatedQuestionsCount]);
		else
		{
			_validateQuestion.interactable = false;
			_questionsToValidateText.text = "Validación Completada!";
		}
		RefreshPlayButton();
	}

	private void LoadQuestion(QuestionData questionData)
	{
		_statementInput.text = questionData.statement;
		for (int i = 0; i < _answersText.Length; i++)
		{
			_answersText[i].text = questionData.options[i];
			_correctAnswerToggle[i].isOn = (i == questionData.correctId);
		}
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
			_challengesToValidateText.color = Color.blue;
		if (_validatedChallengesCount == 3)
			_challengesToValidateText.color = Color.green;

		if (_validatedChallengesCount < _gameData.challenges.Count)
			LoadChallenge(_gameData.challenges[_validatedChallengesCount]);
		else
		{
			_validateChallenge.interactable = false;
			_challengesToValidateText.text = "Validación Completada!";
		}

		RefreshPlayButton();
	}

	private void LoadChallenge(string challenge)
	{
		_challengeDescriptionInput.text = challenge;
	}

	private void RefreshValidations()
	{
		_questionsToValidateText.text = $"{_validatedQuestionsCount}/${_gameData.questions.Count} Preguntas Corregidas y Validadas";
		_challengesToValidateText.text = $"{_validatedChallengesCount}/${_gameData.challenges.Count} Desafíos Corregidos y Validados";

		if (_validatedChallengesCount < 3)
			_validateChallenge.interactable = true;
		
		if (_validatedQuestionsCount < 3)
			_validateQuestion.interactable = true;
	}

	private void RefreshPlayButton()
	{
		bool buttonState = true;
		int questions = (int)_questionsChallengesSlider.value;
		int challenges = 25-questions;

		if (questions > _validatedQuestionsCount && _validatedQuestionsCount < 2)
			buttonState = false;

		if (challenges > _validatedChallengesCount && _validatedChallengesCount < 2)
			buttonState = false;

		_playButton.interactable = buttonState;
	}
	#endregion
}
