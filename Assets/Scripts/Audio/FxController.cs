using God.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class FxController : MonoBehaviour
{
    [SerializeField] GameController _gameController;
    [SerializeField] AudioMixerGroup _group;
    [SerializeField] AudioClip _fart;
    [SerializeField] AudioClip _jump;
    [SerializeField] AudioClip _cuack;
    [SerializeField] AudioClip _loseTurn;
    [SerializeField] AudioClip _rollDices;

	// Start is called before the first frame update
	void Start()
    {
        _gameController.OnCuack += Cuack;
        _gameController.OnHappy += Happy;
        _gameController.OnSad += Sad;
        _gameController.OnRollDices += RollDices;
        _gameController.OnGameStarts += StartGame;
	}

	private void RollDices()
	{
		AudioPoolManager.PlaySound(_rollDices, _group,1, 0.7f);
	}

	private void StartGame()
	{
		_gameController.BoardController.OnMoveStep += TokenStep;		
	}

	private void TokenStep()
	{
		AudioPoolManager.PlaySound(_fart, _group,1);
	}

	private void Sad()
	{
		AudioPoolManager.PlaySound(_loseTurn, _group,0.7f);
	}

	private void Happy()
	{
		AudioPoolManager.PlaySound(_jump, _group, 1);

	}

	private void Cuack()
	{
		AudioPoolManager.PlaySound(_cuack, _group, 0.7f);
	}
}
