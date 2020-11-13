using Xunit;
using InfluxDB.LineProtocol.Payload;
using System.Collections.Generic;
using System;
using System.IO;
using InfluxDB.Collector.Pipeline;

namespace InfluxDB.LineProtocol.Tests
{
    public class LineProtocolPointTests
    {
        [Fact]
        public void CompleteExampleFromDocs()
        {
            // Given in: https://influxdb.com/docs/v0.9/write_protocols/write_syntax.html
            const string expected = "\"measurement\\ with\\ quotes\",tag\\ key\\ with\\ spaces=tag\\,value\\,with\"commas\" field_key\\\\\\\\=\"string field value, only \\\" need be quoted\" 1441756800000000000\n";

            var point = new LineProtocolPayload(new IPointData[]{ new PointData(
                "\"measurement with quotes\"",
                new[]
                {
                    new KeyValuePair<string, object>("field_key\\\\\\\\", "string field value, only \" need be quoted")
                },
                new[]
                {
                    new KeyValuePair<string, string>("tag key with spaces", "tag,value,with\"commas\"" )
                },
                new DateTime(2015, 9, 9, 0, 0, 0, DateTimeKind.Utc)) });

            var sw = new StringWriter();
            point.Format(sw);

            Assert.Equal(expected, sw.ToString());
        }

        [Fact]
        public void WriteNanosecondTimestamps()
        {
            const string expected = "a,t=1 f=1i 1490951520002000000\n";

            var point = new LineProtocolPayload(new IPointData[]{ new PointData(
                "a",
                new[]
                {
                    new KeyValuePair<string, object>("f", 1)
                },
                new[]
                {
                    new KeyValuePair<string, string>("t", "1")
                },
                new DateTime(636265483200020000L, DateTimeKind.Utc)) });

            var sw = new StringWriter();
            point.Format(sw);

            Assert.Equal(expected, sw.ToString());
        }

        [Fact]
        public void ExampleWithJsonTextWithNestedDoubleQuote()
        {
            const string expected = "\"measurement\\ with\\ json\\ with\\ quotes\",symbol=test field_key=\"{\\\"content\\\":\\\"test \\\\\\\" data\\\"}\" 1441756800000000000\n";

            var json = "{\"content\":\"test \\\" data\"}";

            var point = new LineProtocolPayload(new IPointData[]{ new PointData(
                "\"measurement with json with quotes\"",
                new[]
                {
                    new KeyValuePair<string, object>("field_key", json)
                },
                new[]
                {
                    new KeyValuePair<string, string>("symbol", "test")
                },
                new DateTime(2015, 9, 9, 0, 0, 0, DateTimeKind.Utc)) });

            var sw = new StringWriter();
            point.Format(sw);

            Assert.Equal(expected, sw.ToString());
        }

        [Fact]
        public void Decimal()
        {
            var point = new LineProtocolPayload(new IPointData[]{ new PointData(
                "pinocho",
                new[]
                {
                    new KeyValuePair<string, object>("value", 1090213.0000m)
                },
                new[]
                {
                    new KeyValuePair<string, string>("symbol", "test")
                },
                new DateTime(2015, 9, 9, 0, 0, 0, DateTimeKind.Utc)) });

            var sw = new StringWriter();
            point.Format(sw);

            var result = sw.ToString();

            Assert.Equal("pinocho,symbol=test value=1090213.0000 1441756800000000000\n", sw.ToString());
        }
    }
}
