using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestionPopup : MonoBehaviour
{
    [SerializeField] private Button[] _answersButton;
    [SerializeField] private TextMeshProUGUI[] _answersText;
    [SerializeField] private TextMeshProUGUI _questionText;
    [SerializeField] private TextMeshProUGUI _resultText;
    [SerializeField] private AudioClip _errorSound;
    [SerializeField] private AudioClip _successSound;

    private bool _isCorrectAnswer;
    private QuestionData _currentQuestion;

    // Start is called before the first frame update
    void Start()
    {
        _answersButton[0].onClick.AddListener(Answer0);
        _answersButton[1].onClick.AddListener(Answer1);
        _answersButton[2].onClick.AddListener(Answer2);
        _answersButton[3].onClick.AddListener(Answer3);
    }

    private void Answer0()
    {
        ProcessAnswer(0);
    }

    private void Answer1()
    {
        ProcessAnswer(1);
    }

    private void Answer2()
    {
        ProcessAnswer(2);
    }

    private void Answer3()
    {
        ProcessAnswer(3);
    }

    private void ProcessAnswer(int index)
    {
        if (index == _currentQuestion.correctId)
        {
            _isCorrectAnswer = true;
            _resultText.text = "Correcto! Tira de nuevo!";
            _resultText.color = Color.blue;
        }
        else
        {
            _isCorrectAnswer = false;
            _resultText.text = "No has acertado, pierdes un turno...";
            _resultText.color = Color.red;
        }

        ShowResultAndClose();
    }

    private void ShowResultAndClose()
    {
        _resultText.gameObject.SetActive(true);
        _resultText.transform.localScale = Vector3.zero;

        // Animar de pequeño a grande (scale=1)
        _resultText.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            // Esperar 2 segundos
            DOVirtual.DelayedCall(2f, () =>
            {
                // Animar de grande a pequeño
                _resultText.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    _resultText.gameObject.SetActive(false);
                    gameObject.SetActive(false);
                });
            });
        });
    }


    public async Task<bool> ShowAsync(QuestionData question)
    {
        gameObject.SetActive(true);
        _isCorrectAnswer = false;
        _currentQuestion = question;

        // Set question and answers
        _questionText.text = question.statement;
        for (int i = 0; i < _answersText.Length; i++)
        {
            if (i < question.options.Count)
            {
                _answersText[i].text = question.options[i];
            }
            else
            {
                _answersButton[i].gameObject.SetActive(false);
            }
        }

        // Hide the result text initially
        _resultText.gameObject.SetActive(false);

        // Wait until the game object is deactivated
        while (gameObject.activeSelf)
            await Task.Yield();

        return _isCorrectAnswer;
    }
}


