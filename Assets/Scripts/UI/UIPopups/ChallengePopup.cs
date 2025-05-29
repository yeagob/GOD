using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class ChallengePopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private Button _yesButton;
    [SerializeField] private Button _noButton;
    [SerializeField] private Button _okButton;

    private bool _answer;

    // Start is called before the first frame update
    void Start()
    {
        _yesButton.onClick.AddListener(YesButton);
        _noButton.onClick.AddListener(NoButton);
        _okButton.onClick.AddListener(ClosePanel);
    }

	private void NoButton()
	{
        _answer = false;
        ClosePanel();
	}

	private void YesButton()
	{
        _answer = true;
        ClosePanel();
    }

    private void ClosePanel()
	{
        gameObject.SetActive(false);
    }

    public async Task<bool> ShowAsync(Player currentPlayer, bool firstTime)
	{
        if (firstTime)
		{
            _noButton.gameObject.SetActive(false);
            _yesButton.gameObject.SetActive(false);
            _okButton.gameObject.SetActive(true);
            _titleText.text = currentPlayer.Name + " NO vuelvas a juegar hasta que completes el siguiente desafio: ";
		}
		else
		{
            _noButton.gameObject.SetActive(true);
            _yesButton.gameObject.SetActive(true);
            _okButton.gameObject.SetActive(false);
            _titleText.text = currentPlayer.Name + " has completado el desafio? \n Solo te mientes a ti mismx.";
		}

        _descriptionText.text = currentPlayer.CurrentTile.TileData.challenge.description;
        gameObject.SetActive(true);

        while (gameObject.activeSelf)
            await Task.Yield();

        return _answer;
    }
}
