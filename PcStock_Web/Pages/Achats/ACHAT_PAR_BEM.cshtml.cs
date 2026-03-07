using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace PcStock_Web.Pages.Achats
{
    public class ACHAT_PAR_BEMModel : PageModel
    {
        private readonly SqliteDbService _sqliteService;
        public ACHAT_PAR_BEMModel(SqliteDbService sqliteService) { _sqliteService = sqliteService; }

        public void OnGet() { }

        public IActionResult OnGetLoadData()
        {
            var list = new List<object>();
            string connString = _sqliteService.GetSqliteConnectionString();

            using (var conn = new SqliteConnection(connString))
            {
                conn.Open();
                // استعلام التجميع حسب رقم السند وعائلة المنتج
                string sql = @"
                    SELECT 
                        S.FAMILLE, 
                        A.NO_BR, 
                        A.DATE, 
                        A.NO_FACA, 
                        F.NOM AS FOURNISSEUR, 
                        SUM(A.QTE * A.PAMP) AS MONTANT
                    FROM ST_ACHAT A
                    LEFT JOIN ST_STOCK S ON A.REF = S.REF
                    LEFT JOIN ST_FOURN F ON A.COD_SOC = F.COD_SOC
                    GROUP BY A.NO_BR, S.FAMILLE, A.DATE, A.NO_FACA, F.NOM
                    ORDER BY A.DATE DESC, A.NO_BR DESC";

                var cmd = new SqliteCommand(sql, conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new
                        {
                            famille = reader["FAMILLE"]?.ToString() ?? "---",
                            no_br = reader["NO_BR"]?.ToString(),
                            date_achat = reader["DATE"]?.ToString(),
                            no_faca = reader["NO_FACA"]?.ToString(),
                            fournisseur = reader["FOURNISSEUR"]?.ToString() ?? "INCONNU",
                            montant = reader["MONTANT"] != DBNull.Value ? Convert.ToDouble(reader["MONTANT"]) : 0
                        });
                    }
                }
            }
            return new JsonResult(list);
        }
    }
}