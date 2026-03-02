using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NDbfReader; // تأكد من تثبيت المكتبة عبر NuGet
using System.Text;

namespace PcStock_Web.Pages.Stocks
{
    public class ListModel : PageModel
    {
        private readonly string _dbfPath = @"C:\PCSTOCK\2026\ST_STOCK.DBF";

        // متغيرات الترقيم (Pagination)
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }

        public List<StockItem> Inventory { get; set; } = new List<StockItem>();

        public void OnGet()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            if (!System.IO.File.Exists(_dbfPath)) return;

            var allItems = new List<StockItem>();

            using (var table = Table.Open(_dbfPath))
            {
                var reader = table.OpenReader(Encoding.GetEncoding(1252));
                while (reader.Read())
                {
                    // استخدام الـ Index لتجاوز مشاكل أسماء الأعمدة
                    allItems.Add(new StockItem
                    {
                        REF = reader.GetValue("REF\0\b\0\0\0\u0012mI")?.ToString()?.Trim(),       // العمود [0] هو REF
                        CODE_INT = reader.GetValue("CODE_INT\0mI")?.ToString()?.Trim(),   // العمود [1] هو CODE_INT
                        INTITULE = reader.GetValue("INTITULE\0mI")?.ToString()?.Trim(),  // العمود [4] هو INTITULE
                        INTITULE2 = reader.GetValue("INTITULE2\0I")?.ToString()?.Trim(), // العمود [5] هو INTITULE2
                        INTITULE3 = reader.GetValue("INTITULE3\0I")?.ToString()?.Trim(), // العمود [6] هو INTITULE3
                        FAMILLE = reader.GetValue("FAMILLE\0E\0I")?.ToString()?.Trim(),   // العمود [9] هو FAMILLE
                        
                        // الأرقام مع تحويل آمن
                        QTE = Convert.ToDecimal(reader.GetValue("QTE\0LLE\0E\0I") ?? 0),  // العمود [10] هو QTE
                        PAMP = Convert.ToDecimal(reader.GetValue("PAMP\0VEN\0\0I") ?? 0), // العمود [14] هو PAMP
                        
                        // بالنسبة لـ DATE_MAJ، ابحث عن ترتيبها في القائمة المنسدلة لديك
                        DATE_MAJ = reader.GetValue("DATE_MAJ\0I") as DateTime?
                    });
                }
            }

            // حسابات الترقيم
            TotalItems = allItems.Count;
            TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);

            Inventory = allItems
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        //public void OnGet()
        //{
        //    // تسجيل ترميز النصوص لدعم اللغة العربية أو الفرنسية في ملفات DBF
        //    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        //    if (!System.IO.File.Exists(_dbfPath)) return;

        //    using (var table = Table.Open(_dbfPath))
        //    {
        //        var reader = table.OpenReader(Encoding.GetEncoding(1252)); // ترميز Windows-1252
        //        while (reader.Read())
        //        {
        //            Inventory.Add(new StockItem
        //            {
        //                // نستخدم رقم العمود (Index) بناءً على ترتيبه في صورتك
        //                REF = reader.GetValue("REF\0\b\0\0\0\u0012mI")?.ToString()?.Trim(),       // العمود [0] هو REF
        //                CODE_INT = reader.GetValue("CODE_INT\0mI")?.ToString()?.Trim(),   // العمود [1] هو CODE_INT
        //                INTITULE = reader.GetValue("INTITULE\0mI")?.ToString()?.Trim(),  // العمود [4] هو INTITULE
        //                INTITULE2 = reader.GetValue("INTITULE2\0I")?.ToString()?.Trim(), // العمود [5] هو INTITULE2
        //                INTITULE3 = reader.GetValue("INTITULE3\0I")?.ToString()?.Trim(), // العمود [6] هو INTITULE3
        //                FAMILLE = reader.GetValue("FAMILLE\0E\0I")?.ToString()?.Trim(),   // العمود [9] هو FAMILLE

        //                // الأرقام مع تحويل آمن
        //                QTE = Convert.ToDecimal(reader.GetValue("QTE\0LLE\0E\0I") ?? 0),  // العمود [10] هو QTE
        //                PAMP = Convert.ToDecimal(reader.GetValue("PAMP\0VEN\0\0I") ?? 0), // العمود [14] هو PAMP

        //                // بالنسبة لـ DATE_MAJ، ابحث عن ترتيبها في القائمة المنسدلة لديك
        //                DATE_MAJ = reader.GetValue("DATE_MAJ\0I") as DateTime?
        //            });
        //        }
        //    }
        //}
    }

    public class StockItem
    {
        public string REF { get; set; }
        public string CODE_INT { get; set; }
        public string INTITULE { get; set; }
        public string INTITULE2 { get; set; }
        public string INTITULE3 { get; set; }
        public string FAMILLE { get; set; }
        public decimal QTE { get; set; }
        public decimal PAMP { get; set; }
        public DateTime? DATE_MAJ { get; set; }
    }
}