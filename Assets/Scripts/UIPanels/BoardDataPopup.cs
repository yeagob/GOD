using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoardDataPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _tittleText;
    [SerializeField] private TextMeshProUGUI _proposalText;
    [SerializeField] private Button _playButton;


    // Start is called before the first frame update
    void Start()
    {
        _playButton.onClick.AddListener(ClosePanel);
    }

	private void ClosePanel()
	{
        gameObject.SetActive(false);
    }

    public async Task ShowAsync(BoardData board)
    {
        _tittleText.text = board.name;
        _proposalText.text = board.proposal;
        gameObject.SetActive(true);

        while (gameObject.activeSelf)
            await Task.Yield();
    }
}
