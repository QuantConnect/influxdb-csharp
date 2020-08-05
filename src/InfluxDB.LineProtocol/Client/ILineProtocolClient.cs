using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.LineProtocol.Payload;

namespace InfluxDB.LineProtocol.Client
{
    public interface ILineProtocolClient
    {
        Task<LineProtocolWriteResult> SendAsync(LineProtocolWriter lineProtocolWriter, CancellationToken cancellationToken = default);
        LineProtocolWriteResult WriteAsync(List<IPointData> payload, CancellationToken cancellationToken = default);
        Task<LineProtocolWriteResult> WriteAsync(string payload, CancellationToken cancellationToken = default);
    }
}