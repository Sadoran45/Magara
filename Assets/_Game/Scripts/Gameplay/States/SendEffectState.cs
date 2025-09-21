using System;
using System.Threading;
using _Game.Scripts.Core;
using _Game.Scripts.Core.Components;
using _Game.Scripts.Gameplay.Characters;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Game.Scripts.Gameplay.States
{
    public class SendEffectState : IState
    {
        [Serializable]
        public class Config
        {
            public GameObject shieldSendEffectPrefab;
        }
        public class Data
        {
            public PlayerMotor Target { get; }
            
            public Data(PlayerMotor target)
            {
                Target = target;
            }
        }
        
        private readonly PlayerMotor _owner;
        private readonly Config _config;
        public Data StateData { get; }
        public SendEffectState(PlayerMotor owner, Config config, Data data)
        {
            _owner = owner;
            _config = config;
            StateData = data;
        }
        
        public async UniTask ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var shieldObject = Object.Instantiate(_config.shieldSendEffectPrefab, _owner.transform.position,
                Quaternion.identity);

            await MoveShieldToAvenger(shieldObject);
        }

        private async UniTask MoveShieldToAvenger(GameObject shieldEffect)
        {
            var t = 0f;
            while (t <= 1f)
            {
                var position = BezierFollower.EvaluateCubicBezier(t, _owner.ToCoreCurveHolder.p0.position,
                    _owner.ToCoreCurveHolder.p1.position, _owner.ToCoreCurveHolder.p2.position,
                    _owner.ToCoreCurveHolder.p3.position);
                shieldEffect.transform.position = position;
                
                await UniTask.Yield();
                
                t += Time.deltaTime;
            }

            while (t >= 0f)
            {
                var position = BezierFollower.EvaluateCubicBezier(t, StateData.Target.ToCoreCurveHolder.p0.position,
                    StateData.Target.ToCoreCurveHolder.p1.position, StateData.Target.ToCoreCurveHolder.p2.position,
                    StateData.Target.ToCoreCurveHolder.p3.position);
                
                shieldEffect.transform.position = position;
                await UniTask.Yield();
                
                t -= Time.deltaTime;
            }
            
            // TODO: Dissolve shield effect
            Object.Destroy(shieldEffect);
            
            // Notify the target that the effect has arrived
            if (StateData.Target is AvengerCharacter avengerCharacter)
            {
                avengerCharacter.ReceiveShield(3f).Forget();
            }
        }
    }
}