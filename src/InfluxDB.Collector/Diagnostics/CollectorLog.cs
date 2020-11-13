using System;
using System.Collections.Generic;
using InfluxDB.Collector.Util;

namespace InfluxDB.Collector.Diagnostics
{
    public static class CollectorLog
    {
        private static readonly List<Action<string, Exception>> ErrorHandlers = new List<Action<string, Exception>>();
        private static readonly List<Action<string>> TraceHandlers = new List<Action<string>>();

        /// <summary>
        /// Registers an error handler. Errors reported may not neccessarily always correspond with an exception. That is to say, when the callback is invoked, the exception may be null.
        /// To unregister a handler, simply dispose of the IDisposable returned from this method
        /// The order in which handlers are invoked is not defined
        /// </summary>
        /// <param name="errorHandler">A func accepting a string and an exception. Note that exception may be null when invoked</param>
        /// <returns>An IDispoable which when explicitly disposed, will unnregister the handler</returns>
        public static IDisposable RegisterErrorHandler(Action<string, Exception> errorHandler)
        {
            if (errorHandler == null) throw new ArgumentNullException(nameof(errorHandler));

            lock (ErrorHandlers)
            {
                ErrorHandlers.Add(errorHandler);
            }

            return new DisposableAction(() => 
            {
                lock (ErrorHandlers)
                {
                    ErrorHandlers.Remove(errorHandler);
                }
            });
        }

        /// <summary>
        /// Registers a trace handler.
        /// To unregister a handler, simply dispose of the IDisposable returned from this method
        /// The order in which handlers are invoked is not defined
        /// </summary>
        /// <param name="traceHandler">A func accepting a string and an exception</param>
        /// <returns>An IDispoable which when explicitly disposed, will unnregister the handler</returns>
        public static IDisposable RegisterTraceHandler(Action<string> traceHandler)
        {
            if (traceHandler == null) throw new ArgumentNullException(nameof(traceHandler));

            lock (TraceHandlers)
            {
                TraceHandlers.Add(traceHandler);
            }

            return new DisposableAction(() =>
            {
                lock (TraceHandlers)
                {
                    TraceHandlers.Remove(traceHandler);
                }
            });
        }

        internal static void ReportError(string message, Exception exception)
        {
            lock (ErrorHandlers)
            {
                for (var i = 0; i < ErrorHandlers.Count; i++)
                {
                    ErrorHandlers[i](message, exception);
                }
            }
        }

        internal static void Report(string message)
        {
            lock (TraceHandlers)
            {
                for (var i = 0; i < TraceHandlers.Count; i++)
                {
                    TraceHandlers[i](message);
                }
            }
        }

        internal static void ClearHandlers()
        {
            lock (ErrorHandlers)
            {
                ErrorHandlers.Clear();
            }
            lock (TraceHandlers)
            {
                TraceHandlers.Clear();
            }
        }
    }
}
