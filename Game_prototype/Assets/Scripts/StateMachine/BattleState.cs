using Core;
using Services;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace StateMachine
{
    public class BattleState : GameStateBase<BattleState.Model>
    {
        public class Model
        {
            public readonly AssetReference BattleScene;
            
            public Model(AssetReference battleScene)
            {
                BattleScene = battleScene;
            }
        }

        public BattleState(GameMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        private readonly CompositeDisposable _stateDisposable = new();
        private readonly GameMachine _stateMachine;

        protected override void Init(Model stateModel)
        {
            Observable.ReturnUnit()
                .ContinueWith(_ => BundleService.PreloadBundle(EntryPoint.SharedServices.MonoTarget, stateModel.BattleScene, EntryPoint.SharedServices.Progress))
                .ContinueWith(_ => BundleService.LoadScene(stateModel.BattleScene))
                .SafeSubscribe(_ => OnSceneLoaded())
                .AddTo(_stateDisposable);
        }

        protected override void Deinit()
        {
            _stateDisposable.Clear();
        }

        private void OnSceneLoaded()
        {
            var battlePoint = GameObject.FindObjectOfType<BattlePoint>();

            battlePoint
                .Init(new BattlePoint.Model(OnCompleted))
                .AddTo(_stateDisposable);

            void OnCompleted()
            {
                EntryPoint.SharedServices.WindowsService.ToggleLoading(true);
                EntryPoint.SharedServices.WindowsService.CloseAllWindows();
                _stateMachine.ChangeState<MetaState>(new MetaState.Model());
            }

            EntryPoint.SharedServices.WindowsService.ToggleLoading(false);
            EntryPoint.SharedServices.WindowsService.Open(EntryPoint.SharedServices.WindowsResolver.GetInfoWindowModel("Hello in battle!"));
        }
    }
}