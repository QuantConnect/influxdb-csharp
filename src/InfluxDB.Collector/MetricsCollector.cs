using System;
using System.Collections.Generic;
using InfluxDB.Collector.Diagnostics;
using InfluxDB.Collector.Pipeline;
using InfluxDB.LineProtocol.Payload;

namespace InfluxDB.Collector
{
    public abstract class MetricsCollector : IPointEmitter, IDisposable
    {
        readonly Util.ITimestampSource _timestampSource = new Util.PseudoHighResTimestampSource();

        public void Increment(string measurement, long count = 1, KeyValuePair<string, string>[] tags = null)
        {
            Write(measurement, new[] { new KeyValuePair<string, object>("count", count) }, tags);
        }

        public void Measure(string measurement, object value, KeyValuePair<string, string>[] tags = null)
        {
            Write(measurement, new[] { new KeyValuePair<string, object>("value", value) }, tags);
        }

        public IDisposable Time(string measurement, KeyValuePair<string, string>[] tags = null)
        {
            return new StopwatchTimer(this, measurement, tags);
        }

        public CollectorConfiguration Specialize()
        {
            return new CollectorConfiguration(this);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) { }

        public void Write(string measurement, KeyValuePair<string, object>[] fields, KeyValuePair<string, string>[] tags = null, DateTime? timestamp = null)
        {
            try
            {
                var point = new PointData(measurement, fields, tags, timestamp ?? _timestampSource.GetUtcNow());
                Emit(point);
            }
            catch (Exception ex)
            {
                CollectorLog.ReportError("Failed to write point", ex);
            }
        }

        void IPointEmitter.Emit(IPointData[] points)
        {
            Emit(points);
        }

        void IPointEmitter.Emit(IPointData point)
        {
            Emit(point);
        }

        protected abstract void Emit(IPointData[] points);
        protected abstract void Emit(IPointData point);
    }
}
