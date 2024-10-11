using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;

namespace God.Utils
{

    public static class AudioPoolManager
    {
		#region Fields

		private static GameObject _audioPoolObject;
        private static List<AudioSource> _audioSources = new List<AudioSource>();

		#endregion

		#region Public Methods

		public static void PlaySound(AudioClip clip, AudioMixerGroup group= null, float volume = 1, float pitch = 1)
        {
            AudioSource source = GetFreeAudioSource();
            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;
			source.outputAudioMixerGroup = group;

			source.Play();
        }
        #endregion

        #region Private Methods

        private static AudioSource GetFreeAudioSource()
        {
            // Buscar un AudioSource libre
            foreach (AudioSource source in _audioSources)
            {
                if (!source.isPlaying )
                {
                    return source;
                }
            }

            // Si no hay ninguno libre, crear uno nuevo
            return CreateNewAudioSource();
        }

        private static AudioSource CreateNewAudioSource()
        {
            // Crear el GameObject si aún no existe
            if (_audioPoolObject == null)
            {
                _audioPoolObject = new GameObject("Audio Pool");
                GameObject.DontDestroyOnLoad(_audioPoolObject);
			}

            // Crear un nuevo AudioSource
            AudioSource newSource = _audioPoolObject.AddComponent<AudioSource>();
            _audioSources.Add(newSource);			

			return newSource;
        }

        #endregion

    }
}
