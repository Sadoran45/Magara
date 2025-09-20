using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Game.Scripts.Core.Features
{
    public class BackgroundMusicManager : MonoBehaviour
    {
        #region Singleton
        
        private static BackgroundMusicManager _instance;
        public static BackgroundMusicManager Instance
        {
            get
            {
                if (_instance) return _instance;
                
                _instance = FindAnyObjectByType<BackgroundMusicManager>();
                if (_instance) return _instance;
                
                var go = new GameObject("BackgroundMusicManager");
                _instance = go.AddComponent<BackgroundMusicManager>();
                DontDestroyOnLoad(go);

                return _instance;
            }
        }

        #endregion

        #region AudioSource

        private AudioSource _audioSource;
        private AudioSource AudioSource
        {
            get
            {
                if (_audioSource == null)
                {
                    _audioSource = gameObject.AddComponent<AudioSource>();
                }

                return _audioSource;
            }
        }

        #endregion
        
        private string _currentMusicPath;

        private void Awake()
        {
            if(_instance && _instance != this) Destroy(gameObject);
        }

        private async UniTask<AudioClip> LoadAudioClip(string path)
        {
            path = $"Musics/{path}";
            var operation = Resources.LoadAsync<AudioClip>(path);
            
            await operation;
            
            var audioClip = operation.asset as AudioClip;
            
            if (!audioClip)
            {
                throw new Exception($"AudioClip not found at path: {path}");
            }
            
            return audioClip;
        }
        
        public async UniTaskVoid PlayMusic(string path, float volume = 1f)
        {
            if (_currentMusicPath == path) return;
            
            _currentMusicPath = path;
            
            var audioClip = await LoadAudioClip(path);
            
            PlayMusic(audioClip, volume);
        }
        public void PlayMusic(AudioClip audioClip, float volume = 1f)
        {
            AudioSource.clip = audioClip;
            AudioSource.loop = true;

            var realVolume = volume * 0.05f;
            AudioSource.volume = realVolume;
            
            AudioSource.Play();
        }

        public void StopMusic()
        {
            AudioSource.Stop();
        }
    }
}