using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class WelcomePopups : MonoBehaviour
{
    //Panels
    [SerializeField] private GameObject _initialPanel;
    [SerializeField] private QuestionPanel _questionPanel;

    //Buttons
    [SerializeField] private Button _letsgoButton;

    //Answers
    private string _answer1;
    private string _answer2;

	private void Awake()
	{
        _letsgoButton.onClick.AddListener(CloseInitial);
	}

	internal async Task<string[]> ShowAsync()
	{
        _initialPanel.SetActive(true);

        while (_initialPanel.activeSelf)
            await Task.Yield();

		_answer1 = await _questionPanel.ShowQuestionOne();
		_answer2 = await _questionPanel.ShowQuestionTwo();

        string[] answers = new string[2]
        {
            _answer1, _answer2
        };

        return answers;
	}

	private void CloseInitial()
	{
		_initialPanel.SetActive(false);
	}
}
