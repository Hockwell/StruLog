using Microsoft.Extensions.Logging;
using StruLog.Entites;
using System;
using LogLevel = StruLog.Entites.LogLevel;

namespace StruLog
{
    public class Logger : ILogger
    {
        public string Name { get; private set; }

        internal bool IsInsideLogger { get; private set; }
        internal Logger(string loggerName, bool isInsideLogger)
        {
            Name = loggerName;
            IsInsideLogger = isInsideLogger;
        }


        internal void Log(LogLevel level, string message, object obj = null, Exception exception = null)
        {
            var time = ConfigProvider.Config.currentTime_Func();
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
                var sm = ConfigProvider.Config.insideLoggingStore;
                sm.TryLog(logData);
                //if (sm is null)
                //    Console.WriteLine($"StruLog initialization event: {logData.loggerName} : {logData.level.EnumToString()} : {logData.message} : {logData.exception?.Message} : {logData.exception?.StackTrace}");
                //else
                //    sm.TryLog(logData);
            }
            else
                foreach (var storeManager in ConfigProvider.Config.usingStores)
                {
                    storeManager.TryLog(logData);
                }

        }

        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            string message = null;
            object eventObj = eventId != default ? new { Id = eventId.Id, Name = eventId.Name } : null;
            object obj;

            if (formatter != null) //object is absent
            {
                message = formatter(state, exception);
                obj = new { @event = eventObj };
            }
            else //print object only, without message
            {
                if (eventObj is null)
                    obj = new { @object = state };
                else
                    obj = new { @object = state, @event = eventObj };
            }

            switch (logLevel)
            {
                case Microsoft.Extensions.Logging.LogLevel.Critical:
                    Log(LogLevel.FATAL, message, obj, exception);
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Debug:
                    Log(LogLevel.DEBUG, message, obj, exception);
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Trace:
                    Log(LogLevel.TRACE, message, obj, exception);
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Error:
                    Log(LogLevel.ERROR, message, obj, exception);
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Information:
                    Log(LogLevel.INFO, message, obj, exception);
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Warning:
                    Log(LogLevel.WARNING, message, obj, exception);
                    break;
                case Microsoft.Extensions.Logging.LogLevel.None:
                    break;
                default:
                    Log(LogLevel.INFO, message, state, exception);
                    Log(LogLevel.WARNING, $"(Encountered unknown logLevel = {logLevel}, writing out as Info)");
                    break;
            }
        }

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
}
