using DG.Tweening;
using GOD.Utils;//NOP!!
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestionPopup : MonoBehaviour
{
    [SerializeField] private Button[] _answersButton;
    [SerializeField] private TextMeshProUGUI[] _answersText;
    [SerializeField] private TextMeshProUGUI _questionText;
    [SerializeField] private AudioClip _errorSound;
    [SerializeField] private AudioClip _successSound;

    private bool _isCorrectAnswer;
    private QuestionData _currentQuestion;
    private PopupsController _popupsController;

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
        ProcessAnswer(0).WrapErrors();
    }

    private void Answer1()
    {
        ProcessAnswer(1).WrapErrors();
    }

    private void Answer2()
    {
        ProcessAnswer(2).WrapErrors();
    }

    private void Answer3()
    {
        ProcessAnswer(3).WrapErrors();
    }

    private async Task ProcessAnswer(int index)
    {
        if (index == _currentQuestion.correctId)
        {
            _isCorrectAnswer = true;
            await _popupsController.ShowGenericMessage("Correcto!\n Tira de nuevo!", 2, Color.white);
        }
        else
        {
            _isCorrectAnswer = false;
            await _popupsController.ShowGenericMessage("Has Fallado...\n Pasa turno!", 2, Color.black);
        }

        gameObject.SetActive(false);
    }

    public async Task<bool> ShowAsync(QuestionData question, PopupsController popupsController)
    {
        _popupsController = popupsController;

        gameObject.SetActive(true);
        _isCorrectAnswer = false;
        _currentQuestion = question;

        // Set question and answers
        _questionText.text = question.statement;
        for (int i = 0; i < _answersText.Length; i++)
        {
            if (i < question.options.Length)
            {
                _answersText[i].text = question.options[i];
            }
            else
            {
                _answersButton[i].gameObject.SetActive(false);
            }
        }

        // Wait until the game object is deactivated
        while (gameObject.activeSelf)
            await Task.Yield();

        return _isCorrectAnswer;
    }
}


