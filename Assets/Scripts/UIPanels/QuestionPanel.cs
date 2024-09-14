using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestionPanel : MonoBehaviour
{
	const string QUIESTION1 = "Escribe de que quieres que vaya el juego. Algo que te apasione o que quieras mejorar en la vida";
	const string QUIESTION2 = "Escribe con quien vas a jugar esta partida. Puedes jugar solo, pon tu nombre en ese caso.";

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

	internal async Task<string> ShowQuestionOne()
	{
		gameObject.SetActive(true);
		_questionText.text = QUIESTION1;
		_answerInput.text = string.Empty;
		while (gameObject.activeSelf)
			await Task.Yield();

		return _answerInput.text;
	}

	internal async Task<string> ShowQuestionTwo()
	{
		gameObject.SetActive(true);

		_questionText.text = QUIESTION2;
		_answerInput.text = string.Empty;

		while (gameObject.activeSelf)
			await Task.Yield();

		return _answerInput.text;

	}
}
