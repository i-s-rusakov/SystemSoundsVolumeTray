using System.IO;
using System.Text.Json;

namespace SystemSoundsVolumeTray
{
    public class AppConfig
    {
        public int Volume { get; set; } = 100;

        private static readonly string configPath = Path.Combine(AppContext.BaseDirectory, "config.json");

        public static AppConfig Load()
        {
            if (File.Exists(configPath))
            {
                try
                {
                    string json = File.ReadAllText(configPath);
                    return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
                }
                catch
                {
                    // В случае ошибки чтения возвращаем конфиг по умолчанию
                    return new AppConfig();
                }
            }
            return new AppConfig();
        }

        public void Save()
        {
            try
            {
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save config: {ex.Message}");
            }
        }
    }
}
