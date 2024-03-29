﻿using MongoDB.Driver;
using StruLog.Entites;
using StruLog.Entites.Stores;
using StruLog.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace StruLog.SM
{
    internal class MongoDbSM : DbStoreManager, IBatchProcessingCompatible
    {
        internal const string NAME = "mongoDB";
        public BlockingCollection<LogData> ProcessingQueue { get; }
        public int[] AccessAttemptsDelays_mSeconds { get; }
        public int ProcessingQueueSize { get; }

        private static MongoDbSM @this = null;
        private MongoClient client;
        private IMongoCollection<LogDataModel> logsCollection;
        private MongoDBStore Config;

        private MongoDbSM(MongoDBStore config)
        {
            client = new MongoClient(config.connectionString);
            var db = client.GetDatabase(config.dbName);
            logsCollection = db.GetCollection<LogDataModel>(config.collectionName);
            //Если она не существует, неявно создастся
            Config = config;
            ProcessingQueueSize = 1_000_000;
            ProcessingQueue = new BlockingCollection<LogData>(ProcessingQueueSize);
            AccessAttemptsDelays_mSeconds = new int[] { 25, 45, 90, 250, 500, 1000, 3000, 5000 };
            Logger = LoggersFactory.GetLogger<MongoDbSM>(true);
            MinLogLevel = config.minLogLevel;
        }
        internal static MongoDbSM Init(MongoDBStore config)
        {
            if (config is null)
                throw new StruLogConfigException($"Not found configuration for '{NAME}' store");
            if (@this == null)
            {
                @this = new MongoDbSM(config);
            }
            return @this;
        }

        /// <summary>
        /// Central method for logging to store
        /// </summary>
        /// <returns></returns>
        private async Task Log()
        {
            var logsBatchProcessor = new LogsBatchProccessor<LogDataModel>(this, Config.outputPattern);
            Func<Task> ConnectTo_Func = () =>
            {
                ConnectTo();
                return Task.CompletedTask;
            };
            Func<LogDataModel, LogData, Task> WriteLogEntryTo_Func = (logEntry, logData) =>
            {
                WriteTo(logEntry);
                return Task.CompletedTask;
            };
            await logsBatchProcessor.Log_Type1Async(ConnectTo_Func, WriteLogEntryTo_Func);

        }

        private void ConnectTo()
        {
            client.StartSession();
        }
        internal override void TryLog(LogData logData)
        {
            //Trace.WriteLine("processingQueue=" + ProcessingQueue.Count);
            try
            {
                ProcessingQueue.TryAdd(logData);
            }
            catch (Exception ex)
            {
                Logger.Error($"Addition of logEntries to queue on processing is impossible. Likely processing thread doesn't working. {ex.GetType()}:{ex.Message}");

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

        protected override Task WriteTo(object logEntryObj)
        {
            if (logEntryObj is LogDataModel model)
                logsCollection.InsertOne(model);
            else
                throw new StruLogException("Detected LogEntry with invalid type. Check your code.");
            return Task.CompletedTask;

        }
    }
}
