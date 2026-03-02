using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PcStock_Web.Pages.Stocks
{
    public class ArrivageModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public int TotalPages { get; set; }

        public int TotalItems { get; set; } // لإظهار إجمالي عدد الأسطر

        private readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ArrivageData.xlsx");

        [BindProperty]
        public ArrivageEntry NewEntry { get; set; } = new ArrivageEntry();

        // القائمة التي ستعرض في الجدول أسفل الصفحة
        public List<ArrivageEntry> SavedEntries { get; set; } = new List<ArrivageEntry>();

        public void OnGet()
        {
            var allData = LoadDataFromExcel(); // جلب كل البيانات

            TotalItems = allData.Count;
            TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);

            // تقسيم البيانات للعرض (Pagination)
            SavedEntries = allData
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            //TotalPages = (int)Math.Ceiling(allData.Count / (double)PageSize);

            //// تقسيم البيانات لعرض الصفحة المطلوبة فقط
            //SavedEntries = allData
            //    .Skip((CurrentPage - 1) * PageSize)
            //    .Take(PageSize)
            //    .ToList();
        }

        public List<ArrivageEntry> LoadDataFromExcel()
        {
            var list = new List<ArrivageEntry>();
            if (!System.IO.File.Exists(_filePath)) return list;

            //if (!System.IO.File.Exists(_filePath)) return;

            using (var workbook = new XLWorkbook(_filePath))
            {
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // تخطي سطر العناوين

                foreach (var row in rows)
                {
                    list.Add(new ArrivageEntry
                    {
                        Ord = row.Cell(1).GetValue<int>(),
                        Dates = row.Cell(2).Value.ToString(),

                        //    SavedEntries.Add(new ArrivageEntry
                        //{
                        //    Ord = row.Cell(1).GetValue<int>(),
                        //    // الحل هنا: نقرأ القيمة كـ string لنتجنب خطأ الـ Cast
                        //    Dates = row.Cell(2).Value.ToString(), 
                        REF = row.Cell(3).GetValue<string>(),
                        DESIGNATION = row.Cell(4).GetValue<string>(),
                        MACHINE = row.Cell(5).GetValue<string>(),
                        QTE = row.Cell(6).GetValue<int>(),
                        PRIX = row.Cell(7).GetValue<decimal>(),
                        FOURNISSEUR = row.Cell(8).GetValue<string>(),
                        FACT_N = row.Cell(9).GetValue<string>(),
                        BC_N = row.Cell(10).GetValue<string>(),
                        CASIER = row.Cell(11).GetValue<string>(),
                        ACHETTEUR = row.Cell(12).GetValue<string>()
                    });
                }
                // ترتيب البيانات لتظهر الأحدث في الأعلى (خيار إضافي لجعلها خفيفة)
                SavedEntries.Reverse();
            }

            return list.OrderByDescending(x => x.Ord).ToList(); // الأحدث دائماً في الأعلى

        }

        public IActionResult OnPost()
        {
            // ... (كود الحفظ السابق الذي كتبناه) ...
            // بعد الحفظ بنجاح:
            return RedirectToPage(); // سيعيد تحميل الصفحة وتظهر البيانات الجديدة فوراً
        }
    }

    public class ArrivageEntry
    {
        public int Ord { get; set; }
        public string Dates { get; set; }  // تاريخ الإدخال الافتراضي هو الوقت الحالي 
        public string REF { get; set; }
        public string DESIGNATION { get; set; }
        public string MACHINE { get; set; }
        public int QTE { get; set; }
        public decimal PRIX { get; set; }
        public string FOURNISSEUR { get; set; }
        public string FACT_N { get; set; }
        public string BC_N { get; set; }
        public string CASIER { get; set; }
        public string ACHETTEUR { get; set; }
    }
}
