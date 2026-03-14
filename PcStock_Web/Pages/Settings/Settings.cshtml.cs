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

        [BindProperty] public string DbPath { get; set; }
        [BindProperty] public IFormFile? LogoFile { get; set; }

        // الحقول الجديدة لبيانات الشركة
        [BindProperty] public string Direction { get; set; }
        [BindProperty] public string Departement { get; set; }
        [BindProperty] public string Service { get; set; }
        [BindProperty] public string Adresse { get; set; }
        [BindProperty] public string Email { get; set; }

        public void OnGet()
        {
            // تحميل كل البيانات المحفوظة من الخدمة
            var settings = _configService.GetAllSettings();
            DbPath = settings.DbPath;
            Direction = settings.Direction;
            Departement = settings.Departement;
            Service = settings.Service;
            Adresse = settings.Adresse;
            Email = settings.Email;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // حفظ البيانات النصية في ملف JSON عبر الخدمة
            _configService.SaveAllSettings(new AppSettings
            {
                DbPath = DbPath,
                Direction = Direction,
                Departement = Departement,
                Service = Service,
                Adresse = Adresse,
                Email = Email
            });

            // معالجة اللوغو
            if (LogoFile != null) { /* ... كود رفع الصورة ... */ }

            // مزامنة SQLite
            await _sqliteService.SyncTables(new List<string> { "ST_STOCK", "ST_ACHAT", "ST_FOURN", "ST_CESS", "ST_CESSR", "ST_CONSO", "ST_SORTI", "ST_UNITE", "ST_FAMI", "ST_FAGRP", "ST_SERVI" });

            TempData["SuccessMessage"] = "Toutes les configurations ont ete enregistrees !";
            return Page();
        }
    }
}