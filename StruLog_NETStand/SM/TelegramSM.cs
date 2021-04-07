using StruLog;
using StruLog.Entites;
using StruLog.Entites.Stores;
using StruLog.SM;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;

namespace StruLog.SM
{
    class TelegramSM : StringStoreManager, IBatchProcessingCompatible
    {
        internal const string NAME = "telegram";
        internal static TelegramBotClient Client {get; private set;}

        public BlockingCollection<LogData> ProcessingQueue => throw new NotImplementedException();

        public int[] AccessAttemptsDelays_mSeconds => throw new NotImplementedException();

        public int ProcessingQueueSize => throw new NotImplementedException();

        public void RunBatchProcessing()
        {
            
        }

        protected override void WriteTo(object logEntry)
        {
            
        }

        internal override void TryLog(LogData logData)
        {
            
        }

        internal static StoreManager Init(TelegramStore config)
        {
            Client = new TelegramBotClient(config.token);
            return null;
        }
    }
}
