using GOD.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    #region Fields

    [SerializeField] private GameController _gameController;

    [Header("UI BUttons")]
    [SerializeField] private Button _quitGameButton;
    [SerializeField] private Button _restartGameButton;
    [SerializeField] private Button _editPlayersButton;
    [SerializeField] private Button _editBoardButton;
    [SerializeField] private Button _shareButton;
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
        _shareButton.onClick.AddListener(OnShareClicked);
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
        _shareButton.interactable = false;
        _extraButton.interactable = false;
        
        if (GameController.GameState == GameStateState.Playing)
		{
            _quitGameButton.interactable = true;
            _restartGameButton.interactable = true;
            _editPlayersButton.interactable = true;
           // _editBoardButton.interactable = true;
           // _shareButton.interactable = true;
        }

        if (GameController.GameState == GameStateState.Editing)
        {
            _quitGameButton.interactable = true;
        }
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
        ClosePanel();
        Debug.Log("Edit Board button clicked");
    }

    private void OnShareClicked()
    {
        Debug.Log("Share button clicked");
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
