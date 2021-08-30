using StruLog.Entites.Stores;

namespace StruLog.Entites
{
    internal class StoreSettings
    {
        internal ConsoleStore console;
        internal FileStore file;
        internal MongoDBStore mongoDB;
        internal TelegramStore telegram;
    }
}
