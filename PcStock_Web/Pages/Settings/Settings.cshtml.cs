using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PcStock_Web.Pages.Settings
{
    public class SettingsModel : PageModel
    {
        private readonly ConfigService _configService;
        private readonly SqliteDbService _sqliteService;
        private readonly IWebHostEnvironment _environment;

        public SettingsModel(ConfigService configService, SqliteDbService sqliteService, IWebHostEnvironment environment)
        {
            _configService = configService;
            _sqliteService = sqliteService;
            _environment = environment;
        }

        [BindProperty]
        public string DbPath { get; set; }

        [BindProperty]
        public IFormFile? LogoFile { get; set; }

        public void OnGet()
        {
            DbPath = _configService.GetDbPath();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // 1. حفظ المسار في الإعدادات
            if (!string.IsNullOrEmpty(DbPath))
            {
                _configService.SaveDbPath(DbPath);
            }

            // 2. معالجة رفع الشعار
            if (LogoFile != null && LogoFile.Length > 0)
            {
                string folderPath = Path.Combine(_environment.WebRootPath, "images");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                string filePath = Path.Combine(folderPath, "logo_entreprise.png");
                using var stream = new FileStream(filePath, FileMode.Create);
                await LogoFile.CopyToAsync(stream);
            }

            // 3. المزامنة التلقائية لأهم الجداول
            var tablesToSync = new List<string> { "ST_STOCK", "ST_ACHAT", "ST_FOURN", "ST_CESS", "ST_UNITE", "ST_CONSO" };
            var result = await _sqliteService.SyncTables(tablesToSync);

            if (result.success)
                TempData["SuccessMessage"] = "Paramètres enregistrés et données synchronisées dans SQLite !";
            else
                TempData["ErrorMessage"] = result.message;

            return Page();
        }
    }
}