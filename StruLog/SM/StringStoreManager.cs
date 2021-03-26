using StruLog.Entites;
using StruLog.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace StruLog.SM
{
    /// <summary>
    /// Обеспечивает общий функционал для string-based хранилищ (например, файлов и консоли, где данные пишутся строками)
    /// </summary>
    internal abstract class StringStoreManager : StoreManager
    {
        internal override object CreateLogEntry(LogData logData, object outputPattern) //TODO: проход по листу паттерна, invoke каждого и конкатенация
        {
            StringBuilder logEntry = new StringBuilder();

            foreach (var action in outputPattern as List<Func<LogData, string>>)
            {
                logEntry.Append(action.Invoke(logData));
            }

            return logEntry.ToString();
        }
        internal static List<Func<LogData, string>> GetOutputActions(string outputPattern)
        {
            var outputActions = new List<Func<LogData, string>>();
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
                //не нашли ни паттерн, ни управляющие символы => просто повторяем написанное в паттерне
                outputActions.Add((logData) => ch.ToString());
            }

            return outputActions;
        }

        private static Func<LogData, string> CreateOutputActionBySelector(string selector)
        {
            var loggerNameRegex = new Regex(@"loggerName-[1-9]{1}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            switch (selector)
            {
                case "msg":
                    return (logData) => logData?.message;
                case "excMsg":
                    return (logData) =>
                    {
                        if (logData.exception is null)
                            return null;
                        return GetExcMsg(logData);
                    };
                case "excClassLine":
                    return (logData) => GetExcClassLine(logData);
                case "excStackTrace":
                    return (logData) =>
                    {
                        if (logData.exception is null)
                            return null;
                        else
                            return $"{Environment.NewLine}STACKTRACE:{logData.exception.StackTrace}";
                    };
                case "time":
                    return (logData) => logData.time.ToString();
                case "logLevel":
                    return (logData) => logData.level.EnumToString();
                case "loggerName":
                    return (logData) => logData.loggerName;
                case var s when loggerNameRegex.IsMatch(s): //'loggerName-n'
                    ushort n = ushort.Parse(s[s.Length - 1].ToString());
                    return (logData) =>
                    {
                        return ExtractShortLoggerName(n, logData.loggerName);
                    };
                case "obj":
                    return (logData) =>
                    {
                        var obj = logData.obj;
                        return obj != null ? JsonSerializer.Serialize(obj) : string.Empty;
                    };
                default:
                    throw new StruLogConfigException($"Unknown selector '{selector}' detected. Repair it or remove.");
            }

        }
    }
}
