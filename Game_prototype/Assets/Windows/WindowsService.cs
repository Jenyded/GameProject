using System;
using System.Collections.Generic;
using Core;
using Data;
using UniRx;
using UnityEngine;

namespace Windows
{
    public class WindowsService : DisposableBehaviour<WindowsService.Model>
    {
        public class Model
        {
            public readonly WindowsData WindowsData;
            
            public Model(WindowsData windowsData)
            {
                WindowsData = windowsData;
            }
        }

        [SerializeField] private Transform _windowAnchor;
        [SerializeField] private LoadingWindow _loadingWindow;

        private const int EnvironmentOrder = 1;
        private readonly List<WindowBase> _instances = new();
        private readonly Stack<WindowBase> _windowsStack = new();

        public IObservable<Unit> ObserveWindowOpen<T>() => CurrentWindow
            .Where(window => window != null && window.GetType() == typeof(T))
            .Select(_ => _).First().AsUnitObservable();

        public IObservable<Unit> ObserveWindowClose<T>() => CurrentWindow.Pairwise()
            .Where(pair => pair.Previous != null && pair.Previous.GetType() == typeof(T))
            .Select(_ => _).First().AsUnitObservable();

        public IObservable<Unit> ObserveWindowFullClose<T>()
        {
            return _closeObservable
                .Where(closingWindowType => closingWindowType == typeof(T)).First()
                .AsUnitObservable();
        }

        public IReadOnlyReactiveProperty<WindowBase> CurrentWindow => _currentWindow;

        private readonly ReactiveCommand<Type> _closeObservable = new();
        private readonly ReactiveProperty<WindowBase> _currentWindow = new();

        private readonly CompositeDisposable _loadingDisposable = new();

        public void ToggleLoading(bool isActive)
        {
            _loadingWindow.gameObject.SetActive(isActive);
        }

        public void ReportProgress(float progress)
        {
            _loadingWindow.ReportProgress(progress);
        }

        public void Open(object model, bool isDisplacing = true)
        {
            if (isDisplacing)
            {
                while (_windowsStack.Count > 0)
                {
                    var closingWindow = _windowsStack.Pop();
                    closingWindow.Close();
                    _closeObservable.Execute(closingWindow.GetType());
                }
            }

            var currentWindow = GetOrCreateWindow(model.GetType());
            currentWindow.SetOrder(_windowsStack.Count + EnvironmentOrder);
            currentWindow.Open(model);
            _windowsStack.Push(currentWindow);

            OnWindowsQueueChanged();
        }

        public void Close()
        {
            if (_windowsStack.Count > 0)
            {
                var currentWindow = _windowsStack.Pop();
                currentWindow.Close();
                _closeObservable.Execute(currentWindow.GetType());
                OnWindowsQueueChanged();
            }
        }

        public void CloseAllWindows()
        {
            var queueChanged = false;

            while (_windowsStack.Count > 0)
            {
                queueChanged = true;
                var closingWindow = _windowsStack.Pop();
                closingWindow.Close();
                _closeObservable.Execute(closingWindow.GetType());
            }

            if (queueChanged)
                OnWindowsQueueChanged();
        }

        protected override void OnInit()
        {
            Disposable
                .Create(OnDisposed)
                .AddTo(Disposables);
        }

        private WindowBase GetOrCreateWindow(Type modelType)
        {
            var instance = _instances.Find(instanceArg => instanceArg.ModelType == modelType);

            if (instance == null)
                instance = CreateWindowInstance(modelType);

            return instance;
        }

        private WindowBase CreateWindowInstance(Type modelType)
        {
            var prefab = ActiveModel.WindowsData.prefabs
                .Find(windowInstanceArg => windowInstanceArg.ModelType == modelType);

            var instance = GameObject.Instantiate(prefab, _windowAnchor);
            _instances.Add(instance);

            return instance;
        }

        private void OnDisposed()
        {
            _loadingDisposable.Clear();

            while (_windowsStack.Count > 0)
                Close();

            foreach (var instance in _instances)
                GameObject.Destroy(instance.gameObject);

            _instances.Clear();
        }

        private void OnWindowsQueueChanged()
        {
            _currentWindow.Value = _windowsStack.Count > 0
                ? _windowsStack.Peek()
                : null;
        }
    }
}
