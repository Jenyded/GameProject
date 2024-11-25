namespace StateMachine
{
    public interface IGameState
    {
        void Init(object data);
        void Deinit();
    }

    public abstract class GameStateBase<T> : IGameState where T : class
    {
        void IGameState.Init(object data) => Init(data as T);
        void IGameState.Deinit() => Deinit();
        protected abstract void Init(T stateModel);
        protected abstract void Deinit();
    }
}