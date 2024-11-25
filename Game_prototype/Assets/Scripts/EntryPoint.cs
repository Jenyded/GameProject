using System;
using Windows;
using Core;
using Data;
using Services;
using StateMachine;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class EntryPoint : MonoBehaviour, ISharedServices
{
    [SerializeField] private AssetReference _gameDataReference;
    [SerializeField] private WindowsService _windowsService;

    public static ISharedServices SharedServices { get; private set; }
    public GameData GameData { get; private set; }
    public WindowsResolver WindowsResolver { get; private set; }
    public WindowsService WindowsService => _windowsService;
    public MonoBehaviour MonoTarget => this;
    
    private readonly CompositeDisposable _rootDisposable = new();
    
    public void Progress(float progress)
    {
        SharedServices.WindowsService.ReportProgress(progress);
    }
    
    private void Awake()
    {
        SharedServices = this;
        SharedServices.WindowsService.ToggleLoading(true);
        
        Observable
            .ReturnUnit()
            .ContinueWith(_ => LoadData())
            .ContinueWith(_ => LoadWindows())
            .SafeSubscribe(_ => InitGame())
            .AddTo(_rootDisposable);
    }

    private IObservable<Unit> LoadData()
    {
        var subject = new Subject<Unit>();
        
        Observable.ReturnUnit()
            .ContinueWith(_ => BundleService.PreloadBundle(SharedServices.MonoTarget, _gameDataReference, SharedServices.Progress))
            .ContinueWith(_ => BundleService.LoadBundle<GameData>(_gameDataReference))
            .SafeSubscribe(gameData =>
            {
                GameData = gameData;
                subject.OnNext(Unit.Default);
                subject.OnCompleted();
            })
            .AddTo(_rootDisposable);

        return subject;
    }
    
    private IObservable<Unit> LoadWindows()
    {
        var subject = new Subject<Unit>();
        
        Observable.ReturnUnit()
            .ContinueWith(_ => BundleService.PreloadBundle(SharedServices.MonoTarget, SharedServices.GameData.windowsDataReference, SharedServices.Progress))
            .ContinueWith(_ => BundleService.LoadBundle<WindowsData>(SharedServices.GameData.windowsDataReference))
            .SafeSubscribe(windowsData =>
            {
                _windowsService
                    .Init(new WindowsService.Model(windowsData))
                    .AddTo(_rootDisposable);

                WindowsResolver = new WindowsResolver();
                
                subject.OnNext(Unit.Default);
                subject.OnCompleted();
            })
            .AddTo(_rootDisposable);

        return subject;
    }

    
    private void InitGame()
    {
        var stateMachine = new GameMachine();
        stateMachine.Init().AddTo(_rootDisposable);

        stateMachine.AddState(new MetaState(stateMachine, SharedServices.GameData.metaSceneReference));
        stateMachine.AddState(new BattleState(stateMachine));
        
        stateMachine.ChangeState<MetaState>(new MetaState.Model());
    }

    private void OnDestroy()
    {
        _rootDisposable.Clear();
    }
}

public interface ISharedServices
{
    void Progress(float value);
    MonoBehaviour MonoTarget { get; }
    GameData GameData { get; }
    WindowsService WindowsService { get; }
    WindowsResolver WindowsResolver { get; }
}

