using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditBoardPopup : MonoBehaviour
{
    [Header("Header")]
    [SerializeField] private TMP_InputField _tittleText;
    [SerializeField] private TMP_InputField _proposalText;
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _backButton;

    [Header("Questions-Chalenges Slider ")]
    [SerializeField] private Slider _questionsChallengesSlider;

    //Hacer Sub Controllers
    [Header("Questions")]

    //Hacer Sub Controllers
    [Header("Challenges")]


    //TODO: Move to Controller??
    private GameData _gameData = new GameData();

    // Start is called before the first frame update
    void Start()
    {		
		_playButton.onClick.AddListener(Play);
		_backButton.onClick.AddListener(Back);
    }

	private void Back()
	{
		_gameData = null;
		gameObject.SetActive(false);
	}

    private void Play()
	{
		_back = false;
		gameObject.SetActive(false);
    }

    public async Task<bool> ShowAsync(BoardData board)
    {
		GameController.GameState = GameStateState.Editing;

		gameObject.SetActive(true);

        _tittleText.text = board.tittle;
        _proposalText.text = board.proposal;

        while (gameObject.activeSelf)
            await Task.Yield();

        return _back;
    }
}
