using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace InfluxDB.LineProtocol.Payload
{
    public class LineProtocolPayload
    {
        IPointData[] _points;

        public LineProtocolPayload(IEnumerable<IPointData> points = null)
        {
            _points = (points ?? Enumerable.Empty<IPointData>()).ToArray();
        }
        public LineProtocolPayload(IPointData[] points)
        {
            _points = points;
        }

        public void Add(IPointData point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            _points = _points.Concat(new []{ point }).ToArray();
        }

        public void Format(TextWriter textWriter)
        {
            if (textWriter == null) throw new ArgumentNullException(nameof(textWriter));

            for (var i = 0; i < _points.Length; i++)
            {
                FormatPoint(textWriter, _points[i]);
                textWriter.Write('\n');
            }
        }

        private void FormatPoint(TextWriter textWriter, IPointData pointData)
        {
            if (textWriter == null) throw new ArgumentNullException(nameof(textWriter));

            LineProtocolSyntax.EscapeName(pointData.Measurement, textWriter);

            if (pointData.Tags != null)
            {
                var enumerable = pointData.Tags.Length == 1
                    ? (IEnumerable<KeyValuePair<string, string>>)pointData.Tags
                    : pointData.Tags.OrderBy(t => t.Key);

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
            for (var i = 0; i < pointData.Fields.Length; i++)
            {
                var kvp = pointData.Fields[i];

                if (string.IsNullOrEmpty(kvp.Key)) throw new ArgumentException("Fields must have non-empty names");

                textWriter.Write(fieldDelim);
                LineProtocolSyntax.EscapeName(kvp.Key, textWriter);
                textWriter.Write('=');
                textWriter.Write(LineProtocolSyntax.FormatValue(kvp.Value));

                fieldDelim = ',';
            }

            if (pointData.UtcTimestamp != null)
            {
                textWriter.Write(' ');
                textWriter.Write(LineProtocolSyntax.FormatTimestamp(pointData.UtcTimestamp.Value));
            }
        }
    }
}
