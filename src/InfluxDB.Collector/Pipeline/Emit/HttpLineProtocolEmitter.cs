using System;
using System.Collections.Generic;
using System.Threading;
using InfluxDB.Collector.Diagnostics;
using InfluxDB.LineProtocol.Client;
using InfluxDB.LineProtocol.Payload;

namespace InfluxDB.Collector.Pipeline.Emit
{
    class HttpLineProtocolEmitter : IDisposable, IPointEmitter
    {
        readonly ILineProtocolClient _client;

        private static DateTime _lastReport;
        private static int _globalCounter;
        private static int _emitCount;

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

            Interlocked.Add(ref _globalCounter, points.Count / 1000);

            if (!influxResult.Success)
            {
                CollectorLog.ReportError(influxResult.ErrorMessage, null);
            }
            else
            {
                if (Interlocked.Increment(ref _emitCount) % 500 == 0)
                {
                    var now = DateTime.UtcNow;
                    if (now - _lastReport > TimeSpan.FromSeconds(10))
                    {
                        _lastReport = now;
                        CollectorLog.Report($"Influxdb pushed: {_globalCounter}K");
                    }
                }
            }
        }

        public void Emit(IPointData point)
        {
            throw new NotImplementedException();
        }
    }
}
