using StruLog.Entites;
using StruLog.Entites.Stores;
using StruLog.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace StruLog.SM
{
    internal class ConsoleSM : StringStoreManager, IBatchProcessingCompatible //Singleton
    {
        internal const string NAME = "console";
        public BlockingCollection<LogData> ProcessingQueue { get; }
        public int[] AccessAttemptsDelays_mSeconds { get; }
        public int ProcessingQueueSize { get; }
        private static ConsoleSM @this = null;
        private ConsoleStore Config;


        private ConsoleSM(ConsoleStore config)
        {
            Config = config;
            ProcessingQueueSize = 100_000;
            ProcessingQueue = new BlockingCollection<LogData>(ProcessingQueueSize);
            AccessAttemptsDelays_mSeconds = new int[] { 1, 3, 6, 10, 15, 25, 30, 40 };
            Logger = LoggersFactory.GetLogger<ConsoleSM>(true);
            MinLogLevel = config.minLogLevel;
        }
        internal static ConsoleSM Init(ConsoleStore config)
        {
            if (config is null)
                throw new StruLogConfigException($"Not found configuration for '{NAME}' store");
            if (@this == null)
            {
                @this = new ConsoleSM(config);
            }
            return @this;
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
        private async Task Log() //Не вынесен как abstract в StoreManager, ибо только при пакетной обработке нет аргументов, а при обычной есть
        {
            var logsBatchProcessor = new LogsBatchProccessor<string>(this, Config.outputPattern);
            Func<Task> ConnectToFunc = () =>
            {
                //empty
                return Task.CompletedTask;
            };
            Func<string, LogData, Task> WriteLogEntryToFunc = async (logEntry, logData) =>
            {
                var fontColorBefore = Console.ForegroundColor;
                var backColorBefore = Console.BackgroundColor;
                SwitchConsoleColorByLogLevel(logData);
                await WriteTo(logEntry);
                Console.ForegroundColor = fontColorBefore;
                Console.BackgroundColor = backColorBefore;
            };
            await logsBatchProcessor.Log_Type1Async(ConnectToFunc, WriteLogEntryToFunc);

        }
        protected override Task WriteTo(object logEntry)
        {
            Console.WriteLine($">>> {logEntry}");
            return Task.CompletedTask;
        }
        private void SwitchConsoleColorByLogLevel(LogData logData)
        {
            switch (logData.level)
            {
                case LogLevel.TRACE:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
                case LogLevel.DEBUG:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogLevel.INFO:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case LogLevel.IMPORTANT:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case LogLevel.WARNING:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.ERROR:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogLevel.FATAL:
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.Red;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.White;
                    Logger.Important($"There is not handler for '{logData.level.EnumToString<LogLevel>()}' color. Set White.");
                    break;

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
                    Logger.Error($"Processing thread is dropped. {ex.GetType()}:{ex.Message}");
                }
                ProcessingQueue.CompleteAdding();
            }, TaskCreationOptions.LongRunning);
        }
    }
}
