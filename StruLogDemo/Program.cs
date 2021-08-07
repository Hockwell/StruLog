using Microsoft.Extensions.Logging;
using StruLog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace StruLogDemo
{
    public class Program
    {
        //private static readonly StruLog.Logger logger = StruLog.LoggersFactory.GetLogger(typeof(Program));
        private static readonly ILogger logger = StruLog.LoggersFactory.GetLogger(typeof(Program));

        static async Task Main(string[] args)
        {
            StruLog.StruLogProvider.Init(@"C:\Users\volnu\OneDrive\Data\Dev\Src\20\StruLog\StruLogDemo\StruLog.json", false);
            //StressTest();

            logger.Log<object>(LogLevel.Debug, default, new { el = 67, zel = 434, lalalachka = "ki" }, null, null);
            logger.Log<object>(LogLevel.Debug, new EventId(404), new { mulil = 5, poiwer = 6 }, new Exception(), null);
            logger.LogCritical(new EventId(404), "dfsdf");
            logger.LogCritical(new EventId(), "dfsdf");
            await Task.Delay(100000);
        }
        static async Task StressTest()
        {
            var task1 = Task.Run(async () => {
                for (int i = 0; i < 15000; i++)
                {
                    try
                    {
                        lalka();
                    }
                    catch (Exception e)
                    {
                        //logger.Info("message from Thread A", new { el = 5, zel = 6 }, e);
                        logger.LogInformation("message from Thread A", new { el = 5, zel = 6 });
                        await Task.Delay(10);
                    }

                }
            });
            var task2 = Task.Run(async () => {
                for (int i = 0; i < 15000; i++)
                {
                    try
                    {
                        //logger.Info("message from Thread B", new { el = 67, zel = 434, lalalachka = "ki" });
                        logger.LogInformation("message from Thread B");

                        await Task.Delay(10);
                        throw new Exception("my custom exception message");
                    }
                    catch (Exception e)
                    {
                        //logger.Info("dfsdf", e);
                        logger.LogCritical(new EventId(404, "babaika is dead"), e, "dfsdf");
                    }
                }
            });
            var task3 = Task.Run(async () => {
                for (int i = 0; i < 15000; i++)
                {
                    //logger.Warn("message from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread Сmessage from Thread С");
                    //logger.Warn(" ServerId: '{ ClusterId: 1, EndPoint: 'Unspecified/ghjghjghj' }', EndPoint: 'Unspecified / tyutyityujtyjghjghj', ReasonChanged: 'Heartbeat', State: 'Disconnected', ServerVersion: , TopologyVersion: , Type: 'Unknown', HeartbeatException: 'MongoDB.Driver.MongoConnectionException: An exception occurred while opening a connection to the server.--->");
                    logger.LogWarning("ServerId: '{ ClusterId: 1, EndPoint: 'Unspecified/ghjghjghj' }', EndPoint: 'Unspecified / tyutyityujtyjghjghj', ReasonChanged: 'Heartbeat', State: 'Disconnected', ServerVersion: , TopologyVersion: , Type: 'Unknown', HeartbeatException: 'MongoDB.Driver.MongoConnectionException: An exception occurred while opening a connection to the server.--->");
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
