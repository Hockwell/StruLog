﻿using StruLog.Entites;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StruLog.SM
{
    internal abstract class StoreManager
    {
        protected const char SELECTOR_START_CHAR = '{';
        protected const char SELECTOR_END_CHAR = '}';
        internal Logger Logger;
        protected LogLevel MinLogLevel;
        /// <summary>
        /// Инициирует обработку лога хранилищем
        /// </summary>
        /// <param name="logData"></param>
        abstract internal void TryLog(LogData logData);
        /// <summary>
        /// Записывает лог в хранилище
        /// </summary>
        /// <param name="logEntry">handled logEntry for output</param>
        abstract protected Task WriteTo(object logEntry);
        abstract internal object CreateLogEntry(LogData logData, object outputPattern);

        internal bool IsLoggingAllowed(LogLevel logEntryLevel)
        {
            return MinLogLevel <= logEntryLevel;
        }
        /// <summary>
        /// Показывает класс и строку по 0 и 1 фреймам
        /// </summary>
        /// <returns></returns>
        protected static string GetExcClassLine(Exception exc)
        {
            string str = string.Empty;
            if (exc is null)
                return null;

            System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(exc, true);
            str += ExtractClassLineFromFrame(0);
            str += ExtractClassLineFromFrame(1);
            return str;

            string ExtractClassLineFromFrame(int frameNum)
            {
                var stackFrame = trace.GetFrame(frameNum);
                var @class = stackFrame?.GetMethod().ReflectedType.Name;
                var @method = stackFrame?.GetFileLineNumber();
                if (!string.IsNullOrEmpty(@class))
                    return $"/Frame{frameNum}: {@class},{@method} ";
                return string.Empty;
            }
        }
        protected static string GetExcMsg(Exception exc)
        {
            if (exc is null)
                return null;
            return $"{ exc.GetType()}: { exc.Message }";
        }
        internal static void RunProcessing()
        {
            foreach (var store in ConfigProvider.Config.usingStores)
            {
                if (store is IBatchProcessingCompatible bStore)
                    bStore.RunBatchProcessing();
            }
        }

        /// <summary>
        /// Позволяет получить лишь часть Namespace-имени логгера, которое разделено точками зачастую
        /// </summary>
        /// <param name="loggerNameSegmentsNum"></param>
        /// <param name="loggerName"></param>
        /// <returns></returns>
        internal static string ExtractShortLoggerName(ushort loggerNameSegmentsNum, string loggerName)
        {
            ushort nameSegmentsIndex = 0; //MainClass.LalkaClass.GalkaClass (3 parts); 1 - крайний правый сегмент (GalkaClass)
            for (int i = loggerName.Length - 1; i >= 0; i--) //идём справа-налево
            {
                if (loggerName[i] == '.') //край сегмента достигнут
                {
                    nameSegmentsIndex++;
                    if (nameSegmentsIndex == loggerNameSegmentsNum)
                    {
                        return loggerName.Substring(i + 1); //+1, чтобы точку слева миновать
                    }
                }
            }
            return loggerName; //в имени сегментов меньше или равно требуемого кол-ва
        }
    }
}
