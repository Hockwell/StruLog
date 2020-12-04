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
            StruLog.StruLogProvider.Init("StruLog.json", true);
            var task1 = Task.Run(async () => {
                for (int i = 0; i < 15000; i++)
                {
                    try
                    {
                        throw new Exception("my custom exception message");
                    }
                    catch(Exception e)
                    {
                        logger.Debug("message from Thread A", new { el = 5, zel = 6 }, e);
                        await Task.Delay(500);
                    }
                    
                }
            });
            var task2 = Task.Run(async () => {
                for (int i = 0; i < 15000; i++)
                {
                    logger.Info("message from Thread B", new { el = 67, zel = 434, lalalachka = "ki" });
                    await Task.Delay(500);
                }
            });

            await Task.WhenAll(new Task[] { task1, task2 });
        }
    }
}
