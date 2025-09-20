using System.Collections.Concurrent;
using System.Threading;
using _Game.Scripts.Core.Features;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Game.Scripts.Core.Extensions
{
    public static class PlayableSoundExtensions
    {
        private static readonly ConcurrentQueue<AudioSource> AudioSourcePool = new();
        
        private static AudioSource RetrieveNewAudioSource()
        {
            var audioSource = new GameObject("AudioSource").AddComponent<AudioSource>();
            audioSource.gameObject.hideFlags = HideFlags.HideAndDontSave;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0;
            return audioSource;
        }
        
        public static async UniTask PlaySound(this IPlayableSound sound, CancellationToken cancellationToken = default)
        {
            if (!AudioSourcePool.TryDequeue(out var audioSource))
            {
                audioSource = RetrieveNewAudioSource();
            }
            
            await sound.PlaySound(audioSource, cancellationToken);
            
            AudioSourcePool.Enqueue(audioSource);
        }
    }
}