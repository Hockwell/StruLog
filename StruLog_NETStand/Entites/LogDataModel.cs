using System;
using System.Collections.Generic;

namespace StruLog.Entites
{
    /// <summary>
    /// Отфильтрованная по паттерну логируемая информация, пригодная в том числе для типизированного добавления в БД
    /// </summary>
    public class LogDataModel
    {
        public class ExceptionInfo
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
        public IList<ExceptionInfo> innerExceptions;
    }
}
