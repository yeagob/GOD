using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class WelcomePopups : MonoBehaviour
{
    //Panels
    [SerializeField] private GameObject _initialPanel;
    [SerializeField] private QuestionPanel _questionPanel;
    [SerializeField] private GameObject _creatingPanel;

    //Buttons
    [SerializeField] private Button _letsgoButton;

    //Answers
    private string _answer = "";
    private string _answer2  = "";

	private void Awake()
	{
        _letsgoButton.onClick.AddListener(CloseInitial);
        _initialPanel.SetActive(false);
        _creatingPanel.SetActive(false);
    }

    internal async Task<string> ShowAsync()
	{
        gameObject.SetActive(true);

        _initialPanel.SetActive(true);

        while (_initialPanel.activeSelf)
            await Task.Yield();

		_answer = await _questionPanel.ShowQuestion();

        //TODO: Show Board Config
        _creatingPanel.SetActive(true);

        return _answer;
	}

	private void CloseInitial()
	{
		_initialPanel.SetActive(false);
	}
}
