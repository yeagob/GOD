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
	[SerializeField] private Image _boardimage;
	[SerializeField] private Button _playButton;
	[SerializeField] private Button _backButton;

	[Header("Questions-Challenges Slider ")]
	[SerializeField] private Slider _questionsChallengesSlider;

	// Sub Controllers for Questions
	[Header("Questions")]
	[SerializeField] private TextMeshProUGUI _questionsToValidate;
	[SerializeField] private TMP_InputField _statementInput;
	[SerializeField] private TMP_InputField[] _answersText;
	[SerializeField] private Toggle[] _correctAnswerToggle;
	[SerializeField] private Button _validateQuestion;

	// Sub Controllers for Challenges
	[Header("Challenges")]
	[SerializeField] private TextMeshProUGUI _challengesToValidate;
	[SerializeField] private Button[] _challengeTypes; // Dynamically created buttons
	[SerializeField] private TMP_InputField _challengeTypeInput;
	[SerializeField] private TMP_InputField _challengeDescriptionInput;
	[SerializeField] private Button _addChallengeType;
	[SerializeField] private Button _validateChallenge;

	private GameData _gameData = new GameData();
	private int _validatedQuestions = 0;
	private int _validatedChallenges = 0;

	#endregion

	#region Properties
	public GameData GameData => _gameData;
	#endregion

	#region Unity Callbacks
	private void Start()
	{
		_playButton.onClick.AddListener(Play);
		_backButton.onClick.AddListener(Back);
		_addChallengeType.onClick.AddListener(AddChallengeType);
	}
	#endregion

	#region Public Methods

	public async Task<GameData> ShowAsync(BoardData boardData)
	{
		_gameData = new GameData();
		GameController.GameState = GameStateState.Editing;

		// Show popup
		gameObject.SetActive(true);

		// Set title and proposal from BoardData
		_tittleInput.text = boardData.tittle;
		_proposalInput.text = boardData.proposal;

		// Set question and challenge counts from BoardData
		_questionsChallengesSlider.minValue = 0;
		_questionsChallengesSlider.maxValue = 50; // 25 questions + 25 challenges
		_questionsChallengesSlider.value = boardData.questionsCount;

		//TODO!!!
		// Set image (if you have an image URL system in place, otherwise you can skip this)
		

		// Load tiles (first question and challenge are assigned to inputs)
		for (int i = 0; i < boardData.tiles.Length; i++)
		{
			TileData tile = boardData.tiles[i];

			// Assign the first challenge
			if (tile.type == "Challenge" && tile.challenge != null)
			{
				_challengeDescriptionInput.text = tile.challenge.description;
				_validatedChallenges++;
			}

			// Assign the first question
			if (tile.type == "Question" && tile.question != null)
			{
				_statementInput.text = tile.question.statement;
				for (int j = 0; j < _answersText.Length; j++)
				{
					_answersText[j].text = tile.question.options[j];
					_correctAnswerToggle[j].isOn = (j == tile.question.correctId);
				}
				_validatedQuestions++;
			}

			// Stop once both are filled
			if (_validatedQuestions > 0 && _validatedChallenges > 0)
			{
				break;
			}
		}

		// Set recommendations for validation
		_questionsToValidate.text = $"{_validatedQuestions}/3 Questions validated";
		_challengesToValidate.text = $"{_validatedChallenges}/3 Challenges validated";

		// Handle dynamic buttons for challenge types
		LoadChallengeTypes(boardData.tiles
			.Where(t => t.type == "Challenge" && t.challenge != null)
			.Select(t => t.challenge.description)
			.ToList());

		// Wait until the popup is closed
		while (gameObject.activeSelf)
		{
			await Task.Yield();
		}

		return _gameData;
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
		_questionsChallengesSlider.maxValue = 50; // 25 questions + 25 challenges
		_questionsChallengesSlider.value = gameData.questionsCount;

		// Place the first challenge and question into the UI
		if (gameData.challenges.Count > 0)
		{
			_challengeDescriptionInput.text = gameData.challenges[0];
		}

		if (gameData.questions.Count > 0)
		{
			_statementInput.text = gameData.questions[0].statement;
			for (int i = 0; i < _answersText.Length; i++)
			{
				_answersText[i].text = gameData.questions[0].options[i];
				_correctAnswerToggle[i].isOn = (i == gameData.questions[0].correctId);
			}
		}

		// Load challenge types as buttons
		LoadChallengeTypes(gameData.challengTypes);

		// Set recommendations to validate at least 3 questions and challenges
		_questionsToValidate.text = $"{_validatedQuestions}/3 Questions validated";
		_challengesToValidate.text = $"{_validatedChallenges}/3 Challenges validated";

		while (gameObject.activeSelf)
		{
			await Task.Yield();
		}

		return _gameData;
	}
	#endregion

	#region Private Methods
	private void Back()
	{
		_gameData = null;
		gameObject.SetActive(false);
	}

	private void Play()
	{
		_gameData = CreateGameData();
		gameObject.SetActive(false);
	}

	private GameData CreateGameData()
	{
		GameData newGameData = new GameData();

		// Set title and proposal
		newGameData.tittle = _tittleInput.text;
		newGameData.proposal = _proposalInput.text;

		// Set question and challenge counts based on slider
		newGameData.questionsCount = Mathf.RoundToInt(_questionsChallengesSlider.value);
		newGameData.challengesCount = 50 - newGameData.questionsCount;

		// Collect challenges
		newGameData.challenges = new List<string> { _challengeDescriptionInput.text };

		// Collect the first question data
		QuestionData firstQuestion = new QuestionData
		{
			statement = _statementInput.text,
			options = new string[_answersText.Length],
			correctId = Array.FindIndex(_correctAnswerToggle, t => t.isOn)
		};

		for (int i = 0; i < _answersText.Length; i++)
		{
			firstQuestion.options[i] = _answersText[i].text;
		}

		newGameData.questions = new List<QuestionData> { firstQuestion };

		return newGameData;
	}

	private void LoadChallengeTypes(List<string> challengeTypes)
	{
		foreach (string challengeType in challengeTypes)
		{
			Button newButton = CreateChallengeTypeButton(challengeType);
			newButton.onClick.AddListener(() => RemoveChallengeType(newButton, challengeType));
		}
	}

	private Button CreateChallengeTypeButton(string challengeType)
	{
		Button newButton = Instantiate(_challengeTypes[0], _challengeTypes[0].transform.parent);
		newButton.gameObject.SetActive(true);
		newButton.GetComponentInChildren<TextMeshProUGUI>().text = challengeType;
		return newButton;
	}

	private void AddChallengeType()
	{
		string challengeType = _challengeTypeInput.text;
		if (!string.IsNullOrEmpty(challengeType))
		{
			Button newButton = CreateChallengeTypeButton(challengeType);
			newButton.onClick.AddListener(() => RemoveChallengeType(newButton, challengeType));
			_gameData.challengTypes.Add(challengeType);
		}
	}

	private void RemoveChallengeType(Button button, string challengeType)
	{
		Destroy(button.gameObject);
		_gameData.challengTypes.Remove(challengeType);
	}

	internal async Task<GameData> ShowAsync(object boardGameData)
	{
		throw new NotImplementedException();
	}
	#endregion
}
