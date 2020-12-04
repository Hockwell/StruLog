using StruLog.Entites;
using System;
using System.Collections.Generic;

namespace StruLog.SM
{
    internal abstract class StoreManager
    {
        protected const char SELECTOR_START_CHAR = '{'; //общий формат селекторов, можно поменять, если указанные символы используются для чего-то ещё в конфиге
        protected const char SELECTOR_END_CHAR = '}';
        internal Logger Logger; //у каждого SM-а есть логгер
        protected LogLevel MinLogLevel;

        abstract internal void TryLog(LogData logData);
        abstract protected void WriteTo(object logEntry);
        abstract internal object CreateLogEntry(LogData logData, object outputPattern);

        internal bool IsLoggingAllowed(LogLevel logEntryLevel)
        {
            return MinLogLevel <= logEntryLevel;
        }
        protected static string GetExcClassLine(LogData logData)
        {
            if (logData.exception is null)
                return null;
            System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(logData.exception, true);
            var stackFrame0 = trace.GetFrame(0);
            var @class = stackFrame0?.GetMethod().ReflectedType.FullName;
            var @method = stackFrame0?.GetFileLineNumber();
            if (string.IsNullOrEmpty(@class))
                return null;
            return $"({@class},{@method})";
        }
        protected static string GetExcMsg(LogData logData)
        {
            if (logData.exception is null)
                return null;
            return $"{ logData.exception.GetType()}:{ logData.exception.Message}";
        }
        internal static void RunProcessing()
        {
            foreach (var store in ConfigFileProvider.Config.usingStores)
            {
                if (store is IBatchProcessingCompatible bStore)
                    bStore.RunBatchProcessing();
            }
        }
    }
}
