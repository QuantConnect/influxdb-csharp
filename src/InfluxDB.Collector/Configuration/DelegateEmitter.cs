using System;
using System.Collections.Generic;
using InfluxDB.Collector.Pipeline;
using InfluxDB.LineProtocol.Payload;

namespace InfluxDB.Collector.Configuration
{
    class DelegateEmitter : IPointEmitter
    {
        readonly Action<List<IPointData>> _emitter;

        public DelegateEmitter(Action<List<IPointData>> emitter)
        {
            if (emitter == null) throw new ArgumentNullException(nameof(emitter));
            _emitter = emitter;
        }

        public void Emit(List<IPointData> points)
        {
            _emitter(points);
        }

        public void Emit(IPointData point)
        {
            _emitter(new List<IPointData> { point });
        }
    }
}