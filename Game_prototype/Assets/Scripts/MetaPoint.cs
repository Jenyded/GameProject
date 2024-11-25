using System;
using Core;
using UniRx;
using UnityEngine.AddressableAssets;

public class MetaPoint : DisposableBehaviour<MetaPoint.Model>
{
    public class Model
    {
        public readonly Action<AssetReference> OnCompleted;
        
        public Model(Action<AssetReference> onCompleted)
        {
            OnCompleted = onCompleted;
        }
    }

    protected override void OnInit()
    {
        base.OnInit();

        var sceneReference = EntryPoint.SharedServices.GameData.battleSceneReference;

        Observable
            .Timer(TimeSpan.FromSeconds(3f))
            .SafeSubscribe(_ => ActiveModel.OnCompleted(sceneReference))
            .AddTo(Disposables);
    }
}