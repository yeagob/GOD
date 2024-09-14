using System;
using System.Threading.Tasks;
using UnityEngine;

public class PopupsController : MonoBehaviour
{
	#region Fields

	[SerializeField] private WelcomePopups _welcomePopups;
	[SerializeField] private ChallengePopup _challengePopup;
	[SerializeField] private RollDicePopup _rollDicePopup;
	[SerializeField] private VictoryPopup _victoryPopup;
	[SerializeField] private PlayerCreationController _playerCreationController;


	#endregion

	#region Properties
	public ChallengePopup ChallengePopup { get => _challengePopup;  }
	public RollDicePopup RollDicePopup { get => _rollDicePopup;  }
	public VictoryPopup VictoryPopup { get => _victoryPopup;  }
	public PlayerCreationController PlayerCreationController { get => _playerCreationController;  }

	#endregion

	#region Public Methods
	public async Task<string[]> ShowWelcome()
	{
		 return await _welcomePopups.ShowAsync();
	}

	internal void HideWelcome()
	{
		_welcomePopups.gameObject.SetActive(false);
	}

	//public async Task<int> ShowRollDicePopup()
	//{
	//	return await _rollDicePopup.ShowAsync(false, false, true);
	//}

	//public async Task ShowChallengePopup()
	//{
	//	await _challengePopup.ShowAsync(true, false, false);
	//}

	//public void ShowVictoryPopup()
	//{
	//	_victoryPopup.ShowAsync(false, false, true);
	//}
	#endregion
}
