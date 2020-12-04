using System;
using System.Collections.Concurrent;

namespace StruLog
{
    public static class LoggersFactory
    {
        private static ConcurrentDictionary<Type, Logger> loggers = new ConcurrentDictionary<Type, Logger>();
        public static Logger GetLogger<T>(bool IsInsideLogger = false)
        {
            return GetLogger(typeof(T), IsInsideLogger);
        }
        public static Logger GetLogger(Type type, bool IsInsideLogger = false)
        {
            return loggers.GetOrAdd(type, (type) => new Logger(type.ToString(), IsInsideLogger));
        }
    }
}
