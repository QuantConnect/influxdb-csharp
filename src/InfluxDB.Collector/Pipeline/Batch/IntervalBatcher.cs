using System;
using System.Collections.Generic;
using System.Linq;
using InfluxDB.Collector.Diagnostics;
using InfluxDB.Collector.Platform;
using InfluxDB.Collector.Util;

namespace InfluxDB.Collector.Pipeline.Batch
{
    class IntervalBatcher : IPointEmitter, IDisposable
    {
        bool _unloading;
        object _queueLock;
        readonly int? _maxBatchSize;
        readonly PortableTimer _timer;
        readonly IPointEmitter _parent;
        private Queue<PointData> _queue;

        public IntervalBatcher(TimeSpan interval, int? maxBatchSize, IPointEmitter parent)
        {
            _parent = parent;
            _queueLock = new object();
            _maxBatchSize = maxBatchSize;
            _queue = new Queue<PointData>();
            _timer = new PortableTimer(cancel => OnTick(), interval);
        }

        void CloseAndFlush()
        {
            lock (_queueLock)
            {
                if (_unloading)
                    return;
                _unloading = true;
            }

            _timer.Dispose();

            OnTick();
        }

        public void Dispose()
        {
            CloseAndFlush();
        }

        void OnTick()
        {
            try
            {
                int count;
                do
                {
                    Queue<PointData> batch;
                    lock (_queueLock)
                    {
                        if (_queue.Count == 0)
                        {
                            // short cut
                            return;
                        }

                        batch = _queue;
                        _queue = new Queue<PointData>();
                    }

                    if (batch.Count > _maxBatchSize * 5)
                    {
                        CollectorLog.ReportError($"InfluxDB.IntervalBatcher(): OnTick() Batch.Count: {batch.Count} lagging", null);
                    }

                    if (_maxBatchSize == null || batch.Count <= _maxBatchSize.Value)
                    {
                        _parent.Emit(batch.ToArray());
                    }
                    else
                    {
                        foreach (var chunk in batch.Batch(_maxBatchSize.Value))
                        {
                            _parent.Emit(chunk.ToArray());
                        }
                    }

                    lock (_queueLock)
                    {
                        count = _queue.Count;
                    }
                    // if there is enough work let's directly loop again
                } while (_maxBatchSize.HasValue && count >= _maxBatchSize.Value);
            }
            catch (Exception ex)
            {
                CollectorLog.ReportError("Failed to emit metrics batch", ex);
            }
        }

        public void Emit(PointData[] points)
        {
            lock (_queueLock)
            {
                foreach (var point in points)
                    _queue.Enqueue(point);
            }
        }

        public void Emit(PointData point)
        {
            lock (_queueLock)
            {
                _queue.Enqueue(point);
            }
        }
    }
}
