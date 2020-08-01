using System.Collections.Generic;
using System.Linq;
using InfluxDB.Collector.Util;
using Xunit;

namespace InfluxDB.LineProtocol.Tests
{
    public class UtilTests
    {
        [Theory]
        [InlineData(0, 0)]
        [InlineData(0, 10)]
        [InlineData(10, 10)]
        [InlineData(9, 10)]
        [InlineData(10, 9)]
        [InlineData(11, 9)]
        [InlineData(11, 22)]
        [InlineData(11, 23)]
        [InlineData(22, 11)]
        [InlineData(23, 11)]
        public void BatchWorks(int count, int batchSize)
        {
            var queue = new Queue<string>();

            for (var i = 0; i < count; i++)
            {
                queue.Enqueue(i.ToString());
            }

            var outputCount = 0;
            queue.Batch(batchSize, strings =>
            {
                Assert.True(strings.All(s => !string.IsNullOrEmpty(s)));
                Assert.True(strings.All(s => s == (outputCount++).ToString()));
            });

            Assert.True(count == outputCount);
        }
    }
}
