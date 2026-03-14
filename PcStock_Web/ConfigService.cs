using System.Text.Json;

namespace PcStock_Web
{
    // 1. تعريف الكلاس الذي يجمع كل الإعدادات (هذا يحل خطأ CS0246)
    public class AppSettings
    {
        public string DbPath { get; set; } = @"C:\PCSTOCK\2026";
        public string Direction { get; set; } = "";
        public string Departement { get; set; } = "";
        public string Service { get; set; } = "";
        public string Adresse { get; set; } = "";
        public string Email { get; set; } = "";
    }

    public class ConfigService
    {
        private readonly string _configPath;

        public ConfigService(IWebHostEnvironment env)
        {
            string folder = Path.Combine(env.ContentRootPath, "App_Data");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            _configPath = Path.Combine(folder, "app_settings.json");
        }

        // 2. دالة جلب كل الإعدادات (هذا يحل خطأ CS1061)
        public AppSettings GetAllSettings()
        {
            if (!File.Exists(_configPath)) return new AppSettings();

            try
            {
                var json = File.ReadAllText(_configPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        // 3. دالة حفظ كل الإعدادات (هذا يحل خطأ CS1061)
        public void SaveAllSettings(AppSettings settings)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(_configPath, json);
        }

        // دالة قديمة للحفاظ على التوافق مع الصفحات السابقة (مثل Liste Articles)
        public string GetDbPath()
        {
            return GetAllSettings().DbPath;
        }
    }
}

//using System.Text.Json;

//namespace PcStock_Web
//{
//    public class ConfigService
//    {
//        private readonly string _configPath;
//        public ConfigService(IWebHostEnvironment env)
//        {
//            string folder = Path.Combine(env.ContentRootPath, "App_Data");
//            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
//            _configPath = Path.Combine(folder, "app_settings.json");
//        }

//        public string GetDbPath()
//        {
//            if (!File.Exists(_configPath)) return @"C:\PCSTOCK\2026";
//            var json = File.ReadAllText(_configPath);
//            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
//            return data["DbPath"];
//        }

//        public void SaveDbPath(string path)
//        {
//            var data = new Dictionary<string, string> { { "DbPath", path } };
//            File.WriteAllText(_configPath, JsonSerializer.Serialize(data));
//        }


//    }
//}