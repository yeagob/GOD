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

    private bool _answer;

    // Start is called before the first frame update
    void Start()
    {
        _yesButton.onClick.AddListener(YesButton);

        _noButton.onClick.AddListener(NoButton);
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

    public async Task<bool> ShowAsync(Player currentPlayer)
	{
        _titleText.text = currentPlayer.Name + " ha completado el desafio???";
        _descriptionText.text = currentPlayer.CurrentTile.TileData.challenge.description;
        gameObject.SetActive(true);

        while (gameObject.activeSelf)
            await Task.Yield();

        return _answer;
    }
}
