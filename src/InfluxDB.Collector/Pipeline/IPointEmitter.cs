using InfluxDB.LineProtocol.Payload;

namespace InfluxDB.Collector.Pipeline
{
    interface IPointEmitter
    {
        void Emit(IPointData[] points);

        void Emit(IPointData point);
    }
}
