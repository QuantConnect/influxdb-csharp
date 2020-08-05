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

        public void Emit(List<IPointData> points)
        {
            for (var i = 0; i < _emitters.Count; i++)
            {
                _emitters[i].Emit(points);
            }
        }

        public void Emit(IPointData point)
        {
            foreach (var emitter in _emitters)
                emitter.Emit(point);
        }
    }
}