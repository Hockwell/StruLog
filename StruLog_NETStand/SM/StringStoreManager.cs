using Newtonsoft.Json;
using StruLog.Entites;
using StruLog.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
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

            int i = -1;
            while (++i < outputPattern.Length)
            {
                if (outputPattern[i] == SELECTOR_START_CHAR)
                {
                    selectorWasFound = true;
                    continue;
                }

                if (outputPattern[i] == SELECTOR_END_CHAR) //селектор получен
                {
                    selectorWasFound = false;
                    outputActions.Add(CreateOutputActionBySelector(selector.ToString()));
                    selector = selector.Clear();
                    continue;
                }

                if (selectorWasFound)
                {
                    selector.Append(outputPattern[i]);
                    continue;
                }
                //не нашли ни паттерн, ни управляющие символы => просто повторяем написанное в паттерне

                string betweenSelectors = "";
                for (;  i < outputPattern.Length; i++)
                {
                    betweenSelectors += outputPattern[i];
                    if (i != outputPattern.Length - 1 && outputPattern[i + 1] == SELECTOR_START_CHAR)
                        break;
                }
                outputActions.Add((logData) => betweenSelectors);
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
                        return obj != null ? JsonConvert.SerializeObject(obj) : string.Empty;
                    };
                default:
                    throw new StruLogConfigException($"Unknown selector '{selector}' detected. Repair it or remove.");
            }

        }
    }
}
