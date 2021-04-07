using StruLog.Entites;
using System;

namespace StruLog
{
    public static class LoggerExtensions
    {
        public static void Info(this Logger logger, string message, object obj = null, Exception ex = null)
        {
            logger.Log(LogLevel.INFO, message, obj, ex);
        }
        public static void Info(this Logger logger, string message, Exception ex)
        {
            logger.Log(LogLevel.INFO, message, exception: ex);
        }


        public static void Warn(this Logger logger, string message, object obj = null, Exception ex = null)
        {
            logger.Log(LogLevel.WARNING, message, obj, ex);
        }
        public static void Warn(this Logger logger, string message, Exception ex)
        {
            logger.Log(LogLevel.WARNING, message, exception: ex);
        }



        public static void Error(this Logger logger, string message, object obj = null, Exception ex = null)
        {
            logger.Log(LogLevel.ERROR, message, obj, ex);
        }
        public static void Error(this Logger logger, string message, Exception ex)
        {
            logger.Log(LogLevel.ERROR, message, exception: ex);
        }



        public static void Fatal(this Logger logger, string message, object obj = null, Exception ex = null)
        {
            logger.Log(LogLevel.FATAL, message, obj, ex);
        }
        public static void Fatal(this Logger logger, string message, Exception ex)
        {
            logger.Log(LogLevel.FATAL, message, exception: ex);
        }


        public static void Important(this Logger logger, string message, object obj = null, Exception ex = null)
        {
            logger.Log(LogLevel.IMPORTANT, message, obj, ex);
        }
        public static void Important(this Logger logger, string message, Exception ex)
        {
            logger.Log(LogLevel.IMPORTANT, message, exception: ex);
        }


        public static void Debug(this Logger logger, string message, object obj = null, Exception ex = null)
        {
            logger.Log(LogLevel.DEBUG, message, obj, ex);
        }
        public static void Debug(this Logger logger, string message, Exception ex)
        {
            logger.Log(LogLevel.DEBUG, message, exception: ex);
        }


        public static void Trace(this Logger logger, string message, object obj = null, Exception ex = null)
        {
            logger.Log(LogLevel.TRACE, message, obj, ex);
        }
        public static void Trace(this Logger logger, string message, Exception ex)
        {
            logger.Log(LogLevel.TRACE, message, exception: ex);
        }
    }
}
