using System.Collections.Generic;
using InfluxDB.LineProtocol.Payload;

namespace InfluxDB.Collector.Pipeline
{
    class NullMetricsCollector : MetricsCollector
    {
        protected override void Emit(List<IPointData> points)
        {
        }

        protected override void Emit(IPointData point)
        {
        }
    }
}
