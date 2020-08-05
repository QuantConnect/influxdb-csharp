using System;
using System.Collections.Generic;
using InfluxDB.Collector.Diagnostics;
using InfluxDB.Collector.Platform;
using InfluxDB.LineProtocol.Payload;

namespace InfluxDB.Collector.Pipeline.Batch
{
    class IntervalBatcher : IPointEmitter, IDisposable
    {
        bool _unloading;
        readonly int _maxBatchSize;
        readonly PortableTimer _timer;
        readonly IPointEmitter _parent;
        private object _queueLock;
        private List<List<IPointData>> _pool;
        private volatile Queue<IPointData> _queue;

        public IntervalBatcher(TimeSpan interval, int? maxBatchSize, IPointEmitter parent)
        {
            var emitters = 4;
            _parent = parent;
            _queueLock = new object();
            _maxBatchSize = maxBatchSize ?? 5000;
            _pool = new List<List<IPointData>>();
            for (var i = 0; i < emitters; i++)
            {
                // we create a buffer for each emitter
                _pool.Add(new List<IPointData>(_maxBatchSize));
            }
            _queue = new Queue<IPointData>();
            _timer = new PortableTimer(OnTick, interval, emittersCount: emitters);
        }

        void CloseAndFlush()
        {
            if (_unloading)
                return;
            _unloading = true;

            _timer.Dispose();

            OnTick(-1);
        }

        public void Dispose()
        {
            CloseAndFlush();
        }

        void OnTick(int id)
        {
            try
            {
                int count;
                Queue<IPointData> queue;
                lock (_queueLock)
                {
                    count = _queue.Count;
                    if (count == 0)
                    {
                        // short cut
                        return;
                    }
                    queue = _queue;
                    _queue = new Queue<IPointData>();
                }

                if (count > _maxBatchSize * 1000)
                {
                    CollectorLog.ReportError($"InfluxDB.IntervalBatcher(): OnTick() Batch.Count: {count} lagging", null);
                }

                var batch = id != -1 ? _pool[id] : new List<IPointData>();
                do
                {
                    for (var i = 0; i < queue.Count && i < _maxBatchSize; i++)
                    {
                        batch.Add(queue.Dequeue());
                    }
                    _parent.Emit(batch);
                    batch.Clear();
                } while (queue.Count > 0);
            }
            catch (Exception ex)
            {
                CollectorLog.ReportError("Failed to emit metrics batch", ex);
            }
        }

        public void Emit(List<IPointData> points)
        {
            lock (_queueLock)
            {
                for (var i = 0; i < points.Count; i++)
                {
                    _queue.Enqueue(points[i]);
                }
            }
        }

        public void Emit(IPointData point)
        {
            lock(_queueLock)
            {
                _queue.Enqueue(point);
            }
        }
    }
}
