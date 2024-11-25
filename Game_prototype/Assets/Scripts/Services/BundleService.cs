using System;
using System.Collections;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Services
{
    public class BundleService
    {
        #if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/ClearBundleCache")]
        private static void ClearBundleCache()
        {
            UnityEngine.Caching.ClearCache();
        }
        #endif
        
        public static IObservable<Unit> LoadScene(AssetReference reference)
        {
            return SceneLoadingRoutine(reference.RuntimeKey.ToString());
        }

        public static IObservable<Unit> LoadScene(string address)
        {
            return SceneLoadingRoutine(address);
        }
        
        public static IObservable<T> LoadBundle<T>(AssetReference reference) where T : UnityEngine.Object
        {
            return BundleLoadingRoutine<T>(reference.RuntimeKey.ToString());
        }
		
        public static IObservable<T> LoadBundle<T>(string address) where T : UnityEngine.Object
        {
            return BundleLoadingRoutine<T>(address);
        }
        
        public static IObservable<Unit> PreloadBundle(MonoBehaviour target, AssetReference reference, Action<float> progress)
        {
            return PreloadBundle(target, reference.RuntimeKey.ToString(), progress);
        }
        
        public static IObservable<Unit> PreloadBundle(MonoBehaviour target, string address, Action<float> progress)
        {
            var subject = new Subject<Unit>();

            void OnCompleted()
            {
                subject.OnNext(Unit.Default);
                subject.OnCompleted();
            }
            
            target.StartCoroutine(
                ProgressBundleRoutine(
                    OnCompleted, progress, address));
            
            return subject;
        }
        
        private static IObservable<Unit> SceneLoadingRoutine(string address)
        {
            return Observable.Create<Unit>(subject =>
            {
                void OnCompleted(AsyncOperationHandle<SceneInstance> operation)
                {
                    if (operation.Status == AsyncOperationStatus.Succeeded)
                    {
                        SceneManager.SetActiveScene(operation.Result.Scene);
                        subject.OnNext(Unit.Default);
                    }
                    else
                    {
                        Debug.LogError($"Scene {address} failed to load.");
                    }
                }

                var handler = Addressables.LoadSceneAsync(address, LoadSceneMode.Additive);
                handler.Completed += OnCompleted;

                return Disposable.Create(() =>
                {
                    handler.Completed -= OnCompleted;
                    Addressables.UnloadSceneAsync(handler);
                });
            });
        }
        
        private static IObservable<T> BundleLoadingRoutine<T>(string address) where T : UnityEngine.Object
        {
            return Observable.Create<T>(subject =>
            {
                void OnCompleted(AsyncOperationHandle<T> obj)
                {
                    if (obj.Status == AsyncOperationStatus.Succeeded)
                    {
                        subject.OnNext(obj.Result);
                    }
                    else
                    {
                        Debug.LogError($"Bundle {address} failed to load.");
                        subject.OnNext(null);
                    }
                }

                var handler = Addressables.LoadAssetAsync<T>(address);
                handler.Completed += OnCompleted;

                return Disposable.Create(() =>
                {
                    handler.Completed -= OnCompleted;
                    Addressables.Release(handler);
                });
            });
        }
        
        private static IEnumerator ProgressBundleRoutine(Action onCompleted, Action<float> progress, string address)
        {
            var total = 0f;
            var handler = Addressables.DownloadDependenciesAsync(address);

            while (handler.Status == AsyncOperationStatus.None)
            {
                var current = handler
                    .GetDownloadStatus()
                    .Percent;
                
                if (total < current)
                {
                    total = current;
                    progress?.Invoke(total);
                }

                yield return null;
            }

            Addressables.Release(handler);
            onCompleted?.Invoke();
        }
    }
}
