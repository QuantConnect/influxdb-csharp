using System.Collections.Generic;
using InfluxDB.LineProtocol.Payload;

namespace InfluxDB.Collector.Pipeline
{
    interface IPointEmitter
    {
        void Emit(List<IPointData> points);

        void Emit(IPointData point);
    }
}
