using System;
using System.Collections.Generic;

namespace InfluxDB.LineProtocol.Payload
{
    public interface IPointData
    {
        string Measurement { get; }

        DateTime? UtcTimestamp { get; }

        KeyValuePair<string, string>[] Tags { get; set; }

        KeyValuePair<string, object>[] Fields { get; }
    }
}