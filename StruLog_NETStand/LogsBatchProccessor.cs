using StruLog.Entites;
using StruLog.Exceptions;
using StruLog.SM;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StruLog
{
    /// <summary>
    /// Обеспечивает различные варианты пакетной обработки логов. 1 storeManager - 1 экземпляр процессора.
    /// Если необходимо вставить др. методы в обработку, создайте новый метод, например Log_Type2, который будет принимать др. анонимные методы.
    /// 
    /// </summary>
    internal class LogsBatchProccessor<TLogEntry> where TLogEntry : class
    {
        /// <summary>
        /// Класс занимается проведением проверок к очереди обработки и логированием этой информации
        /// </summary>
        private class ProcessingQueueChecker
        {
            private const int QUEUE_WARNING_OCCUPIED_CAPACITY_PERCENT = 90;
            private DateTime logEntriesIgrnoring_startTime = default;
            private DateTime logEntriesIgrnoring_endTime = default;
            private bool IsSavedOfEntriesIgrnoringStartTime = false;
            private Logger StoreLogger;
            private BlockingCollection<LogData> Queue;
            public ProcessingQueueChecker(Logger storeLogger, BlockingCollection<LogData> queue)
            {
                StoreLogger = storeLogger;
                Queue = queue;
            }

            internal void CheckOccupiedCapacity()
            {
                float queueOccupiedCapacity = ((float)Queue.Count / (float)Queue.BoundedCapacity) * 100; //занятая ёмкость очереди
                //logger.Trace($"Размер очереди: {queueOccupiedCapacity}%");
                if (queueOccupiedCapacity > QUEUE_WARNING_OCCUPIED_CAPACITY_PERCENT)
                {
                    StoreLogger.Warn($"Queue capacity = {queueOccupiedCapacity}%! Processing is too slow!");
                    if (queueOccupiedCapacity == 100)
                    {
                        if (!IsSavedOfEntriesIgrnoringStartTime)
                        {
                            logEntriesIgrnoring_startTime = ConfigProvider.Config.currentTime_Func();
                            IsSavedOfEntriesIgrnoringStartTime = true;
                        }
                        logEntriesIgrnoring_endTime = ConfigProvider.Config.currentTime_Func();
                        TimeSpan ignoreTime = logEntriesIgrnoring_endTime - logEntriesIgrnoring_startTime;
                        StoreLogger.Error($"New logEntries not exporting during the {ignoreTime:g}");
                    }
                    else if (IsSavedOfEntriesIgrnoringStartTime)
                        IsSavedOfEntriesIgrnoringStartTime = false; //теперь если в след. раз заполненность достигнет 100%, рассчитается новое стартовое время
                }
            }
        }

        private IBatchProcessingCompatible StoreManager;
        private Logger StoreLogger;
        private object OutputPattern;

        public LogsBatchProccessor(IBatchProcessingCompatible storeManager, object outputPattern)
        {
            if (!(storeManager is StoreManager))
                throw new ArgumentException("'storeManager's class don't inherits from StoreManager class");
            StoreManager = storeManager;
            StoreLogger = (StoreManager as StoreManager).Logger;
            OutputPattern = outputPattern;
        }
        /// <summary>
        /// Creates logEntry for each store, uses connectFunc and writeToFunc for print to stores. Анонимные методы могут содержать как async, так и sync-код или быть вовсе пустыми
        /// </summary>
        /// <param name="ConnectTo_Func">Логика подключения к хранилищу</param>
        /// <param name="WriteLogEntryTo_Func">Логика экспорта лог-записи в хранилище</param>
        /// <returns></returns>
        internal async Task Log_Type1Async(Func<Task> ConnectTo_Func, Func<TLogEntry, LogData, Task> WriteLogEntryTo_Func)
        {
            var queueChecker = new ProcessingQueueChecker(StoreLogger, StoreManager.ProcessingQueue);
            int attemptNum = -1;
            while (true)
            {
                queueChecker.CheckOccupiedCapacity();
                try
                {
                    await ConnectTo_Func();
                    break;
                }
                catch (Exception ex)
                {
                    StoreLogger.Error($"Can't get initial access to store for beginning of processing. | {ex.GetType()}:{ex.Message}");
                    await Task.Delay(StoreManager.AccessAttemptsDelays_mSeconds[attemptNum < (StoreManager.AccessAttemptsDelays_mSeconds.Length - 1) ?
                        ++attemptNum : attemptNum]);
                }
            }

            queueChecker = new ProcessingQueueChecker(StoreLogger, StoreManager.ProcessingQueue);//новый отчёт
            TLogEntry logEntry;

            foreach (var logData in StoreManager.ProcessingQueue.GetConsumingEnumerable()) //Мониторит очередь
            {
                if (!(StoreManager as StoreManager).IsLoggingAllowed(logData.level))
                    continue;
                
                try
                {
                    logEntry = (StoreManager as StoreManager).CreateLogEntry(logData, OutputPattern) as TLogEntry;
                }
                catch (Exception ex)
                {
                    StoreLogger.Warn($"Something wrong during the creation of log entry. The log entry will be skipped. | {ex.GetType()}:{ex.Message}");
                    continue;
                }
                

                attemptNum = -1;

                while (true)
                {
                    queueChecker.CheckOccupiedCapacity();
                    try
                    {
                        await WriteLogEntryTo_Func(logEntry, logData);
                        break;
                    }
                    catch (Exception ex)
                    {
                        StoreLogger.Warn($"Access to store was interrupted. The queue was not unloaded. | {ex.GetType()}:{ex.Message}");
                        //останавливаемся на последнем эл-те delaysArray
                        await Task.Delay(StoreManager.AccessAttemptsDelays_mSeconds[attemptNum < StoreManager.AccessAttemptsDelays_mSeconds.Length - 1 ? ++attemptNum : attemptNum]);
                    }
                }
            }
        }
    }
}
