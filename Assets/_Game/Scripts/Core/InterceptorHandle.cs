namespace _Game.Scripts.Core
{
    public class InterceptorHandle
    {
        private readonly IStateContainer _stateContainer;
        private readonly BaseStateInterceptor _interceptor;
        private bool _isRemoved;
        
        public InterceptorHandle(IStateContainer stateContainer, BaseStateInterceptor interceptor)
        {
            _stateContainer = stateContainer;
            _interceptor = interceptor;
            _isRemoved = false;
        }
        
        public void Remove()
        {
            if (_isRemoved) return;
            
            _isRemoved = true;
            _stateContainer.RemoveInterceptor(_interceptor);
        }
    }
}