using Newtonsoft.Json.Linq;
using StruLog.Entites;
using StruLog.Entites.Stores;
using StruLog.Exceptions;
using StruLog.SM;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static StruLog.StringTools;


namespace StruLog
{
    internal static class ConfigFileProvider
    {
        private static JObject jsonDoc;
        internal static Config Config { get; private set; } = new Config();

        internal static void Parse(string configFileContent)
        {
            try
            {
                jsonDoc = JObject.Parse(configFileContent);
                Config.stores = new StoresSettings();
                ParseUsingStores();
                ParseTime();
                ParseInsideLoggingStore();
            }
            catch (Exception ex)
            {
                throw new StruLogConfigException($"Can't parse config: {ex.Message} {ex.StackTrace}");
            }

        }
        private static ConsoleStore ParseConsoleStoreSettings()
        {
            var stores = jsonDoc.SelectToken(nameof(Config.stores));

            var console = stores.SelectToken(ConsoleSM.NAME);
            return new ConsoleStore
            {
                minLogLevel = console.Value<string>(nameof(ConsoleStore.minLogLevel)).StringToEnum<LogLevel>(),
                outputPattern = StringStoreManager.GetOutputActions(console.Value<string>(nameof(ConsoleStore.outputPattern)))
            };
        }
        private static FileStore ParseFileStoreSettings()
        {
            var stores = jsonDoc.SelectToken(nameof(Config.stores));

            var file = stores.SelectToken(FileSM.NAME);
            var fileStore = new FileStore
            {
                path = file.Value<string>(nameof(FileStore.path)),
                minLogLevel = file.Value<string>(nameof(FileStore.minLogLevel)).StringToEnum<LogLevel>(),
                outputPattern = StringStoreManager.GetOutputActions(file.Value<string>(nameof(FileStore.outputPattern))),
                recreationPeriodInDays = file.Value<int>(nameof(FileStore.recreationPeriodInDays))
            };
            if (fileStore.recreationPeriodInDays <= 0 || fileStore.recreationPeriodInDays > 365)
                throw new StruLogConfigException($"{nameof(fileStore.recreationPeriodInDays)} must be in interval [1,365]");
            return fileStore;
        }
        private static MongoDBStore ParseMongoDbStoreSettings()
        {
            var stores = jsonDoc.SelectToken(nameof(Config.stores));

            var mongoDB = stores.SelectToken(MongoDbSM.NAME);
            var mongoDBStore = new MongoDBStore
            {
                connectionString = mongoDB.Value<string>(nameof(MongoDBStore.connectionString)),
                minLogLevel = mongoDB.Value<string>(nameof(MongoDBStore.minLogLevel)).StringToEnum<LogLevel>(),
                outputPattern = DbStoreManager.GetOutputActions(mongoDB.Value<string>(nameof(MongoDBStore.outputPattern))),
                collectionName = mongoDB.Value<string>(nameof(MongoDBStore.collectionName)),
                dbName = mongoDB.Value<string>(nameof(MongoDBStore.dbName))
            };
            return mongoDBStore;
        }

        private static void ParseUsingStores()
        {
            var usingStores = jsonDoc.SelectToken(nameof(Config.usingStores));
            var list = new List<StoreManager>();
            for (int i = 0; i < usingStores.Count(); i++)
            {
                var val = (string)usingStores[i];
                list.Add(InitStoreManager(val));
            }
            Config.usingStores = list.ToImmutableList();
        }
        private static void ParseTime()
        {
            string time = jsonDoc.Value<string>(nameof(Config.time));
            Config.time = time.StringToEnum<TimeRepresentation>();
        }

        private static void ParseInsideLoggingStore()
        {
            var storeManagerConfigName = jsonDoc.Value<string>(nameof(Config.insideLoggingStore));
            Config.insideLoggingStore = InitStoreManager(storeManagerConfigName);
        }

        private static StoreManager InitStoreManager(string storeNameFromConfig)
        {
            if (CompareStrings(storeNameFromConfig, ConsoleSM.NAME))
            {
                if (Config.stores.console is null)
                {
                    Config.stores.console = ParseConsoleStoreSettings();
                }
                return ConsoleSM.Init(Config.stores.console);
            }
            else if (CompareStrings(storeNameFromConfig, FileSM.NAME))
            {
                if (Config.stores.file is null)
                {
                    Config.stores.file = ParseFileStoreSettings();
                }
                return FileSM.Init(Config.stores.file);
            }
            else if (CompareStrings(storeNameFromConfig, MongoDbSM.NAME))
            {
                if (Config.stores.mongoDB is null)
                {
                    Config.stores.mongoDB = ParseMongoDbStoreSettings();
                }
                return MongoDbSM.Init(Config.stores.mongoDB);
            }
            else
                throw new StruLogConfigException($"Store '{storeNameFromConfig}' not found");
        }
    }



}
