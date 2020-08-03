using System;
using System.Collections.Generic;
using InfluxDB.LineProtocol.Payload;

namespace InfluxDB.Collector.Pipeline
{
    public class PointData : IPointData
    {
        public string Measurement { get; }

        public DateTime? UtcTimestamp { get; }

        public KeyValuePair<string, string>[] Tags { get; set; }

        public KeyValuePair<string, object>[] Fields { get; }

        public PointData(
            string measurement,
            KeyValuePair<string, object>[] fields,
            KeyValuePair<string, string>[] tags = null,
            DateTime? utcTimestamp = null)
        {
            if (measurement == null) throw new ArgumentNullException(nameof(measurement));
            if (fields == null) throw new ArgumentNullException(nameof(fields));

            Measurement = measurement;
            Fields = fields;
            Tags = tags;
            UtcTimestamp = utcTimestamp;
        }
    }
}
