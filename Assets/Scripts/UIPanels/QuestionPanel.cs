using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestionPanel : MonoBehaviour
{
	const string QUIESTION1 = "Escribe detalladamente de que quiers que vaya el juego.";

	[SerializeField] private TextMeshProUGUI _questionText;
	[SerializeField] private TMP_InputField _answerInput;
	[SerializeField] private Button _answerButton;
	private void Awake()
	{
		_answerButton.onClick.AddListener(Done);
		gameObject.SetActive(false);
	}

	private void Done()
	{
		gameObject.SetActive(false);
	}

	internal async Task<string> ShowQuestion()
	{
		gameObject.SetActive(true);
		_questionText.text = QUIESTION1;
		_answerInput.text = string.Empty;

		while (gameObject.activeSelf)
			await Task.Yield();

		return _answerInput.text;
	}
}
