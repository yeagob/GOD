using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PublishBoardPopup : MonoBehaviour
{
	[SerializeField] private Button _okButton;
	[SerializeField] private Button _cancelButton;
	[SerializeField] private TMP_InputField _mailInputText;

	private bool _ok = false;

	void Start()
	{
		_okButton.onClick.AddListener(Close);
		_cancelButton.onClick.AddListener(Cancel);
	}

	private void Cancel()
	{
		_mailInputText.text = "";
		Close();
	}

	private void Close()
	{
		gameObject.SetActive(false);
	}

	internal async Task<string> ShowAsync()
	{		
		if (PlayerPrefs.HasKey("mail"))
			_mailInputText.text = PlayerPrefs.GetString("mail");

		_ok = false;
		gameObject.SetActive(true);

		while (gameObject.activeSelf)
			await Task.Yield();

		if (_mailInputText.text != "")
			PlayerPrefs.SetString("mail", _mailInputText.text);


		return _mailInputText.text;
	}

    // Update is called once per frame
    void Update()
    {
		if (_mailInputText.text.Contains("@"))
			_okButton.interactable = true;
		else
			_okButton.interactable = false;
	}
}
