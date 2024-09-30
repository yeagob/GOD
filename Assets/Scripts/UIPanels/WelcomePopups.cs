using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;


//TODO: Change to Generic Image Popup?
public class WelcomePopups : MonoBehaviour
{
	//Panels
	[SerializeField] private GameObject _initialPanel;

	//Buttons
	[SerializeField] private Button _letsgoButton;

	private void Awake()
	{
		_letsgoButton.onClick.AddListener(CloseInitial);
	}

	private void CloseInitial()
	{
		_initialPanel.SetActive(false);
	}


	internal async Task ShowAsync()
	{
		gameObject.SetActive(true);
		_initialPanel.SetActive(true);

		while (_initialPanel.activeSelf)
			await Task.Yield();

	}
}


