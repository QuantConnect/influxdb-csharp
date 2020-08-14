using InfluxDB.LineProtocol.Payload;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InfluxDB.LineProtocol.Client
{
    public abstract class LineProtocolClientBase : ILineProtocolClient
    {
        private Queue<StringBuilder> _stringBuilders;
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
            _stringBuilders = new Queue<StringBuilder>();
        }

        public LineProtocolWriteResult WriteAsync(List<IPointData> points, CancellationToken cancellationToken = default(CancellationToken))
        {
            var stringBuilder = RentStringBuilder();

            var stringWriter = new StringWriter(stringBuilder);
            LineProtocolPayload.Format(stringWriter, points);
            var payload = stringWriter.ToString();

            ReturnStringBuilder(stringBuilder);

            LineProtocolWriteResult result = default;
            var eventComplete = new ManualResetEvent(false);

            WriteAsync(payload, cancellationToken).ContinueWith(task =>
            {
                result = task.Result;
                eventComplete.Set();
            }, cancellationToken);

            // avoid task.Wait which spins
            eventComplete.WaitOne();
            return result;
        }

        public Task<LineProtocolWriteResult> WriteAsync(string payload, CancellationToken cancellationToken = default(CancellationToken))
        {
            return OnSendAsync(payload, Precision.Nanoseconds, cancellationToken);
        }

        public Task<LineProtocolWriteResult> SendAsync(LineProtocolWriter lineProtocolWriter, CancellationToken cancellationToken = default(CancellationToken))
        {
            return OnSendAsync(lineProtocolWriter.ToString(), lineProtocolWriter.Precision, cancellationToken);
        }

        protected abstract Task<LineProtocolWriteResult> OnSendAsync(
            string payload,
            Precision precision,
            CancellationToken cancellationToken = default(CancellationToken));

        protected StringBuilder RentStringBuilder()
        {
            StringBuilder stringBuilder = null;
            lock (_stringBuilders)
            {
                if (_stringBuilders.Count > 0)
                {
                    stringBuilder = _stringBuilders.Dequeue();
                }
            }
            if (stringBuilder == null)
            {
                stringBuilder = new StringBuilder();
            }

            return stringBuilder;
        }

        protected void ReturnStringBuilder(StringBuilder stringBuilder)
        {
            stringBuilder.Clear();
            lock (_stringBuilders)
            {
                _stringBuilders.Enqueue(stringBuilder);
            }
        }
    }
}
