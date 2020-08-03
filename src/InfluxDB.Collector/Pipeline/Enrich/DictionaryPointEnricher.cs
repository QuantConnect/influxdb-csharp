using System.Collections.Generic;
using System.Linq;
using InfluxDB.LineProtocol.Payload;

namespace InfluxDB.Collector.Pipeline.Enrich
{
    class DictionaryPointEnricher : IPointEnricher
    {
        readonly IReadOnlyDictionary<string, string> _tags;

        public DictionaryPointEnricher(IReadOnlyDictionary<string, string> tags)
        {
            _tags = tags;
        }

        public void Enrich(IPointData point)
        {
            var tags = point.Tags ?? new KeyValuePair<string, string>[0];
            point.Tags = tags.Concat(_tags.Where(pair => !tags.Contains(pair))).ToArray();
        }
    }
}
