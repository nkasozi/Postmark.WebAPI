using Akka.Configuration;
using System.IO;

namespace Postmark.WebAPI
{
    public class ConfigurationLoader
    {
        public static Config Load() => LoadConfig("akka.conf");

        private static Config LoadConfig(string configFile)
        {
            if (File.Exists(configFile))
            {
                string config = File.ReadAllText(configFile);
                return ConfigurationFactory.ParseString(config);
            }


            return Config.Empty;
        }
    }
}
