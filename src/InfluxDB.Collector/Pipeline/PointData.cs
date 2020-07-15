using System;
using System.Collections.Generic;

namespace InfluxDB.Collector.Pipeline
{
    public class PointData
    {
        public string Measurement { get; }
        public Dictionary<string, object> Fields { get; }
        public Dictionary<string, string> Tags { get; set; }
        public DateTime? UtcTimestamp { get; }

        public PointData(
            string measurement,
            Dictionary<string, object> fields,
            Dictionary<string, string> tags = null,
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
