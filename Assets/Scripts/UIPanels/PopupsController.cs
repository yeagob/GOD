using System;
using System.Threading.Tasks;
using UnityEngine;

public class PopupsController : MonoBehaviour
{
	#region Fields

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

	//public void ShowVictoryPopup()
	//{
	//	_victoryPopup.ShowAsync(false, false, true);
	//}
	#endregion
}
