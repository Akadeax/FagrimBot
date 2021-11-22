using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FagrimBot.Core.Managers
{
    public static class ConfigManager
    {
        private const string CONFIG_FOLDER = "Resources";
        private const string CONFIG_FILE = " config.json";
        private const string CONFIG_PATH = $"{CONFIG_FOLDER}/{CONFIG_FILE}";

        public static BotConfig Config { get; private set; }

        static ConfigManager()
        {
            if (!File.Exists(CONFIG_PATH))
            {
                Config = new BotConfig();
                var json = JsonConvert.SerializeObject(Config, Formatting.Indented);
                File.WriteAllText(CONFIG_PATH, json);
            }
            else
            {
                var json = File.ReadAllText(CONFIG_PATH);
                Config = JsonConvert.DeserializeObject<BotConfig>(json);
            }
        }
    }

    public struct BotConfig
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("prefix")]
        public string Prefix { get; set; }
    }
}
