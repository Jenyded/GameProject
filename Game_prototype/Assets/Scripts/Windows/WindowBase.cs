using System;
using UniRx;
using UnityEngine;

namespace Windows
{
    public abstract class WindowBase<T> : WindowBase
    {
        public sealed override Type ModelType => typeof(T);
        protected T ActiveModel { get; private set; }

        protected readonly CompositeDisposable Disposables = new();

        public sealed override void Open(object model)
        {
            ActiveModel = (T)model;

            OnOpen();
            
            gameObject.SetActive(true);
        }

        public sealed override void Close()
        {
            Disposables.Clear();

            ActiveModel = default;
            
            gameObject.SetActive(false);
        }

        protected abstract void OnOpen();
    }

    public abstract class WindowBase : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;
        public abstract Type ModelType { get; }
        public void SetOrder(int order)
        {
            _canvas.sortingOrder = order;
        }

        public abstract void Open(object model);
        public abstract void Close();

    }
}