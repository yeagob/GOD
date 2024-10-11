using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
	#region Fields

	[SerializeField] private AudioMixer _audioMixer;
	[SerializeField] private Slider _musicVolumeSlider;
	[SerializeField] private Slider _fxVolumeSlider;

	#endregion

	#region Unity Callbacks

	private void Start()
	{
		// Set default slider values based on current AudioMixer values
		float musicVolume, fxVolume;
		_audioMixer.GetFloat("MusicVolume", out musicVolume);
		_audioMixer.GetFloat("FXVolume", out fxVolume);

		// Convert dB to linear for the sliders (0-1 range)
		_musicVolumeSlider.value = Mathf.Pow(10, musicVolume / 20);
		_fxVolumeSlider.value = Mathf.Pow(10, fxVolume / 20);

		// Add listeners to sliders
		_musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
		_fxVolumeSlider.onValueChanged.AddListener(SetFXVolume);
	}

	private void OnDisable()
	{
		PlayerPrefs.SetFloat("MusicVolume", _musicVolumeSlider.value);
		PlayerPrefs.SetFloat("FXVolume", _fxVolumeSlider.value);
	}
	#endregion

	#region Public Methods

	/// <summary>
	/// Adjusts the music volume based on the slider value
	/// </summary>
	public void SetMusicVolume(float value)
	{
		float volumeInDb = value == 0 ? -50 : Mathf.Log10(value) * 20;
		_audioMixer.SetFloat("MusicVolume", volumeInDb);
	}

	/// <summary>
	/// Adjusts the FX volume based on the slider value
	/// </summary>
	public void SetFXVolume(float value)
	{
		float volumeInDb = value == 0? -50 : Mathf.Log10(value) * 20;
		_audioMixer.SetFloat("FXVolume", volumeInDb);
	}

	internal void Initialize()
	{
		if (PlayerPrefs.HasKey("MusicVolume"))
		{
			_fxVolumeSlider.value = PlayerPrefs.GetFloat("FXVolume");
			_musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume");
			SetFXVolume(_fxVolumeSlider.value);
			SetMusicVolume(_musicVolumeSlider.value);
		}
	}

	#endregion
}
