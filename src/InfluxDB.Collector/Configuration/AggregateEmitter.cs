using System;
using System.Collections.Generic;
using InfluxDB.Collector.Pipeline;
using InfluxDB.LineProtocol.Payload;

namespace InfluxDB.Collector.Configuration
{
    class AggregateEmitter : IPointEmitter
    {
        readonly List<IPointEmitter> _emitters;

        public AggregateEmitter(List<IPointEmitter> emitters)
        {
            if (emitters == null) throw new ArgumentNullException(nameof(emitters));
            _emitters = emitters;
        }

        public void Emit(IPointData[] points)
        {
            foreach (var emitter in _emitters)
                emitter.Emit(points);
        }

        public void Emit(IPointData point)
        {
            foreach (var emitter in _emitters)
                emitter.Emit(point);
        }
    }
}