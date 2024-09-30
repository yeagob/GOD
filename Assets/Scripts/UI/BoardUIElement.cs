using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoardUIElement : MonoBehaviour
{
	[SerializeField] private Image _thumbnailImage;
	[SerializeField] private TMP_Text _titleText;
	[SerializeField] private Toggle _toggle;
	[SerializeField] private GameObject _selectedFeeback;

	private BoardData _boardData;

	public event Action <BoardData> OnBoardSelected;

	public void Initialize(BoardData data, Sprite image, ToggleGroup group)
	{
		_boardData = data;

		_titleText.text = data.tittle;
		
		_thumbnailImage.sprite = image;

		_toggle.onValueChanged.AddListener(OnToggleValueChanged);
		_toggle.group = group;

		gameObject.SetActive(true);
	}

	private void OnToggleValueChanged(bool isOn)
	{
		if (isOn)
		{
			OnBoardSelected?.Invoke(_boardData);
			_selectedFeeback.SetActive(true);
		}
		else
		{
			_selectedFeeback.SetActive(false);
		}
	}

	public void SetIsOn(bool isOn)
	{
		_toggle.isOn = isOn;
	}
}
