using StruLog.Entites;
using System;

namespace StruLog
{
    public class Logger
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
    }
}
