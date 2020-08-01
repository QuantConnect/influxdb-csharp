using System;
using System.Collections.Generic;

namespace InfluxDB.Collector.Util
{
    public static class Util
    {
        /// <summary>
        /// Helper method to batch a queue into an array with maximum size and trigger an action
        /// </summary>
        public static void Batch<T>(this Queue<T> batch, int size, Action<T[]> action)
        {
            if (batch.Count == 0)
            {
                return;
            }

            var batched = new T[batch.Count > size ? size : batch.Count];
            var j = 0;
            do
            {
                batched[j] = batch.Dequeue();

                if (++j % size == 0)
                {
                    j = 0;
                    action(batched);
                    batched = new T[batch.Count > size ? size : batch.Count];
                }
                else if (0 == batch.Count)
                {
                    action(batched);
                    break;
                }
            } while (batch.Count > 0);
        } 
    }
}
