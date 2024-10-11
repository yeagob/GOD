using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class RollDicePopup : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI _diceText;

	public async Task ShowAsync(int diceValue, float showingTime = 1)
	{
		gameObject.SetActive(true);
		_diceText.text = diceValue.ToString();
		float time = showingTime;
		while (time > 0)
		{
			time -= Time.deltaTime;
			await Task.Yield();
			if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
			{
				break;
			}
		}

		gameObject.SetActive(false);


	}
}
