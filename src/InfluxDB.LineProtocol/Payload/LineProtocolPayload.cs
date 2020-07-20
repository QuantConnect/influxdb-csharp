using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace InfluxDB.LineProtocol.Payload
{
    public class LineProtocolPayload
    {
        IEnumerable<LineProtocolPoint> _points;

        public LineProtocolPayload(IEnumerable<LineProtocolPoint> points = null)
        {
            _points = points ?? Enumerable.Empty<LineProtocolPoint>();
        }

        public void Add(LineProtocolPoint point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            _points = _points.Concat(new []{ point });
        }

        public void Format(TextWriter textWriter)
        {
            if (textWriter == null) throw new ArgumentNullException(nameof(textWriter));

            foreach (var point in _points)
            {
                point.Format(textWriter);
                textWriter.Write('\n');
            }
        }
    }
}
