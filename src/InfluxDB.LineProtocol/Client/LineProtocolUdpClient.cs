﻿using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace InfluxDB.LineProtocol.Client
{
    public class LineProtocolUdpClient : LineProtocolClientBase
    {
        private readonly UdpClient _udpClient;
        private readonly string _udpHostName;
        private readonly int _udpPort;

        public LineProtocolUdpClient(
                        Uri serverBaseAddress,
                        string database,
                        string username = null,
                        string password = null,
                        string retentionPolicy = null)
            :base(serverBaseAddress, database, username, password, retentionPolicy)
        {
            if (serverBaseAddress == null)
                throw new ArgumentNullException(nameof(serverBaseAddress));
            if (string.IsNullOrEmpty(database))
                throw new ArgumentException("A database must be specified");

            _udpHostName = serverBaseAddress.Host;
            _udpPort = serverBaseAddress.Port;
            _udpClient = new UdpClient();
        }

        protected override async Task<LineProtocolWriteResult> OnSendAsync(
                                    byte[] buffer,
                                    Precision precision,
                                    CancellationToken cancellationToken = default(CancellationToken))
        {
            int len = await _udpClient.SendAsync(buffer, buffer.Length, _udpHostName, _udpPort);
            return new LineProtocolWriteResult(len == buffer.Length, null);
        }
    }
}
