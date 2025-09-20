using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Game.Scripts.Core.Features
{
    public interface IPlayableSound
    {
        UniTask PlaySound(AudioSource audioSource, CancellationToken cancellationToken = default);
    }
}