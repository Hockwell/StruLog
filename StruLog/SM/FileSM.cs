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
        private static readonly string CreatedTimeOfLastLogFile_InfoFilePath = $@"{AppContext.BaseDirectory}/CreatedTimeOfLastLogFile";
        //private static readonly string LogFilesRootDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
        public BlockingCollection<LogData> ProcessingQueue { get; }
        public int[] AccessAttemptsDelays_mSeconds { get; }
        public int ProcessingQueueSize { get; }
        private static FileSM @this = null;
        private DateTime createdTimeOfLastFile;
        private StreamWriter file = null; //дескриптор занимаем при работе ПО: 
        //меньше запросов к ОС за дескриптором и не позволяем др. ПО завладеть файлом
        private FileStore Config;


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
                throw new StruLogConfigException($"Не получена конфигурация для хранилища {NAME}");
            if (@this == null)
            {
                @this = new FileSM(config);
            }
            return @this;
        }
        private async Task Log()
        {
            var logsBatchProcessor = new LogsBatchProccessor<string>(this, Config.outputPattern);
            Func<Task> CreateFile_Func = async () =>
            {
                CreateFileOnHot();
            };
            Func<string, LogData, Task> WriteLogEntryTo_Func = async (logEntry, logData) =>
            {
                if (IsNecessaryCreateNewFile(logData.time))
                    CreateFileOnHot();
                WriteTo(logEntry);
            };
            await logsBatchProcessor.Log_Type1Async(CreateFile_Func, WriteLogEntryTo_Func);
        }

        private string RecognizeLogsPathFromConfig()
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
            var time = Logger.GetCurrentTime(); //данные берутся не из LogData-ы, ибо они не нужны до её создания,
            //возможно расхождение во времени на пару секунд
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
                default:
                    Logger.Important($"Найден неизвестный селектор пути {selector}. Вместо него применён селектор m");
                    return time.Month.ToString();
            }
        }
        private bool IsNecessaryCreateNewFile(DateTime logEntryTime) => logEntryTime.Ticks > createdTimeOfLastFile.Date.Ticks + Config.recreationPeriodInDays * TimeSpan.TicksPerDay;
        protected override void WriteTo(object logEntry)
        {
            file.Write(logEntry);
            file.Write(Environment.NewLine);
        }
        private DateTime ImportCreatedTimeOfLastFile()
        {
            string fileContent = null;
            try
            {
                fileContent = File.ReadAllText(CreatedTimeOfLastLogFile_InfoFilePath);
            }
            catch
            {
                Trace.WriteLine("Файл с датой последнего создания лог-файла не найден");
            }
            if (String.IsNullOrEmpty(fileContent))
                return default;

            return Convert.ToDateTime(fileContent, CultureInfo.InvariantCulture);
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

            string logFilePath = RecognizeLogsPathFromConfig();
            Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));
            file = new StreamWriter(logFilePath, true);
            file.AutoFlush = true; //гарантированно данные должны вписаться в лог, в любой момент программа может упасть
            SaveCreatedTimeOfLastFile();


            bool IsFirstFileInitialization() => file == null;
            void SaveCreatedTimeOfLastFile()
            {
                createdTimeOfLastFile = Logger.GetCurrentTime(); //время запрошено из класса логгера, ибо нужен конфиг с поясом
                try
                {
                    File.WriteAllText(CreatedTimeOfLastLogFile_InfoFilePath, createdTimeOfLastFile.ToString("d",CultureInfo.InvariantCulture));
                }
                catch
                {
                    Trace.WriteLine("Дату последнего создания лог-файла невозможно записать в файл");
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
                Logger.Error($"Невозможно добавить эл-ты в очередь на обработку. Возможно, поток обработки не функционирует.{ex.GetType()}:{ex.Message}");
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
                    Logger.Error($"Поток обработки логов аварийно прекратил работу.{ex.GetType()}:{ex.Message}");
                }
                ProcessingQueue.CompleteAdding();
            }, TaskCreationOptions.LongRunning);
        }
    }
}
