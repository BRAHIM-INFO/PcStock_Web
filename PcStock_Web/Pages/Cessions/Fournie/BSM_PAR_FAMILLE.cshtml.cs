using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace PcStock_Web.Pages.Cessions.Fournie
{
    public class BSM_PAR_FAMILLEModel : PageModel
    {
        private readonly SqliteDbService _sqliteService;
        public BSM_PAR_FAMILLEModel(SqliteDbService sqliteService) { _sqliteService = sqliteService; }

        public void OnGet() { }

        public IActionResult OnGetLoadData()
        {
            var list = new List<object>();
            string connString = _sqliteService.GetSqliteConnectionString();

            using (var conn = new SqliteConnection(connString))
            {
                conn.Open();
                // استعلام لتجميع مبالغ التنازلات حسب العائلة ورقم السند
                string sql = @"
                    SELECT 
                        S.FAMILLE, 
                        C.DATE, 
                        C.NO_BC, 
                        C.COD_SOC, 
                        SUM(C.QTE * C.PAMP) AS MONTANT_TOTAL
                    FROM ST_CESS C
                    LEFT JOIN ST_STOCK S ON C.REF = S.REF
                    GROUP BY C.NO_BC, S.FAMILLE, C.DATE, C.COD_SOC
                    ORDER BY C.DATE DESC, C.NO_BC DESC";

                var cmd = new SqliteCommand(sql, conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new
                        {
                            famille = reader["FAMILLE"]?.ToString() ?? "SANS FAMILLE",
                            no_bc = reader["NO_BC"]?.ToString(),
                            date_cess = reader["DATE"]?.ToString(),
                            cod_soc = reader["COD_SOC"]?.ToString(), // كود الموقع/الورشة
                            montant = reader["MONTANT_TOTAL"] != DBNull.Value ? Convert.ToDouble(reader["MONTANT_TOTAL"]) : 0
                        });
                    }
                }
            }
            return new JsonResult(list);
        }
    }
}