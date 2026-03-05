using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.Odbc;
using System.Collections.Generic;
using System;

namespace PcStock_Web.Pages.Stocks
{  
    public class ArticlesModel : PageModel
    {
        public int TotalCount { get; set; }

        private readonly ConfigService _configService;
        public ArticlesModel(ConfigService configService) { _configService = configService; }


        public void OnGet()
        {
            
        }
         

        // دالة جلب كل البيانات دفعة واحدة كـ JSON
        public IActionResult OnGetAllData()
        {
            var list = new List<ArticleData>();
            //string dbPath = @"C:\PCSTOCK\2026";
            // جلب المسار "الديناميكي" الذي حفظه المستخدم في الإعدادات
            string dbPath = _configService.GetDbPath();

            string connString = $@"Driver={{Microsoft Access dBASE Driver (*.dbf, *.ndx, *.mdx)}};Dbq={dbPath};";

            using (OdbcConnection conn = new OdbcConnection(connString))
            {
                try
                {
                    conn.Open();
                    // جلب كل البيانات بطلب واحد
                    string query = "SELECT REF, INTITULE, FAMILLE, QTE, PAMP, STOCK_INI, CASIER, DATE_MAJ FROM ST_STOCK.DBF";
                    OdbcCommand cmd = new OdbcCommand(query, conn);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new ArticleData
                            {
                                REF = reader["REF"]?.ToString() ?? "",
                                INTITULE = reader["INTITULE"]?.ToString() ?? "",
                                FAMILLE = reader["FAMILLE"]?.ToString() ?? "",
                                QTE = reader["QTE"] != DBNull.Value ? Convert.ToDouble(reader["QTE"]) : 0,
                                PAMP = reader["PAMP"] != DBNull.Value ? Convert.ToDouble(reader["PAMP"]) : 0,
                                STOCK_INI = reader["STOCK_INI"] != DBNull.Value ? Convert.ToDouble(reader["STOCK_INI"]) : 0,
                                CASIER = reader["CASIER"]?.ToString() ?? "",
                                DATE_MAJ = reader["DATE_MAJ"] != DBNull.Value ? Convert.ToDateTime(reader["DATE_MAJ"]) : (DateTime?)null
                            });
                        }
                    }
                }
                catch { }
            }
            return new JsonResult(list);
        }
    }

    // غيرت الاسم لـ ArticleData لتجنب التضارب مع أي كلاس آخر بنفس الاسم
    public class ArticleData
    {
        public string REF { get; set; }
        public string INTITULE { get; set; }
        public string INTITULE2 { get; set; }
        public string INTITULE3 { get; set; }
        public string FAMILLE { get; set; }
        public double QTE { get; set; }
        public double PAMP { get; set; }
        public double STOCK_INI { get; set; }
        public string CASIER { get; set; }
        public DateTime? DATE_MAJ { get; set; }
        public double MONTANT => QTE * PAMP;
    }
}