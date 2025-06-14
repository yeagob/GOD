using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;

public class PlayerTurnPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _messageText;
    [SerializeField] private Button _readyButton;

    // Start is called before the first frame update
    void Start()
    {
        _readyButton.onClick.AddListener(Close);
    }

	private void Close()
	{
        gameObject.SetActive(false);

    }

    public async Task ShowAsync(Player player)
	{
        _nameText.text = player.Name;
        _nameText.color = player.Token.Color;
        gameObject.SetActive(true);

        if (player.State == PlayerState.PlayAgain)
            _messageText.text = "Tira de nuevo!!";
        else
            _messageText.text = "Es el turno de:";

        while (gameObject.activeSelf)
            await Task.Yield();
    }
}
