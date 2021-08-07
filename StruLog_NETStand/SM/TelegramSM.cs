using StruLog;
using StruLog.Entites;
using StruLog.Entites.Stores;
using StruLog.Exceptions;
using StruLog.SM;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace StruLog.SM
{
    internal class TelegramSM : StringStoreManager, IBatchProcessingCompatible
    {
        internal const string NAME = "telegram";
        /// <summary>
        /// Без учета символов форматирования
        /// </summary>
        private const ushort POST_MAX_LENGTH = 4_096;
        internal static TelegramBotClient Client {get; private set;}
        internal TelegramHandlingIntensivity Intensivity { get; private set; }

        public BlockingCollection<LogData> ProcessingQueue { get; }

        public int[] AccessAttemptsDelays_mSeconds { get; }

        public int ProcessingQueueSize { get; }
        private static TelegramSM @this = null;
        private TelegramStore Config;

        private TelegramSM(TelegramStore config)
        {
            Config = config;
            ProcessingQueueSize = 100_000;
            ProcessingQueue = new BlockingCollection<LogData>(ProcessingQueueSize);
            AccessAttemptsDelays_mSeconds = new int[] { 25, 45, 90, 250, 500, 1000, 3000, 5000 };
            Logger = LoggersFactory.GetLogger<TelegramSM>(true);
            MinLogLevel = config.minLogLevel;
            if (Config.intensivity.enable)
                Intensivity = new TelegramHandlingIntensivity { PeriodLength = Config.intensivity.period };
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

        private async Task Log()
        {
            var logsBatchProcessor = new LogsBatchProccessor<string>(this, Config.outputPattern);
            Func<Task> ConnectTo_Func = () =>
            {
                Client = new TelegramBotClient(ConfigProvider.Config.stores.telegram.token);
                return Task.CompletedTask;
            };
            Func<string, LogData, Task> WriteLogEntryTo_Func = async (logEntry, logData) =>
            {
                await WriteTo(logEntry);
            };
            await logsBatchProcessor.Log_Type1Async(ConnectTo_Func, WriteLogEntryTo_Func);
        }

        protected override async Task WriteTo(object logEntry)
        {
            if (Config.intensivity.enable)
            {
                if (!CheckLoggingIntensivity())
                    return;
            }
            foreach (var chatId in Config.chatIds)
            {
                string tag = $"\n#{ConfigProvider.Config.projectName}";
                string log = (logEntry as string);
                if (log.Length + tag.Length > POST_MAX_LENGTH)
                {
                    log = log.Substring(0, POST_MAX_LENGTH - 4 - tag.Length) + "...";
                }
                log = log.Replace('<', '[').Replace('>', ']');
                await Client.SendTextMessageAsync(chatId, $"<code>{log}</code>{tag}", parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            }
            await Task.Delay(Config.sendingPeriod); //because TelegramBot work too slow
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if valid activity</returns>
        private bool CheckLoggingIntensivity()
        {
            Intensivity.TryMovePeriod();
            Intensivity.nLogEntriesPerPeriod ++;
            if (Intensivity.nLogEntriesPerPeriod > Config.intensivity.nMessagesPerPeriod)
                return false;
            return true;

        }

        internal override void TryLog(LogData logData)
        {
            try
            {
                ProcessingQueue.TryAdd(logData);
                //Trace.WriteLine(ProcessingQueue.Count);
            }
            catch (Exception ex)
            {
                Logger.Error($"Addition of logEntries to queue on processing is impossible. Likely, processing thread doesn't work. {ex.GetType()}:{ex.Message}");

            }
        }

        internal static StoreManager Init(TelegramStore config)
        {
            if (config is null)
                throw new StruLogConfigException($"Not found configuration for '{NAME}' store");
            if (@this == null)
            {
                @this = new TelegramSM(config);
            }
            return @this;
        }
    }
}
