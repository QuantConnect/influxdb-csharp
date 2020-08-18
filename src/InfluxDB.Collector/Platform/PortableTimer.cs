// Based on code:
// Copyright 2013-2016 Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Threading;
using InfluxDB.Collector.Diagnostics;

namespace InfluxDB.Collector.Platform
{
    class PortableTimer : IDisposable
    {
        bool _disposed;
        TimeSpan _interval;
        readonly List<Thread> _threads;
        readonly CancellationTokenSource _cancel;
        readonly Action<int> _onTick;

        public PortableTimer(Action<int> onTick, TimeSpan interval, int emittersCount = 4)
        {
            _interval = interval;
            _cancel = new CancellationTokenSource();
            _threads = new List<Thread>(emittersCount);
            _onTick = onTick ?? throw new ArgumentNullException(nameof(onTick));

            for (var i = 0; i < emittersCount; i++)
            {
                var myId = i;
                _threads.Add(new Thread(() => OnTick(myId)) { IsBackground = true });
                _threads[i].Start();
            }
        }

        void OnTick(int id)
        {
            while (!_cancel.IsCancellationRequested)
            {
                try
                {
                    Thread.Sleep(_interval);
                    if (!_cancel.IsCancellationRequested)
                    {
                        _onTick(id);
                    }
                }
                catch (OperationCanceledException tcx)
                {
                    CollectorLog.ReportError("The timer was canceled during invocation", tcx);
                }
                catch (Exception exception)
                {
                    if (!_cancel.IsCancellationRequested)
                    {
                        CollectorLog.ReportError("Unknown exception in timer", exception);
                    }
                }
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _cancel.Cancel();
            _disposed = true;
            foreach (var thread in _threads)
            {
                thread.Join(1000);
            }
        }
    }
}
