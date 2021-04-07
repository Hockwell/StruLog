using StruLog.Entites;
using System.Collections.Concurrent;

namespace StruLog
{
    /// <summary>
    ///что является характерным для пакетных обработчиков логов (SM-ов)
    /// </summary>
    internal interface IBatchProcessingCompatible
    {
        /// <summary>
        /// Запуск пакетной обработки
        /// </summary>
        public void RunBatchProcessing();
        /// <summary>
        /// очередь логов
        /// </summary>
        public BlockingCollection<LogData> ProcessingQueue { get; }
        /// <summary>
        /// задержки при затруднении доступа, чтобы слишком часто не опрашивать хранилище
        /// </summary>
        public int[] AccessAttemptsDelays_mSeconds { get; }
        public int ProcessingQueueSize { get; }
    }
}
