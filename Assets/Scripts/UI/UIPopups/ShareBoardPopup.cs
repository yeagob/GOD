using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShareBoardPopup : MonoBehaviour
{
	[SerializeField] private Button _okButton;
	[SerializeField] private TMP_InputField _urlInputText;

	void Start()
	{
		_okButton.onClick.AddListener(OkButton);
	}

	private void OkButton()
	{
		gameObject.SetActive(false);
	}

	internal async Task ShowAsync(string url)
	{
		gameObject.SetActive(true);
		_urlInputText.text = url;

		while (gameObject.activeSelf)
			await Task.Yield();

	}
}
