using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.Odbc;
using Microsoft.AspNetCore.Hosting; // ضروري للوصول لمجلد الصور

namespace PcStock_Web.Pages.Settings
{
    public class SettingsModel : PageModel
    {  
        private readonly ConfigService _configService;
        public SettingsModel(ConfigService configService) { _configService = configService; }

        [BindProperty]
        public string DbPath { get; set; }

        // داخل كلاس SettingsModel
        private readonly IWebHostEnvironment _environment;
         

        [BindProperty]
        public IFormFile? LogoFile { get; set; } // لاستقبال ملف الصورة

        
        public async Task<IActionResult> OnPostAsync()
        {
            if (!string.IsNullOrEmpty(DbPath))
            {
                // 1. حفظ مسار قاعدة البيانات باستخدام الخدمة
                _configService.SaveDbPath(DbPath);
            }

            // 2. معالجة رفع الشعار (إذا تم اختيار صورة)
            if (LogoFile != null && LogoFile.Length > 0)
            {
                var folderPath = Path.Combine(_environment.WebRootPath, "images");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                var filePath = Path.Combine(folderPath, "logo_entreprise.png");

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await LogoFile.CopyToAsync(stream);
                }
            }

            TempData["SuccessMessage"] = "Paramètres enregistrés avec succès !";
            return Page();

            //// 1. هنا يمكنك حفظ مسار DbPath في قاعدة البيانات الخاصة بتطبيقك
            //// string pathTosaver = DbPath; 

            //// 2. معالجة رفع الشعار (Logo)
            //if (LogoFile != null)
            //{
            //    var folderPath = Path.Combine(_environment.WebRootPath, "images");
            //    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            //    var filePath = Path.Combine(folderPath, "logo_entreprise.png"); // اسم ثابت أو ديناميكي

            //    using (var stream = new FileStream(filePath, FileMode.Create))
            //    {
            //        await LogoFile.CopyToAsync(stream);
            //    }
            //}

            //// إضافة رسالة نجاح ليقرأها الـ JavaScript
            //TempData["SuccessMessage"] = "Les modifications ont ete enregistrees avec succes !";

            //return Page();
        }

        public void OnGet()
        {
            // هنا يتم تحميل البيانات عند فتح الصفحة أول مرة
            DbPath = _configService.GetDbPath();
        }

        // أكشن لاختبار الاتصال (يُسمى Handler في Razor Pages)
        public IActionResult OnPostTestConnection(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return new JsonResult(new { success = false, message = "Veuillez saisir un chemin." });

            try
            {
                if (!Directory.Exists(path))
                    return new JsonResult(new { success = false, message = "Répertoire introuvable." });

                string[] dbfFiles = Directory.GetFiles(path, "*.dbf");
                if (dbfFiles.Length == 0)
                    return new JsonResult(new { success = false, message = "Aucun fichier .dbf trouvé." });

                return new JsonResult(new { success = true, message = $"Succès! {dbfFiles.Length} fichiers détectés." });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public IActionResult OnGetGetSubDirectories(string parentPath)
        {
            try
            {
                // إذا كان المسار فارغاً نبدأ من الأقراص الصلبة
                if (string.IsNullOrEmpty(parentPath))
                {
                    var drives = DriveInfo.GetDrives().Select(d => d.Name).ToList();
                    return new JsonResult(drives);
                }

                var dirs = Directory.GetDirectories(parentPath)
                                    .Select(d => Path.GetFullPath(d))
                                    .ToList();
                return new JsonResult(dirs);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}