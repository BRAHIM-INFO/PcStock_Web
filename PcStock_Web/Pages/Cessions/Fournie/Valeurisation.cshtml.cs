using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Linq;

namespace PcStock_Web.Pages.Cessions.Fournie
{
    public class ValeurisationModel : PageModel
    {
        private readonly SqliteDbService _sqliteService;

        public ValeurisationModel(SqliteDbService sqliteService)
        {
            _sqliteService = sqliteService;
        }

        public void OnGet() { }
         
        // 1. محرك البحث عن الورشات (ST_UNITE)
        public IActionResult OnGetChantierAutocomplete(string term)
        {
            var results = new List<object>();
            using (var conn = new SqliteConnection(_sqliteService.GetSqliteConnectionString()))
            {
                conn.Open();
                // البحث في الكود أو الاسم
                string sql = "SELECT COD_SOC, NOM FROM ST_UNITE WHERE NOM LIKE @t OR COD_SOC LIKE @t LIMIT 20";
                var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@t", "%" + term + "%");

                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        results.Add(new
                        {
                            id = r["COD_SOC"].ToString().Trim(),
                            text = r["COD_SOC"].ToString().Trim() + " - " + r["NOM"].ToString().Trim()
                        });
                    }
                }
            }
            return new JsonResult(results);
        }

        // 2. محرك البحث عن المنتجات (ST_STOCK) مع جلب كل التفاصيل
        public IActionResult OnGetArticleAutocomplete(string term)
        {
            var results = new List<object>();
            using (var conn = new SqliteConnection(_sqliteService.GetSqliteConnectionString()))
            {
                conn.Open();
                // أضفنا حقل QTE هنا
                string sql = @"SELECT REF, INTITULE, INTITULE2, FAMILLE, PAMP, QTE 
                       FROM ST_STOCK 
                       WHERE REF LIKE @t OR INTITULE LIKE @t 
                       LIMIT 20";
                var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@t", "%" + term + "%");

                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        results.Add(new
                        {
                            id = r["REF"].ToString().Trim(),
                            text = r["REF"].ToString().Trim() + " | " + r["INTITULE"].ToString().Trim(),
                            intitule = r["INTITULE"].ToString().Trim(),
                            pamp = r["PAMP"] != DBNull.Value ? Convert.ToDouble(r["PAMP"]) : 0,
                            qteStock = r["QTE"] != DBNull.Value ? Convert.ToDouble(r["QTE"]) : 0 // الحقل الجديد
                        });
                    }
                }
            }
            return new JsonResult(results);
        } 

        // 3. دالة الحفظ النهائي (يمكنك إكمالها لرفع البيانات لملف ST_CESS لاحقاً)
        public IActionResult OnPostSaveBSM([FromBody] BsmFinalData data)
        {
            try
            {
                // هنا نضع منطق الحفظ في ملف DBF (ST_CESS)
                // وأيضاً تحديث كميات الـ SQLITE لضمان دقة المخزون في الويب
                return new JsonResult(new { success = true, message = "Bon enregistré avec succès !" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }
    }

    // كلاس لنقل البيانات النهائية عند الحفظ
    public class BsmFinalData
    {
        public string BsmNo { get; set; }
        public string Date { get; set; }
        public string CodSoc { get; set; }
        public List<BsmLine> Lines { get; set; }
    }

    public class BsmLine
    {
        public string Ref { get; set; }
        public double Qte { get; set; }
        public double Pamp { get; set; }
    }
}