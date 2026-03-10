using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace PcStock_Web.Pages.Achats
{
    public class RECAP_DES_ACHATSModel : PageModel
    {
        private readonly SqliteDbService _sqliteService;
        public RECAP_DES_ACHATSModel(SqliteDbService sqliteService) { _sqliteService = sqliteService; }

        public void OnGet() { }

        public IActionResult OnGetLoadData()
        {
            var list = new List<object>();
            string connString = _sqliteService.GetSqliteConnectionString();

            using (var conn = new SqliteConnection(connString))
            {
                conn.Open();
                // استعلام التجميع الشهري حسب العائلة
                // نستخدم date(..., 'start of month', '+1 month', '-1 day') للحصول على آخر يوم في الشهر
                string sql = @"
                    SELECT 
                        S.FAMILLE, 
                        date(A.DATE, 'start of month', '+1 month', '-1 day') AS DATE_FIN_MOIS,
                        SUM(A.QTE * A.PAMP) AS TOTAL_HT
                    FROM ST_ACHAT A
                    LEFT JOIN ST_STOCK S ON A.REF = S.REF
                    GROUP BY S.FAMILLE, strftime('%Y-%m', A.DATE)
                    ORDER BY DATE_FIN_MOIS DESC, S.FAMILLE ASC";

                var cmd = new SqliteCommand(sql, conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new
                        {
                            famille = reader["FAMILLE"]?.ToString() ?? "---",
                            date = reader["DATE_FIN_MOIS"]?.ToString(),
                            montant = reader["TOTAL_HT"] != DBNull.Value ? Convert.ToDouble(reader["TOTAL_HT"]) : 0
                        });
                    }
                }
            }
            return new JsonResult(list);
        }
    }
}