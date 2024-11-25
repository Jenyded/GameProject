using System;
using Core;
using Services;
using UniRx;
using UnityEngine;

public class BattlePoint : DisposableBehaviour<BattlePoint.Model>
{
    public class Model
    {
        public readonly Action OnCompleted;
        
        public Model(Action onCompleted)
        {
            OnCompleted = onCompleted;
        }
    }

    protected override void OnInit()
    {
        base.OnInit();

        var cubeReference = EntryPoint.SharedServices.GameData.characterData.prefabReference;

        BundleService
            .LoadBundle<GameObject>(cubeReference)
            .SafeSubscribe(prefab =>
            {
                var instance = GameObject.Instantiate(prefab);
                
                instance.transform.position = Vector3.zero;
                instance.transform.rotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;

                Disposable
                    .Create(() => GameObject.Destroy(instance.gameObject))
                    .AddTo(Disposables);
            })
            .AddTo(Disposables);

        Observable
            .Timer(TimeSpan.FromSeconds(3f))
            .SafeSubscribe(_ => ActiveModel.OnCompleted())
            .AddTo(Disposables);
    }
}