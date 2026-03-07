//using System.Text.Json;

//namespace PcStock_Web
//{
//    public class ConfigService
//    {
//        private readonly string _configPath;

//        public ConfigService(IWebHostEnvironment env)
//        {
//            // حفظ الإعدادات في مجلد App_Data ليكون محمياً
//            string folder = Path.Combine(env.ContentRootPath, "App_Data");
//            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
//            _configPath = Path.Combine(folder, "app_settings.json");
//        }

//        public string GetDbPath()
//        {
//            if (!File.Exists(_configPath)) return @"C:\PCSTOCK\2026"; // مسار افتراضي
//            var json = File.ReadAllText(_configPath);
//            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
//            return data.ContainsKey("DbPath") ? data["DbPath"] : @"C:\PCSTOCK\2026";
//        }

//        public void SaveDbPath(string path)
//        {
//            var data = new Dictionary<string, string> { { "DbPath", path } };
//            var json = JsonSerializer.Serialize(data);
//            File.WriteAllText(_configPath, json);
//        }
//    }
//}

using System.Text.Json;

namespace PcStock_Web
{
    public class ConfigService
    {
        private readonly string _configPath;
        public ConfigService(IWebHostEnvironment env)
        {
            string folder = Path.Combine(env.ContentRootPath, "App_Data");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            _configPath = Path.Combine(folder, "app_settings.json");
        }

        public string GetDbPath()
        {
            if (!File.Exists(_configPath)) return @"C:\PCSTOCK\2026";
            var json = File.ReadAllText(_configPath);
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            return data["DbPath"];
        }

        public void SaveDbPath(string path)
        {
            var data = new Dictionary<string, string> { { "DbPath", path } };
            File.WriteAllText(_configPath, JsonSerializer.Serialize(data));
        }
    }
}