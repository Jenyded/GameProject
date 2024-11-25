using System;
using UniRx;
using UnityEngine;

namespace Core
{
    public class DisposableBehaviour<T> : MonoBehaviour
    {
        protected T ActiveModel { get; private set; }
        protected CompositeDisposable Disposables;

        public IDisposable Init(T model)
        {
            Disposables = new CompositeDisposable();
            ActiveModel = model;
            OnInit();
            return Disposables;
        }

        protected virtual void OnInit()
        {
        }
    }
}

