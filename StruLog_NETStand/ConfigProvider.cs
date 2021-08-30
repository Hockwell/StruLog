using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StruLog.Entites;
using StruLog.Entites.Stores;
using StruLog.Exceptions;
using StruLog.SM;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;
using static StruLog.StringTools;


namespace StruLog
{
    /// <summary>
    /// Обеспечивает определение настроек логгера, например методом парсинга конфиг-файла
    /// </summary>
    internal static class ConfigProvider
    {
        private static JObject jsonDoc;
        private static string configFilePath;
        private static ConfiguringOptions options;
        internal static Config Config { get; private set; } = new Config();

        /// <summary>
        /// Центральный метод определения настроек логгера
        /// </summary>
        internal static void Run(string path, ConfiguringOptions options = null)
        {
            configFilePath = path;
            ConfigProvider.options = options;
            var configFileContent = System.IO.File.ReadAllText(path);

            try
            {
                jsonDoc = JObject.Parse(configFileContent);
                Config.stores = new StoreSettings();
                ParseProjectName();
                ParseUsingStores();
                ParseTime();
                ParseInsideLoggingStore();
                jsonDoc = null;
            }
            catch (Exception ex)
            {
                throw new StruLogConfigException($"Can't parse config or write to: {ex.Message} {ex.StackTrace} \n >>> Check access to config file*, config content*, internet access");
            }

        }

        private static void ParseProjectName()
        {
            string value = jsonDoc.Value<string>(nameof(Config.projectName));
            if (string.IsNullOrEmpty(value))
                throw new StruLogConfigException($"{nameof(Config.projectName)} must be not null and not empty");
            Config.projectName = value;
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
                collectionName = mongoDB.Value<string>(nameof(MongoDBStore.collectionName)).Replace("{projectName}",Config.projectName),
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
            string timeRaw = jsonDoc.Value<string>("time");
            TimeRepresentation time = timeRaw.StringToEnum<TimeRepresentation>();
            Config.currentTime_Func = GetTimeFunc();

            Func<DateTime> GetTimeFunc()
            {
                switch (time)
                {
                    case TimeRepresentation.LOCAL:
                        return () => DateTime.Now;
                    case TimeRepresentation.UTC:
                        return () => DateTime.UtcNow;
                    default:
                        Console.WriteLine("StruLog: Time format not found in config, will be set UTC format.");
                        return () => DateTime.UtcNow;
                }
            }
        }

        private static void ParseInsideLoggingStore()
        {
            var storeManagerConfigName = jsonDoc.Value<string>(nameof(Config.insideLoggingStore));
            Config.insideLoggingStore = InitStoreManager(storeManagerConfigName);
        }

        /// <summary>
        /// Инициализирует указанный SM
        /// </summary>
        /// <param name="storeNameFromConfig"></param>
        /// <returns></returns>
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
            else if (CompareStrings(storeNameFromConfig, TelegramSM.NAME))
            {
                if (Config.stores.telegram is null)
                {
                    Config.stores.telegram = DefineTelegramStoreSettings();
                }
                return TelegramSM.Init(Config.stores.telegram);
            }
            else
                throw new StruLogConfigException($"Store '{storeNameFromConfig}' not found");
        }

        /// <summary>
        /// Парсинг из конфига, временное прослушивание телеграм-бота, запись настроек в конфиг
        /// </summary>
        /// <returns></returns>
        private static TelegramStore DefineTelegramStoreSettings()
        {
            var stores = jsonDoc.SelectToken(nameof(Config.stores));

            var telegramFileConfig = stores.SelectToken(TelegramSM.NAME);
            var telegramStoreConfig = new TelegramStore
            {
                minLogLevel = telegramFileConfig.Value<string>(nameof(Store.minLogLevel)).StringToEnum<LogLevel>(),
                outputPattern = StringStoreManager.GetOutputActions(telegramFileConfig.Value<string>(nameof(TelegramStore.outputPattern))),
                token = telegramFileConfig.Value<string>(nameof(TelegramStore.token)),
            };

            int sendingPeriod = telegramFileConfig.Value<int>(nameof(TelegramStore.sendingPeriod));
            if (sendingPeriod < 1000)
            {
                Console.WriteLine(@"StruLog: telegram/sendingPeriod (see config) must be >= 1000, will be 3000");
                telegramStoreConfig.sendingPeriod = 3000;
            }
            else
                telegramStoreConfig.sendingPeriod = sendingPeriod;

            var intensivity = telegramFileConfig.SelectToken(nameof(TelegramStore.intensivity));
            if (intensivity?.HasValues ?? false)
                telegramStoreConfig.intensivity = new TelegramStore.IntensivityControl
                {
                    enable = intensivity.Value<bool>(nameof(TelegramStore.intensivity.enable)),
                    nMessagesPerPeriod = intensivity.Value<long>(nameof(TelegramStore.intensivity.nMessagesPerPeriod)),
                    period = TimeSpan.Parse(intensivity.Value<string>(nameof(TelegramStore.intensivity.period)))
                };
            else
                telegramStoreConfig.intensivity = new TelegramStore.IntensivityControl { enable = false };

            var chatIdsFromConfig = telegramFileConfig.SelectToken("chats");
            HashSet<long> chatIds;
            if (chatIdsFromConfig.HasValues)
            {
                chatIds = NewtonsoftJsonTools.ConvertJTokenToIEnumerable<long>(chatIdsFromConfig).ToHashSet();
            }
            else
                chatIds = new HashSet<long>();

            if (chatIds.Count == 0 || (options?.AddTelegramConsumers ?? false))
            {
                DefineTelegramChatId(telegramFileConfig, telegramStoreConfig, chatIds);
                SaveChangesToConfig();
            }
            else
                telegramStoreConfig.chatIds = chatIds.ToImmutableList();

            return telegramStoreConfig;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="telegramConfig"></param>
        /// <param name="config"></param>
        /// <param name="chatIds">Must contents Ids from config</param>
        private static void DefineTelegramChatId(JToken telegramConfig, TelegramStore config, HashSet<long> chatIds)
        {
            ReceiveChatIdsFromBot(); //adds to chatIds
            if (chatIds.Count == 0)
                throw new StruLogConfigException("Telegram store wasn't configure. Repeat it or turn off Telegram store in config");
            telegramConfig["chats"] = NewtonsoftJsonTools.ConvertIEnumerableToJToken(chatIds);
            config.chatIds = chatIds.ToImmutableList();

            
            void ReceiveChatIdsFromBot()
            {
                TelegramBotClient client = new TelegramBotClient(config.token);
                Console.WriteLine("You enabled Telegram Store, require tuning (if it was by error, stop execution and disable telegram store using in config). \n - Send messages to your TelegramBot from required Telegram accounts. \n - StruLog will determines chatId for each account and saves this to config file. \n - For tuning finish you should send inTelegram command 'finish_tuning' OR stop execution. \n - WARNING: not only you can get access to your telegramBot now, attentionally see to current log, that only valid users got access to project logs. \n - The process of remembering of users:");

                int updatesOffset = 0;
                while (true)
                {
                    Update[] updates = null;
                    try
                    {
                        updates = client.GetUpdatesAsync(offset: updatesOffset).Result;
                        foreach (var update in updates)
                        {
                            Message msg = update.Message;
                            Console.WriteLine($"Detected user @{msg.From.Username} ({msg.From.FirstName} {msg.From.LastName}) with chatId:{msg.Chat.Id}");
                            updatesOffset = update.Id;
                            switch (msg.Text)
                            {
                                case "finish_tuning":
                                    //if (msg.Date.AddSeconds(10) >= DateTime.UtcNow) //block probable fake requests
                                    //{
                                    //    Console.WriteLine(@">>> finish_tuning applied. If you have problems with fake finish_tuning requests set true system time");

                                    //    return;
                                    //}
                                    client.GetUpdatesAsync(offset: updatesOffset + 1).Wait();
                                    return;
                                default:
                                    RememberNewTelegramChat(msg.Chat.Id, chatIds);
                                    break;
                            }
                            
                        }
                    }
                    catch (Exception e)
                    {
                        throw new StruLogException($"Telegram Long Polling unable: {e.Message} {e.StackTrace}");
                    }
                    finally
                    {
                        if (updates != null && updates.Length > 0)
                            updatesOffset++;
                    }
                }

                //throw new StruLogException("Unable define chatId for telegram. You must send only /start command. Leave from chat with bot and delete it, repeat.");
            }
        }
        /// <summary>
        /// Remember new chats, that was opened by users for logs receiving
        /// </summary>
        private static void RememberNewTelegramChat(long chatId, HashSet<long> chatIds)
        {
            chatIds.Add(chatId);
        }

        private static void SaveChangesToConfig()
        {
            System.IO.File.WriteAllText(configFilePath,jsonDoc.ToString(Formatting.Indented));
            
        }
    }

    /// <summary>
    /// Options for change initiation process of configuration 
    /// </summary>
    public class ConfiguringOptions
    {
        public bool AddTelegramConsumers { get; set; } = false;
    }

}
