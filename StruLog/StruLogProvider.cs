using StruLog.SM;
using System.IO;

namespace StruLog
{
    public static class StruLogProvider
    {
        public static void Init(string configPath, bool inProjectDir = false)
        {
            string path = inProjectDir ? $"{ Directory.GetCurrentDirectory() }/{configPath}" : configPath;
            var configFileContent = File.ReadAllText(path);
            ConfigFileProvider.Parse(configFileContent);
            StoreManager.RunProcessing();
        }
    }
}
