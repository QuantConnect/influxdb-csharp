using InfluxDB.LineProtocol.Payload;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IO;

namespace InfluxDB.LineProtocol.Client
{
    public abstract class LineProtocolClientBase : ILineProtocolClient
    {
        private static RecyclableMemoryStreamManager MemoryManager = new RecyclableMemoryStreamManager();
        [ThreadStatic] private static Guid _guid;

        protected readonly string _database, _username, _password, _retentionPolicy;

        protected LineProtocolClientBase(Uri serverBaseAddress, string database, string username, string password, string retentionPolicy)
        {
            if (serverBaseAddress == null)
                throw new ArgumentNullException(nameof(serverBaseAddress));
            if (string.IsNullOrEmpty(database))
                throw new ArgumentException("A database must be specified");

            // Overload that allows injecting handler is protected to avoid HttpMessageHandler being part of our public api which would force clients to reference System.Net.Http when using the lib.
            _database = Uri.EscapeDataString(database);
            _username = username == null ? null : Uri.EscapeDataString(username);
            _password = password == null ? null : Uri.EscapeDataString(password);
            _retentionPolicy = retentionPolicy == null ? null : Uri.EscapeDataString(retentionPolicy);
        }

        public LineProtocolWriteResult WriteAsync(List<IPointData> points, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_guid == default)
            {
                // let's re use the guid per thread
                _guid = new Guid();
            }

            using (var stream = MemoryManager.GetStream(_guid))
            using (var writer = new StreamWriter(stream) { AutoFlush = false })
            {
                LineProtocolPayload.Format(writer, points);
                writer.Flush();

                LineProtocolWriteResult result = default;
                var eventComplete = new ManualResetEvent(false);

                WriteAsync(stream.ToArray(), cancellationToken).ContinueWith(task =>
                {
                    result = task.Result;
                    eventComplete.Set();
                }, cancellationToken);

                // avoid task.Wait which spins
                eventComplete.WaitOne();
                return result;
            }
        }

        public Task<LineProtocolWriteResult> WriteAsync(byte[] payload, CancellationToken cancellationToken = default(CancellationToken))
        {
            return OnSendAsync(payload, Precision.Nanoseconds, cancellationToken);
        }

        public Task<LineProtocolWriteResult> SendAsync(LineProtocolWriter lineProtocolWriter, CancellationToken cancellationToken = default(CancellationToken))
        {
            return OnSendAsync(Encoding.UTF8.GetBytes(lineProtocolWriter.ToString()), lineProtocolWriter.Precision, cancellationToken);
        }

        protected abstract Task<LineProtocolWriteResult> OnSendAsync(
            byte[] payload,
            Precision precision,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
