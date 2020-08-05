using System;
using System.Collections.Generic;
using InfluxDB.Collector.Diagnostics;
using InfluxDB.LineProtocol.Client;
using InfluxDB.LineProtocol.Payload;

namespace InfluxDB.Collector.Pipeline.Emit
{
    class HttpLineProtocolEmitter : IDisposable, IPointEmitter
    {
        readonly ILineProtocolClient _client;

        public HttpLineProtocolEmitter(ILineProtocolClient client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            _client = client;
        }

        public void Dispose()
        {
             // This needs to ensure outstanding operations have completed
        }

        public void Emit(List<IPointData> points)
        {
            var influxResult = _client.WriteAsync(points);
            if (!influxResult.Success)
                CollectorLog.ReportError(influxResult.ErrorMessage, null);
        }

        public void Emit(IPointData point)
        {
            throw new NotImplementedException();
        }
    }
}
