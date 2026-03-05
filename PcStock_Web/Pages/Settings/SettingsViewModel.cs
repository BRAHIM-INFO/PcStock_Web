namespace PcStock_Web.Pages.Settings
{
    public class SettingsViewModel
    {
        public string DbPath { get; set; }
        public IFormFile? LogoFile { get; set; } // لتحميل الصورة
        public string? CurrentLogoPath { get; set; } // لعرض الصورة الحالية
    }
}
