using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class RollDicePopup : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI _diceText;
	private float _showingTime = 1;//s

	public async Task ShowAsync(int diceValue)
	{
		gameObject.SetActive(true);
		_diceText.text = diceValue.ToString();
		float time = _showingTime;
		while (time > 0)
		{
			time -= Time.deltaTime;
			await Task.Yield();
		}

		gameObject.SetActive(false);


	}
}
