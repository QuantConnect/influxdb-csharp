using InfluxDB.LineProtocol.Payload;

namespace InfluxDB.Collector.Pipeline
{
    interface IPointEnricher
    {
        void Enrich(IPointData pointData);
    }
}
