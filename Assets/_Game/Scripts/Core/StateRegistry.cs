namespace _Game.Scripts.Core
{
    public class StateRegistry
    {
        public IState State { get; }
        public StateHandle Handle { get; }
        
        public StateRegistry(IState state, StateHandle handle)
        {
            State = state;
            Handle = handle;
        }
    }
}