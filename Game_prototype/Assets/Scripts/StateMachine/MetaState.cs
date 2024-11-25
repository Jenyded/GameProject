using Core;
using Services;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace StateMachine
{
    public class MetaState : GameStateBase<MetaState.Model>
    {
        public class Model
        {
        }

        public MetaState(GameMachine stateMachine, AssetReference metaScene)
        {
            _stateMachine = stateMachine;
            _metaScene = metaScene;
        }

        private readonly CompositeDisposable _stateDisposable = new();
        private readonly GameMachine _stateMachine;
        private readonly AssetReference _metaScene;

        protected override void Init(Model stateModel)
        {
            Observable.ReturnUnit()
                .ContinueWith(_ => BundleService.PreloadBundle(EntryPoint.SharedServices.MonoTarget, _metaScene, EntryPoint.SharedServices.Progress))
                .ContinueWith(_ => BundleService.LoadScene(_metaScene))
                .SafeSubscribe(_ => OnSceneLoaded())
                .AddTo(_stateDisposable);
        }

        protected override void Deinit()
        {
            _stateDisposable.Clear();
        }
        
        private void OnSceneLoaded()
        {
            var metaPoint = GameObject.FindObjectOfType<MetaPoint>();

            metaPoint
                .Init(new MetaPoint.Model(OnCompleted))
                .AddTo(_stateDisposable);

            void OnCompleted(AssetReference battleScene)
            {
                EntryPoint.SharedServices.WindowsService.ToggleLoading(true);
                EntryPoint.SharedServices.WindowsService.CloseAllWindows();
                _stateMachine.ChangeState<BattleState>(new BattleState.Model(battleScene));
            }
            
            EntryPoint.SharedServices.WindowsService.ToggleLoading(false);
            EntryPoint.SharedServices.WindowsService.Open(EntryPoint.SharedServices.WindowsResolver.GetInfoWindowModel("Hello in meta!"));
        }
    }
}