using System.Threading;
using Cysharp.Threading.Tasks;

namespace _Game.Scripts.Core
{
    public interface IState
    {
        UniTask ExecuteAsync(CancellationToken cancellationToken = default);
        
    }
}