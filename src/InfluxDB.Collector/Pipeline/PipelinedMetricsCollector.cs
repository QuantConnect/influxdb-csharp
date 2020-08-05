
using System;
using System.Collections.Generic;
using InfluxDB.LineProtocol.Payload;

namespace InfluxDB.Collector.Pipeline
{
    class PipelinedMetricsCollector : MetricsCollector
    {
        readonly IPointEmitter _emitter;
        readonly IPointEnricher _enricher;
        readonly Action _dispose;

        public PipelinedMetricsCollector(IPointEmitter emitter, IPointEnricher enricher, Action dispose)
        {
            _emitter = emitter;
            _enricher = enricher;
            _dispose = dispose;
        }

        protected override void Emit(List<IPointData> points)
        {
            if (_enricher != null)
            {
                foreach (var point in points)
                    _enricher.Enrich(point);
            }

            _emitter.Emit(points);
        }

        protected override void Emit(IPointData point)
        {
            _enricher?.Enrich(point);

            _emitter.Emit(point);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _dispose();
        }
    }
}
