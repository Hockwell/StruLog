using StruLog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace StruLogDemo
{
    public class Program
    {
        private static readonly StruLog.Logger logger = StruLog.LoggersFactory.GetLogger(typeof(Program));

        static async Task Main(string[] args)
        {
            StruLog.StruLogProvider.Init(@"C:\Users\Владимир\OneDrive\Data\Dev\Src\20\StruLog\StruLogDemo\StruLog.json", false, new ConfiguringOptions { AddTelegramConsumers = false });
            var task1 = Task.Run(async () => {
                for (int i = 0; i < 15000; i++)
                {
                    try
                    {
                        lalka();
                    }
                    catch(Exception e)
                    {
                        logger.Info("message from Thread A", new { el = 5, zel = 6 }, e);
                        await Task.Delay(10);
                    }
                    
                }
            });
            var task2 = Task.Run(async () => {
                for (int i = 0; i < 15000; i++)
                {
                    try
                    {
                        logger.Info("message from Thread B", new { el = 67, zel = 434, lalalachka = "ki" });
                        await Task.Delay(10);
                        throw new Exception("my custom exception message");
                    }
                    catch (Exception e)
                    {
                        logger.Info("dfsdf", e);
                    }
                }
            });
            var task3 = Task.Run(async () => {
                for (int i = 0; i < 15000; i++)
                {
                    //logger.Warn("message from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread С");
                    logger.Warn(" ServerId: '{ ClusterId: 1, EndPoint: 'Unspecified/devils.dev:27017' }', EndPoint: 'Unspecified / devils.dev:27017', ReasonChanged: 'Heartbeat', State: 'Disconnected', ServerVersion: , TopologyVersion: , Type: 'Unknown', HeartbeatException: 'MongoDB.Driver.MongoConnectionException: An exception occurred while opening a connection to the server.--->");
                    await Task.Delay(10);
                }
            });

            await Task.WhenAll(new Task[] { task1, task2, task3 });
        }
        static void lalka()
        {
            throw new Exception("my custom exception message");
        }
    }
}
