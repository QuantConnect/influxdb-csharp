using System;
using InfluxDB.LineProtocol.Payload;

namespace InfluxDB.Collector.Configuration
{
    public abstract class CollectorEmitConfiguration
    {
        public abstract CollectorConfiguration InfluxDB(Uri serverBaseAddress, string database, string username = null, string password = null, bool enableCompression = false, string retentionPolicy = null);

        public CollectorConfiguration InfluxDB(string serverBaseAddress, string database, string username = null, string password = null, bool enableCompression = false, string retentionPolicy = null)
        {
            return InfluxDB(new Uri(serverBaseAddress), database, username, password, enableCompression, retentionPolicy);
        }

        public abstract CollectorConfiguration Emitter(Action<IPointData[]> emitter);
    }
}
