using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Game.Scripts.Core.Features
{
    [CreateAssetMenu(fileName = "SoundEffectAsset", menuName = "Omedya/SoundEffect Asset")]
    public class SoundEffectAsset : ScriptableObject, IPlayableSound
    {
        [SerializeField] private AudioClip audioClip;
        
        public async UniTask PlaySound(AudioSource audioSource, CancellationToken cancellationToken = default)
        {
            audioSource.clip = audioClip;
            audioSource.Play();
            
            var isCanceled = await UniTask.WaitWhile(() => audioSource.isPlaying, cancellationToken: cancellationToken).SuppressCancellationThrow();
            
            // Check if audio playback was cancelled
            if (isCanceled)
            {
                audioSource.Stop(); // Stop the audio if the operation was cancelled
            }
            
            
            
            
        }
        
    }
}