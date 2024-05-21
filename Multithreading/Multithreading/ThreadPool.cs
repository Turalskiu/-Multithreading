using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Multithreading
{
    public class ThreadPool : IDisposable
    {
        public bool IsDisposed { get; private set; }

        private readonly Thread[] _threads;
        private readonly Queue<Action> _actions;

        private readonly object _syncRoot = new object();

        public ThreadPool(int maxThreads = 4)
        {
            _threads = new Thread[maxThreads];
            _actions = new Queue<Action>();
            for(int i = 0; i < _threads.Length; ++i) {
                _threads[i] = new Thread(ThreadProc) 
                {
                    IsBackground = true,
                    Name         = $"MyThreadPool Thread {i}",
                };
                _threads[i].Start();
            }
        }


        public void Queue(Action action)
        {
            Monitor.Enter(_syncRoot);
            try
            {
                _actions.Enqueue(action);
                if(_actions.Count == 1 ) 
                {
                    Monitor.Pulse(_syncRoot);
                }
            }
            finally
            {
                Monitor.Exit(_syncRoot);
            }
        }

        private void ThreadProc()
        {
            while (true)
            {
                Action action;
                Monitor.Enter(_syncRoot);
                try
                {
                    if(IsDisposed) return;
                    if(_actions.Count > 0)
                    {
                        action = _actions.Dequeue();
                    }
                    else
                    {
                        Monitor.Wait(_syncRoot);
                        continue;
                    } 
                }
                finally
                {
                    Monitor.Exit(_syncRoot);
                }
                action();
            }
        }

        public void Dispose()
        {
            if(!IsDisposed)
            {
                bool isDisposing = false;
                Monitor.Enter(_syncRoot);
                try
                {
                    if (!IsDisposed)
                    {
                        IsDisposed = true;
                        Monitor.Pulse(_syncRoot);
                        isDisposing = true;
                    }
                }
                finally 
                { 
                    Monitor.Exit(_syncRoot); 
                }

                //ждем пока прихлопнутся все потоки
                if (isDisposing)
                {
                    for(int i = 0; i < _threads.Length; i++)
                    {
                        _threads[i].Join();
                    }
                }
            }
        }
    }
}
