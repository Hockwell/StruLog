using System;

namespace StruLog.Entites
{
    /// <summary>
    /// Отфильтрованная по паттерну логируемая информация, пригодная в том числе для типизированного добавления в БД
    /// </summary>
    internal class LogDataModel
    {
        internal class ExceptionInfo
        {
            public string classLine;
            public string msg;
            public string stackTrace;
        }
        //[BsonId]
        public object _id;
        public DateTime time;
        public string level;
        public string message;
        public string loggerName;
        public object obj;
        public ExceptionInfo exception;
    }
}
