using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

/// <summary>
/// Позволяет выполнять код на главном потоке Unity из фоновых потоков.
/// Добавь на любой GameObject в сцене (например, на тот же объект, что и UnityRacingServer).
/// </summary>
public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher _instance;
    private static readonly object _lock = new object();
    private readonly Queue<Action> _queue = new Queue<Action>();
    private readonly object _queueLock = new object();

    public static UnityMainThreadDispatcher Instance()
    {
        if (_instance != null) return _instance;
        lock (_lock)
        {
            if (_instance != null) return _instance;
            var go = new GameObject("UnityMainThreadDispatcher");
            _instance = go.AddComponent<UnityMainThreadDispatcher>();
            DontDestroyOnLoad(go);
        }
        return _instance;
    }

    /// <summary>Поставить действие в очередь на выполнение в главном потоке.</summary>
    public void Enqueue(Action action)
    {
        if (action == null) return;
        lock (_queueLock)
        {
            _queue.Enqueue(action);
        }
    }

    /// <summary>Выполнить функцию на главном потоке и дождаться результата.</summary>
    public T EnqueueAndWait<T>(Func<T> func)
    {
        if (func == null) return default;
        var ev = new ManualResetEventSlim(false);
        T result = default;
        Exception ex = null;
        lock (_queueLock)
        {
            _queue.Enqueue(() =>
            {
                try
                {
                    result = func();
                }
                catch (Exception e)
                {
                    ex = e;
                }
                finally
                {
                    ev.Set();
                }
            });
        }
        ev.Wait();
        if (ex != null) throw ex;
        return result;
    }

    /// <summary>Выполнить действие на главном потоке и дождаться завершения.</summary>
    public void EnqueueAndWait(Action action)
    {
        EnqueueAndWait<object>(() => { action(); return null; });
    }

    private void Update()
    {
        Action a = null;
        lock (_queueLock)
        {
            if (_queue.Count > 0)
                a = _queue.Dequeue();
        }
        a?.Invoke();
    }
}
