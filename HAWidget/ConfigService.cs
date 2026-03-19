using System;
using System.IO;
using System.Text.Json;

namespace HAWidget
{
    public static class ConfigService
    {
        private static readonly string ConfigPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

        public static AppConfig LoadConfig()
        {
            if (!File.Exists(ConfigPath))
            {
                AppConfig defaultConfig = CreateDefaultConfig();
                SaveConfig(defaultConfig);
                return defaultConfig;
            }

            string json = File.ReadAllText(ConfigPath);

            AppConfig? config = JsonSerializer.Deserialize<AppConfig>(json);

            if (config == null)
            {
                AppConfig defaultConfig = CreateDefaultConfig();
                SaveConfig(defaultConfig);
                return defaultConfig;
            }

            return config;
        }

        public static void SaveConfig(AppConfig config)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(config, options);
            File.WriteAllText(ConfigPath, json);
        }

        private static AppConfig CreateDefaultConfig()
        {
            AppConfig config = new AppConfig
            {
                HaBaseUrl = "http://127.0.0.1:8123",
                Token = "",
            };

            WindowGroupConfig group = new WindowGroupConfig
            {
                Title = "环境信息",
                WindowLevel = "TopMost",
                Left = 100,
                Top = 100,
                Width = 320,
                Height = 240
            };

            group.Entities.Add(new EntityConfig
            {
                Title = "客厅温度",
                EntityId = "sensor.living_room_temperature",
                CustomUnit = "°C"
            });

            config.WindowGroups.Add(group);

            return config;
        }
    }
}