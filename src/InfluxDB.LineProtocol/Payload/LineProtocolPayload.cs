using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace InfluxDB.LineProtocol.Payload
{
    public class LineProtocolPayload
    {
        List<IPointData> _points;

        public LineProtocolPayload(IEnumerable<IPointData> points = null)
        {
            _points = (points ?? Enumerable.Empty<IPointData>()).ToList();
        }

        public void Add(IPointData point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            _points = _points.Concat(new []{ point }).ToList();
        }

        public void Format(TextWriter textWriter)
        {
            if (textWriter == null) throw new ArgumentNullException(nameof(textWriter));

            for (var i = 0; i < _points.Count; i++)
            {
                FormatPoint(textWriter, _points[i]);
                textWriter.Write('\n');
            }
        }

        public static void Format(TextWriter textWriter, List<IPointData> points)
        {
            for (var i = 0; i < points.Count; i++)
            {
                FormatPoint(textWriter, points[i]);
                textWriter.Write('\n');
            }
        }

        private static void FormatPoint(TextWriter textWriter, IPointData pointData)
        {
            if (textWriter == null) throw new ArgumentNullException(nameof(textWriter));

            LineProtocolSyntax.EscapeName(pointData.Measurement, textWriter);

            if (pointData.Tags != null && pointData.Tags.Length > 0)
            {
                if (pointData.Tags.Length == 1)
                {
                    textWriter.Write(',');
                    LineProtocolSyntax.EscapeName(pointData.Tags[0].Key, textWriter);
                    textWriter.Write('=');
                    LineProtocolSyntax.EscapeName(pointData.Tags[0].Value, textWriter);
                }
                else
                {
                    foreach (var t in pointData.Tags.OrderBy(t => t.Key))
                    {
                        if (string.IsNullOrEmpty(t.Key)) throw new ArgumentException("Tags must have non-empty names");

                        textWriter.Write(',');
                        LineProtocolSyntax.EscapeName(t.Key, textWriter);
                        textWriter.Write('=');
                        LineProtocolSyntax.EscapeName(t.Value, textWriter);
                    }
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
