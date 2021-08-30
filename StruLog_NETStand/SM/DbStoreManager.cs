using StruLog.Entites;
using StruLog.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static StruLog.Entites.LogDataModel;

namespace StruLog.SM
{
    /// <summary>
    /// Обеспечивает общий функционал для DB-based хранилищ, где хранятся типизированные данные
    /// </summary>
    internal abstract class DbStoreManager : StoreManager
    {
        private static ushort LogEntriesIterator { get; set; } = 0; //итератор сессии логирования для гарантии уникального Id лог-записи в БД
        internal override object CreateLogEntry(LogData logData, object outputPattern)
        {
            LogDataModel model = new LogDataModel();
            if (logData.exception != null)
                model.exception = new LogDataModel.ExceptionInfo();
            foreach (var action in outputPattern as List<Action<LogData, LogDataModel>>)
            {
                action.Invoke(logData, model);
            }
            model._id = GenerateLogEntryID(logData);
            return model;
        }

        private string GenerateLogEntryID(LogData logData)
        {
            string pid_hex = $"{ Process.GetCurrentProcess().Id:X6}";
            string appDomain_hex = $"{ AppDomain.CurrentDomain.Id:X1}";
            return $"{logData.time} {logData.level} <{pid_hex}{appDomain_hex}{(LogEntriesIterator++):X4}>";
        }

        internal static List<Action<LogData, LogDataModel>> GetOutputActions(string outputPattern)
        {
            var outputActions = new List<Action<LogData, LogDataModel>>();
            StringBuilder selector = new StringBuilder();
            bool selectorWasFound = false;

            foreach (var ch in outputPattern)
            {
                if (ch == SELECTOR_START_CHAR)
                {
                    selectorWasFound = true;
                    continue;
                }
                if (ch == SELECTOR_END_CHAR) //селектор получен
                {
                    selectorWasFound = false;
                    outputActions.Add(CreateOutputActionBySelector(selector.ToString()));
                    selector = selector.Clear();
                    continue;
                }
                if (selectorWasFound)
                {
                    selector.Append(ch);
                    continue;
                }
            }

            
            return outputActions;
        }



        private static Action<LogData, LogDataModel> CreateOutputActionBySelector(string selector)
        {
            Match innerExceptionMatch = new Regex(@"innerExc-(\d{1,2})", RegexOptions.Compiled | RegexOptions.IgnoreCase).Match(selector);
            ushort suffix;

            switch (selector)
            {
                case "msg":
                    return (logData, logEntryObj) => logEntryObj.message = logData.message;
                case "excMsg":
                    return (logData, logEntryObj) =>
                    {
                        if (logEntryObj.exception != null)
                            logEntryObj.exception.msg = GetExcMsg(logData.exception);
                    };
                case "excClassLine":
                    return (logData, logEntryObj) =>
                    {
                        if (logEntryObj.exception != null)
                            logEntryObj.exception.classLine = GetExcClassLine(logData.exception);
                    };
                case "excStackTrace":
                    return (logData, logEntryObj) =>
                    {
                        if (logEntryObj.exception != null)
                            logEntryObj.exception.stackTrace = logData.exception?.StackTrace;
                    };
                case "time":
                    return (logData, logEntryObj) =>
                    {
                        logEntryObj.time = logData.time;
                    };
                case "logLevel":
                    return (logData, logEntryObj) =>
                    {
                        logEntryObj.level = logData.level.EnumToString();
                    };
                case var s when innerExceptionMatch.Success:
                    ushort.TryParse(innerExceptionMatch.Groups[1].ToString(), out suffix);
                    return (logData, logEntryObj) =>
                    {
                        if (logEntryObj.exception is null)
                            return;
                        Exception innerExc = logData.exception.InnerException;
                        int listCapacitity = suffix / 2;
                        logEntryObj.innerExceptions = new List<ExceptionInfo>(listCapacitity == 0 ? 1 : listCapacitity);
                        for (int i = 1; i <= suffix; i++)
                        {
                            if (innerExc is null)
                                break;
                            logEntryObj.innerExceptions.Add(new ExceptionInfo { msg = GetExcMsg(innerExc), stackTrace = innerExc.StackTrace });
                            innerExc = innerExc.InnerException;
                        }
                    };
                case "loggerName":
                    return (logData, logEntryObj) =>
                    {
                        logEntryObj.loggerName = logData.loggerName;
                    };
                case "obj":
                    return (logData, logEntryObj) =>
                    {
                        logEntryObj.obj = logData.obj;
                    };
                default:
                    throw new StruLogConfigException($"Unknown selector '{selector}' detected. You must repair or remove it.");
            }
        }

    }

        
}

