using System;
using InfluxDB.Collector.Pipeline;
using InfluxDB.LineProtocol.Payload;

namespace InfluxDB.Collector.Configuration
{
    class DelegateEmitter : IPointEmitter
    {
        readonly Action<IPointData[]> _emitter;

        public DelegateEmitter(Action<IPointData[]> emitter)
        {
            if (emitter == null) throw new ArgumentNullException(nameof(emitter));
            _emitter = emitter;
        }

        public void Emit(IPointData[] points)
        {
            _emitter(points);
        }

        public void Emit(IPointData point)
        {
            _emitter(new [] { point });
        }
    }
}