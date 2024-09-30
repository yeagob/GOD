using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateOrSelectBoardPopup : MonoBehaviour
{
    [SerializeField] private Button _createButton;
    [SerializeField] private Button _chooseButton;

    private bool _createMode;

    // Start is called before the first frame update
    void Start()
    {
        _createButton.onClick.AddListener(CreateMode);
		_chooseButton.onClick.AddListener(ChooseMode);
    }

	private void ChooseMode()
	{
        _createMode = false;
		gameObject.SetActive(false);
	}

    private void CreateMode()
	{
        _createMode = true;
		gameObject.SetActive(false);
    }

    public async Task<bool> ShowAsync()
    {
        gameObject.SetActive(true);

        while (gameObject.activeSelf)
            await Task.Yield();

        return _createMode;

    }
}
