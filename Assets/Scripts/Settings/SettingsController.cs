using GOD.Utils;
using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    #region Fields

    [SerializeField] private GameController _gameController;

    [Header("UI BUttons")]
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _quitGameButton;
    [SerializeField] private Button _restartGameButton;
    [SerializeField] private Button _editPlayersButton;
    [SerializeField] private Button _editBoardButton;
    [SerializeField] private Button _rerollButton;
    [SerializeField] private Button _extraButton;

    [SerializeField] private Button _okButton;
    #endregion

    #region Unity Callbacks

    // Start is called before the first frame update
    void Start()
    {
        _quitGameButton.onClick.AddListener(OnQuitGameClicked);
        _restartGameButton.onClick.AddListener(OnRestartGameClicked);
        _editPlayersButton.onClick.AddListener(OnEditPlayersClicked);
        _editBoardButton.onClick.AddListener(OnEditBoardClicked);
        _rerollButton.onClick.AddListener(OnRerollClicked);
        _extraButton.onClick.AddListener(OnExtraClicked);
        _okButton.onClick.AddListener(ClosePanel);

        InvokeRepeating(nameof(CheckGameState), 1, 1);
    }

    #endregion

    #region private Methods

    private void CheckGameState()
    {
        _quitGameButton.interactable = false;
        _restartGameButton.interactable = false;
        _editPlayersButton.interactable = false;
        _editBoardButton.interactable = false;
        _rerollButton.interactable = false;
        _extraButton.interactable = false;
        
        if (GameController.GameState == GameStateState.Playing)
		{
            _quitGameButton.interactable = true;
            _restartGameButton.interactable = true;
            _editPlayersButton.interactable = true;
            _editBoardButton.interactable = true;
            _rerollButton.interactable = true;
        }

        if (GameController.GameState == GameStateState.Editing)
        {
            _rerollButton.interactable = true;
            _quitGameButton.interactable = true;
        }

        if (gameObject.activeSelf) 
			_settingsButton.gameObject.SetActive(false);
        else
			_settingsButton.gameObject.SetActive(true);

	}

    private void OnQuitGameClicked()
    {
        _gameController.BackToMainMenu();
        ClosePanel();
    }

    private void OnRestartGameClicked()
    {
        _gameController.RestartGame();
        ClosePanel();
    }

    private void OnEditPlayersClicked()
    {
        _gameController.EditPlayers().WrapErrors();
        ClosePanel();
    }

    private void OnEditBoardClicked()
    {
        _gameController.EnterEditMode();
		ClosePanel();
    }

    private void OnRerollClicked()
    {
        _gameController.RerollGame().WrapErrors();
        ClosePanel();
    }

    private void OnExtraClicked()
    {
        Debug.Log("Extra button clicked");
    }


    private void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    #endregion
}
