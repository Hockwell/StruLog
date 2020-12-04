using System;

namespace StruLog.Entites
{
    /// <summary>
    /// логируемая информация, она обрабатывается и проходит фильтрацию (напрямую в БД не пишется)
    /// </summary>
    internal class LogData
    {
        public DateTime time;
        public LogLevel level;
        public string message;
        /// <summary>
        /// полное название логгера
        /// </summary>
        public string loggerName;
        public object obj;
        public Exception exception;
    }
}
