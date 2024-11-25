using System;
using UniRx;

namespace Core
{
    public static class ReactiveExtensions
    {
        public static IDisposable EmptySubscribe<T>(this IObservable<T> source) => source.SafeSubscribe(_ => { });

        public static IDisposable SafeSubscribe<T>(this IObservable<T> source, Action<T> action)
        {
            return source.Catch<T, Exception>(exception =>
            {
                UnityEngine.Debug.LogException(exception);
                return Observable.Return<T>(default);
            }).Subscribe(action);
        }
    }
}