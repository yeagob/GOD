using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CreateBoardQuestionPopup : MonoBehaviour
{
	[SerializeField] private float _minLength = 3;
	[SerializeField] private Button _okButton;
	[SerializeField] private TMP_InputField _promptInputText;

	void Start()
	{
		_okButton.onClick.AddListener(OkButton);
	}

	private void OkButton()
	{
		gameObject.SetActive(false);
	}

	internal async Task<string> ShowAsync()
	{
		gameObject.SetActive(true);

		while (gameObject.activeSelf)
			await Task.Yield();

		return _promptInputText.text;
	}

    // Update is called once per frame
    void Update()
    {
		if (_promptInputText.text.Length >= 3)
			_okButton.gameObject.SetActive(true);
		else
			_okButton.gameObject.SetActive(false);


	}
}
