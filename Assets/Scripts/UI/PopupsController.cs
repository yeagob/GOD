using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Core.Enums;
using DG.Tweening.CustomPlugins;
using DG.Tweening.Plugins.Core;
using DG.Tweening.Plugins.Core.PathCore;
using DG.Tweening.Plugins.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class PopupsController : MonoBehaviour
{
	#region Fields

	[SerializeField] private TextMeshProUGUI _genericText;

	[SerializeField] private WelcomePopups _welcomePopups;
	[SerializeField] private CreateOrChooseBoardPopup _createOrChooosePopup;
	[SerializeField] private CreateBoardQuestionPopup _createBoardQuestion;
	[SerializeField] private ChooseBoardPopup _chooseBoardPopup;
	[SerializeField] private EditBoardPopup _editBoardPopup;
	[SerializeField] private BoardDataPopup _boardDataPopup;
	[SerializeField] private PlayerCreationController _playerCreationController;
	[SerializeField] private PlayerTurnPopup _playerTurnPopup;
	[SerializeField] private RollDicePopup _rollDicePopup;
	[SerializeField] private QuestionPopup _questionPopup;
	[SerializeField] private ChallengePopup _challengePopup;

	#endregion

	#region Properties
	//TODO: mal
	public PatoCienciaPopup PatoCienciaPopup;

	//TODO: Necesito un PlayerController que no sea un POPUP!
	public PlayerCreationController PlayerCreationController { get => _playerCreationController;  }

	#endregion

	private void Awake()
	{
		HideAll();
	}

	#region Public Methods

	public async Task ShowWelcome()
	{
		 await _welcomePopups.ShowAsync();
	}

	public async Task<bool> ShowCreateOrChooseBoard()
	{
		return await _createOrChooosePopup.ShowAsync();
	}

	public async Task<string> ShowCreateBoardQuestionPopup()
	{
		return await _createBoardQuestion.ShowAsync();
	}

	internal async Task<BoardData> ShowChooseBoardPopup(List <BoardData> gameBoards)
	{
		return await _chooseBoardPopup.ShowAsync(gameBoards);
	}

	internal async Task<GameData> ShowEditBoardPopup(GameData gameData)
	{
		return await _editBoardPopup.ShowAsync(gameData);
	}
	internal async Task<GameData> ShowEditBoardPopup(BoardData boardData)
	{
		return await _editBoardPopup.ShowAsync(boardData);
	}

	public async Task ShowPlayerTurn(Player currentPlayer)
	{
		await _playerTurnPopup.ShowAsync(currentPlayer);
	}

	public async Task<bool> ShowQuestion(QuestionData question)
	{
		return await _questionPopup.ShowAsync(question, this);
	}

	public async Task<bool> ShowChallengePlayer(Player currentPlayer, bool firstTime)
	{
		return await _challengePopup.ShowAsync(currentPlayer, firstTime);
	}

	public async Task ShowPlayerDiceValue(int diceValue)
	{
		await _rollDicePopup.ShowAsync(diceValue);

	}

	public async Task ShowBoardInfoPopup(BoardData board)
	{
		await _boardDataPopup.ShowAsync(board);
	}

	public async Task<bool> ShowGenericMessage(string message, float time = 3, Color color = default)
	{
		bool userInteraction = false;
		if (color == default)
			color = Color.white;

		_genericText.gameObject.SetActive(true);
		_genericText.text = message;
		_genericText.color = color;
		_genericText.transform.localScale = Vector3.zero;

		// Animar de pequeño a grande (scale=1)
		Tween scaleUp = _genericText.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);

		// Esperar a que la animación de escala hacia arriba se complete
		await scaleUp.AsyncWaitForCompletion();

		// Crear tareas para el retraso y la entrada del usuario		
		userInteraction = await WaitForInput(time);

		// Animar de grande a pequeño
		Tween scaleDown = _genericText.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack);
		await scaleDown.AsyncWaitForCompletion();

		_genericText.gameObject.SetActive(false);

		return userInteraction;
	}

	private async Task<bool> WaitForInput(float time)
	{		
		while (time > 0)
		{
			if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
			{
				return true;
			}
			await Task.Yield();
			time -= Time.deltaTime;
		}
		return false;
	}

	internal void HideAll()
	{
		_welcomePopups.gameObject.SetActive(false);
		_challengePopup.gameObject.SetActive(false);
		_rollDicePopup.gameObject.SetActive(false);
		_questionPopup.gameObject.SetActive(false);
		_playerTurnPopup.gameObject.SetActive(false);
		_boardDataPopup.gameObject.SetActive(false);
		_playerCreationController.gameObject.SetActive(false);
		_chooseBoardPopup.gameObject.SetActive(false);
		_createBoardQuestion.gameObject.SetActive(false);
		_createOrChooosePopup.gameObject.SetActive(false);
		_editBoardPopup.gameObject.SetActive(false);
	}

	#endregion

}
