﻿using System;
using System.Collections.Generic;
using InfluxDB.Collector.Pipeline;

namespace InfluxDB.Collector
{
    public static class Metrics
    {
        static MetricsCollector CurrentCollector = new NullMetricsCollector();

        public static MetricsCollector Collector
        {
            get { return CurrentCollector; }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                CurrentCollector = value;
            }
        }

        public static void Increment(string measurement, long value = 1, Dictionary<string, string> tags = null)
        {
            Collector.Increment(measurement, value, tags);
        }

        public static void Measure(string measurement, object value, Dictionary<string, string> tags = null)
        {
            Collector.Measure(measurement, value, tags);
        }

        public static IDisposable Time(string measurement, Dictionary<string, string> tags = null)
        {
            return Collector.Time(measurement, tags);
        }

        public static void Write(string measurement, Dictionary<string, object> fields, Dictionary<string, string> tags = null)
        {
            Collector.Write(measurement, fields, tags);
        }

        public static CollectorConfiguration Specialize()
        {
            return Collector.Specialize();
        }

        public static void Close()
        {
            Collector.Dispose();
        }
    }
}
