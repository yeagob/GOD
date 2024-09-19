using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Core.Enums;
using DG.Tweening.CustomPlugins;
using DG.Tweening.Plugins.Core;
using DG.Tweening.Plugins.Core.PathCore;
using DG.Tweening.Plugins.Options;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class PopupsController : MonoBehaviour
{
	#region Fields
	[SerializeField] private TextMeshProUGUI _genericText;

	[SerializeField] private WelcomePopups _welcomePopups;
	[SerializeField] private ChallengePopup _challengePopup;
	[SerializeField] private RollDicePopup _rollDicePopup;
	[SerializeField] private QuestionPopup _questionPopup;
	[SerializeField] private PlayerTurnPopup _playerTurnPopup;
	[SerializeField] private VictoryPopup _victoryPopup;
	[SerializeField] private BoardDataPopup _boardDataPopup;
	[SerializeField] private PlayerCreationController _playerCreationController;

	#endregion

	#region Properties
	public ChallengePopup ChallengePopup { get => _challengePopup;  }
	public RollDicePopup RollDicePopup { get => _rollDicePopup;  }
	public VictoryPopup VictoryPopup { get => _victoryPopup;  }
	public PlayerCreationController PlayerCreationController { get => _playerCreationController;  }

	#endregion

	//TODO: Desactivar todos los paneles inicialmente!!

	#region Public Methods

	public async Task<string[]> ShowWelcome()
	{
		 return await _welcomePopups.ShowAsync();
	}

	public void HideWelcome()
	{
		_welcomePopups.gameObject.SetActive(false);
	}

	public async Task ShowPlayerTurn(Player currentPlayer)
	{
		await _playerTurnPopup.ShowAsync(currentPlayer);
	}

	public async Task<bool> ShowChallengePlayer(Player currentPlayer, bool firstTime)
	{
		return await _challengePopup.ShowAsync(currentPlayer, firstTime);
	}

	public async Task ShowPlayerDiceValue(int diceValue)
	{
		await _rollDicePopup.ShowAsync(diceValue);

	}
	
	public async Task<bool> ShowQuestion(QuestionData question)
	{
		return await _questionPopup.ShowAsync(question);

	}	

	public async Task ShowBoardDataPopup(BoardData board)
	{
		await _boardDataPopup.ShowAsync(board);

	}

	public async Task ShowGenericMessage(string message, float time = 3)
	{
		_genericText.gameObject.SetActive(true);
		_genericText.text = message;
		_genericText.transform.localScale = Vector3.zero;

		// Animar de pequeño a grande (scale=1)
		Tween scaleUp = _genericText.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);

		// Esperar a que la animación de escala hacia arriba se complete
		await scaleUp.AsyncWaitForCompletion();

		// Crear tareas para el retraso y la entrada del usuario		
		await WaitForInput(time);

		// Animar de grande a pequeño
		Tween scaleDown = _genericText.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack);
		await scaleDown.AsyncWaitForCompletion();

		_genericText.gameObject.SetActive(false);
	}

	private async Task WaitForInput(float time)
	{		
		while (time > 0)
		{
			if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
			{
				break;
			}
			await Task.Yield();
			time -= Time.deltaTime;
		}
	}

	//public void ShowVictoryPopup()
	//{
	//	_victoryPopup.ShowAsync(false, false, true);
	//}
	#endregion
}
