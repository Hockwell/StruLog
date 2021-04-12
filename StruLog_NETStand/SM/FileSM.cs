using StruLog.Entites;
using StruLog.Entites.Stores;
using StruLog.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace StruLog.SM
{
    internal class FileSM : StringStoreManager, IBatchProcessingCompatible //Singleton
    {
        internal const string NAME = "file";
        private static readonly string CreatedTimeOfLastLogFile_InfoFilePath;
        //private static readonly string LogFilesRootDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
        public BlockingCollection<LogData> ProcessingQueue { get; }
        public int[] AccessAttemptsDelays_mSeconds { get; }
        public int ProcessingQueueSize { get; }
        private static FileSM @this = null;
        /// <summary>
        /// default, if unknown
        /// </summary>
        private DateTime createdTimeOfLastFile;
        private StreamWriter file = null; //дескриптор занимаем при работе ПО: 
        //меньше запросов к ОС за дескриптором и не позволяем др. ПО завладеть файлом
        private FileStore Config;

        static FileSM()
        {
            //string prjName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name ?? "unknownNameOfPrj";
            string prjName = ConfigProvider.Config.projectName;
            CreatedTimeOfLastLogFile_InfoFilePath = $@"{AppContext.BaseDirectory}/CToLLF_{prjName}";
        }
        private FileSM(FileStore config)
        {
            Config = config;
            Logger = LoggersFactory.GetLogger<FileSM>(true);
            ProcessingQueueSize = 1_000_000;
            ProcessingQueue = new BlockingCollection<LogData>(ProcessingQueueSize);
            AccessAttemptsDelays_mSeconds = new int[] { 1, 5, 25, 45, 90, 250, 500, 1000 };
            MinLogLevel = config.minLogLevel;
            createdTimeOfLastFile = ImportCreatedTimeOfLastFile();
        }
        internal static FileSM Init(FileStore config)
        {
            if (config is null)
                throw new StruLogConfigException($"Not found configuration for '{NAME}' store");
            if (@this == null)
            {
                @this = new FileSM(config);
            }
            return @this;
        }
        private async Task Log()
        {
            var logsBatchProcessor = new LogsBatchProccessor<string>(this, Config.outputPattern);
            Func<Task> CreateFile_Func = () =>
            {
                CreateFileOnHot();
                return Task.CompletedTask;
            };
            Func<string, LogData, Task> WriteLogEntryTo_Func = async (logEntry, logData) =>
            {
                if (IsNecessaryCreateNewFile(logData.time))
                    CreateFileOnHot();
                await WriteTo(logEntry);
            };
            await logsBatchProcessor.Log_Type1Async(CreateFile_Func, WriteLogEntryTo_Func);
        }
        /// <summary>
        /// asd_{selector}_DSds -> asd_dataByselector_DSds
        /// </summary>
        /// <returns></returns>
        private string ClarifyLogsPathFromConfig()
        {
            StringBuilder logPath = new StringBuilder();
            StringBuilder selector = new StringBuilder();
            bool selectorWasFound = false;

            foreach (var ch in Config.path)
            {
                if (ch == SELECTOR_START_CHAR)
                {
                    selectorWasFound = true;
                    continue;
                }
                if (ch == SELECTOR_END_CHAR) //селектор получен
                {
                    selectorWasFound = false;
                    logPath.Append(GetInfoByPathSelector(selector.ToString()) ?? string.Empty);
                    selector = selector.Clear();
                    continue;
                }
                if (selectorWasFound)
                {
                    selector.Append(ch);
                    continue;
                }
                //не нашли ни паттерн, ни управляющие символы => просто повторяем написанное в паттерне
                logPath.Append(ch);
            }

            return logPath.ToString();

        }
        private string GetInfoByPathSelector(string selector)
        {
            var time = ConfigProvider.Config.currentTime_Func(); 

            switch (selector)
            {
                case "y":
                    return time.Year.ToString("D4");
                case "m":
                    return time.Month.ToString("D2");
                case "d":
                    return time.Day.ToString("D2");
                case "project":
                    return $"{Directory.GetCurrentDirectory()}";
                case "projectName":
                    return $"{ConfigProvider.Config.projectName}";
                default:
                    Logger.Important($"Unknown selector '{selector}' detected, selector 'm' will be use instead.");
                    return time.Month.ToString();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logEntryTime"></param>
        /// <returns></returns>
        private bool IsNecessaryCreateNewFile(DateTime logEntryTime) => logEntryTime > createdTimeOfLastFile.Date.AddDays(Config.recreationPeriodInDays);
        protected override async Task WriteTo(object logEntry)
        {
            await file.WriteAsync((string)logEntry);
            await file.WriteAsync(Environment.NewLine);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>default при неудаче</returns>
        private DateTime ImportCreatedTimeOfLastFile()
        {
            string fileContent = null;
            DateTime createdTime = default;

            try
            {
                fileContent = File.ReadAllText(CreatedTimeOfLastLogFile_InfoFilePath);
            }
            catch
            {
                Console.WriteLine("StruLog: 'CreatedTimeOfLastLogFile (CToLLF)' not found, will create new log file for current day");
            }

            if (string.IsNullOrEmpty(fileContent))
                return createdTime;

            try
            {
                createdTime = Convert.ToDateTime(fileContent, CultureInfo.InvariantCulture);
            }
            catch
            {
                Logger.Important("Can't parse 'CreatedTimeOfLastLogFile (CToLLF)', rewrite it.");
            }

            return createdTime;
        }
        private void CreateFileOnHot()
        {
            //throw new Exception();
            if (!IsFirstFileInitialization())
            {
                file.Flush();
                file.Dispose();
                file.Close();
            }

            string logFilePath = ClarifyLogsPathFromConfig();
            Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));
            file = new StreamWriter(logFilePath, true);
            file.AutoFlush = true; //гарантированно данные должны вписаться в лог, в любой момент программа может упасть
            SaveCreatedTimeOfLastFile();


            bool IsFirstFileInitialization() => file == null;
            void SaveCreatedTimeOfLastFile()
            {
                createdTimeOfLastFile = ConfigProvider.Config.currentTime_Func(); //we must use time type from config else files will be replace each other not obvious   
                try
                {
                    File.WriteAllText(CreatedTimeOfLastLogFile_InfoFilePath, createdTimeOfLastFile.ToString("d",CultureInfo.InvariantCulture));
                }
                catch
                {
                    Logger.Important("Writing to 'CreatedTimeOfLastLogFile (CToLLF)' is impossible. Next log file will create on next day and there is repeat writing attempt");
                }

            }
        }

        internal override void TryLog(LogData logData)
        {
            try
            {
                ProcessingQueue.TryAdd(logData);
            }
            catch (Exception ex)
            {
                Logger.Error($"Addition of logEntries to queue on processing is impossible. Likely, processing thread doesn't work. {ex.GetType()}:{ex.Message}");
            }

        }

        public void RunBatchProcessing()
        {
            Task task = Task.Factory.StartNew(async () =>
            {
                try
                {
                    await Log();
                }
                catch (Exception ex)
                {
                    Logger.Error($"Processing thread was dropped. {ex.GetType()}:{ex.Message}");
                }
                ProcessingQueue.CompleteAdding();
            }, TaskCreationOptions.LongRunning);
        }
    }
}
