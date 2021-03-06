﻿using System;

namespace InfluxDB.Collector.Configuration
{
    public abstract class CollectorBatchConfiguration
    {
        public CollectorConfiguration AtInterval(TimeSpan interval) => AtInterval(interval, 5000, 4);

        public abstract CollectorConfiguration AtInterval(TimeSpan interval, int? maxBatchSize, int? workers);
    }
}