using StruLog.Entites;
using System;

namespace StruLog
{
    public class Logger
    {
        private static readonly object _lock = new object();
        public string Name { get; private set; }

        internal bool IsInsideLogger { get; private set; }
        internal Logger(string loggerName, bool isInsideLogger)
        {
            Name = loggerName;
            IsInsideLogger = isInsideLogger;
        }


        internal void Log(LogLevel level, string message, object obj = null, Exception exception = null)
        {
            lock (_lock)
            {
                var time = GetCurrentTime();
                var logData = new LogData
                {
                    level = level,
                    message = message,
                    time = time,
                    obj = obj,
                    exception = exception,
                    loggerName = Name
                };

                if (IsInsideLogger) //самологгирование
                {
                    ConfigFileProvider.Config.insideLoggingStore.TryLog(logData);
                }
                else
                    foreach (var storeManager in ConfigFileProvider.Config.usingStores)
                    {
                        storeManager.TryLog(logData);
                    }
            }

        }

        internal static DateTime GetCurrentTime()
        {
            switch (ConfigFileProvider.Config.time)
            {
                case TimeRepresentation.LOCAL:
                    return DateTime.Now;
                case TimeRepresentation.UTC:
                    return DateTime.UtcNow;
                default:
                    return DateTime.Now;
            }
        }
    }
}
