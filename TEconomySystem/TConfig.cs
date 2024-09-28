using TShockAPI;
using Newtonsoft.Json;

namespace TEconomySystem
{
    public class Config
    {
        public string ConnectionString { get; set; } = "server=localhost;user=root;database=teconomy;port=3306;password=your_password";
        public decimal TaxRate { get; set; } = 0.05m;
        public string Economyname { get; set; } = "{ConfigManager.ConfigData.Economyname}";
    }

    public static class ConfigManager
    {
        private static readonly string configDirectory = Path.Combine("tshock", "TSuite", "TEconomySystem");
        private static readonly string configFilePath = Path.Combine(configDirectory, "appsettings.json");
        public static Config ConfigData { get; private set; }

        public static void LoadConfig()
        {
            EnsureConfigFileExists();
            string configJson = File.ReadAllText(configFilePath);
            ConfigData = JsonConvert.DeserializeObject<Config>(configJson);
        }

        private static void EnsureConfigFileExists()
        {
            // Check if directory exists, create if missing
            if (!Directory.Exists(configDirectory))
            {
                Directory.CreateDirectory(configDirectory);
            }

            // Check if file exists, create with default values if missing
            if (!File.Exists(configFilePath))
            {
                var defaultConfig = new Config();
                string json = JsonConvert.SerializeObject(defaultConfig, Formatting.Indented);
                File.WriteAllText(configFilePath, json);
                TShock.Log.ConsoleInfo("[TEconomySystem] Created default appsettings.json.");
            }
        }
    }
}
