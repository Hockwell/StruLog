using StruLog.SM;
using System.IO;

namespace StruLog
{
    public static class StruLogProvider
    {
        public static void Init(string configPath, bool inProjectDir = false, ConfiguringOptions options = null)
        {
            string path = inProjectDir ? $"{ Directory.GetCurrentDirectory() }/{configPath}" : configPath;
            ConfigProvider.Run(path, options);
            StoreManager.RunProcessing();
        }
    }
}
