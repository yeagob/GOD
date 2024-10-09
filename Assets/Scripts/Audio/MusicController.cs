using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    [SerializeField] private AudioClip _clipDrumBass;
    [SerializeField] private AudioClip _clipRock;
    [SerializeField] private AudioClip _clipBase;

    private AudioSource _audioSource;
    // Start is called before the first frame update
    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void PlayDrumBass()
	{
       // _audioSource.clip = _clipDrumBass;
        _audioSource.Play();
    }   
    public void PlayBase()
	{
       // _audioSource.clip = _clipBase;
        _audioSource.Play();
    }    
    public void PlayRock()
	{
       // _audioSource.clip = _clipRock;
        _audioSource.Play();
    }
}
