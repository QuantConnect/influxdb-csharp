using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace InfluxDB.LineProtocol.Payload
{
    public class LineProtocolPoint
    {
        private string _measurement;
        private DateTime? _utcTimestamp;
        private Dictionary<string, string> _tags;
        private Dictionary<string, object> _fields;

        public LineProtocolPoint(
            string measurement,
            Dictionary<string, object> fields,
            Dictionary<string, string> tags = null,
            DateTime? utcTimestamp = null)
        {
            if (string.IsNullOrEmpty(measurement)) throw new ArgumentException("A measurement name must be specified");
            if (fields == null || fields.Count == 0) throw new ArgumentException("At least one field must be specified");

            if (utcTimestamp != null && utcTimestamp.Value.Kind != DateTimeKind.Utc)
                throw new ArgumentException("Timestamps must be specified as UTC");

            _measurement = measurement;
            _fields = fields;
            _tags = tags;
            _utcTimestamp = utcTimestamp;
        }

        public void Format(TextWriter textWriter)
        {
            if (textWriter == null) throw new ArgumentNullException(nameof(textWriter));

            LineProtocolSyntax.EscapeName(_measurement, textWriter);

            if (_tags != null)
            {
                var enumerable = _tags.Count == 1
                    ? (IEnumerable<KeyValuePair<string, string>>) _tags
                    : _tags.OrderBy(t => t.Key);

                foreach (var t in enumerable)
                {
                    if (string.IsNullOrEmpty(t.Key)) throw new ArgumentException("Tags must have non-empty names");

                    textWriter.Write(',');
                    LineProtocolSyntax.EscapeName(t.Key, textWriter);
                    textWriter.Write('=');
                    LineProtocolSyntax.EscapeName(t.Value, textWriter);
                }
            }

            var fieldDelim = ' ';
            foreach (var f in _fields)
            {
                if (string.IsNullOrEmpty(f.Key)) throw new ArgumentException("Fields must have non-empty names");

                textWriter.Write(fieldDelim);
                LineProtocolSyntax.EscapeName(f.Key, textWriter);
                textWriter.Write('=');
                textWriter.Write(LineProtocolSyntax.FormatValue(f.Value));

                fieldDelim = ',';
            }

            if (_utcTimestamp != null)
            {
                textWriter.Write(' ');
                textWriter.Write(LineProtocolSyntax.FormatTimestamp(_utcTimestamp.Value));
            }
        }
    }
}

