using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace PcStock_Web.Pages.Achats
{
    public class RECAP_ACHATS_MENSUELModel : PageModel
    {
        private readonly SqliteDbService _sqliteService;
        public RECAP_ACHATS_MENSUELModel(SqliteDbService sqliteService) { _sqliteService = sqliteService; }

        public void OnGet() { }

        public IActionResult OnGetLoadData()
        {
            var list = new List<object>();
            string connString = _sqliteService.GetSqliteConnectionString();

            using (var conn = new SqliteConnection(connString))
            {
                conn.Open();
                // الاستعلام يجمع حسب العائلة والشهر (Month/Year)
                // ويحسب تاريخ آخر يوم في ذلك الشهر ليعرضه في عمود التاريخ
                string sql = @"
                      SELECT 
                          S.FAMILLE, 
                          date(A.DATE, 'start of month', '+1 month', '-1 day') AS DATE_FIN_MOIS,
                          SUM(A.QTE * A.PAMP) AS MONTANT_TOTAL
                      FROM ST_ACHAT A
                      LEFT JOIN ST_STOCK S ON A.REF = S.REF
                      GROUP BY S.FAMILLE, strftime('%Y-%m', A.DATE)
                      ORDER BY A.DATE DESC, S.FAMILLE ASC";
                //string sql = @"
                //    SELECT 
                //        S.FAMILLE, 
                //        date(A.DATE, 'start of month', '+1 month', '-1 day') AS DATE_FIN_MOIS,
                //        SUM(A.QTE * A.PAMP) AS MONTANT_TOTAL
                //    FROM ST_ACHAT A
                //    LEFT JOIN ST_STOCK S ON A.REF = S.REF
                //    GROUP BY S.FAMILLE, strftime('%Y-%m', A.DATE)
                //    ORDER BY A.DATE DESC, S.FAMILLE ASC";

                var cmd = new SqliteCommand(sql, conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new
                        {
                            famille = reader["FAMILLE"]?.ToString() ?? "---",
                            date_achat = reader["DATE_FIN_MOIS"]?.ToString(), // تأكد أنها ترسل YYYY-MM-DD
                            montant = Convert.ToDouble(reader["MONTANT_TOTAL"])
                        });
                        //list.Add(new
                        //{
                        //    famille = reader["FAMILLE"]?.ToString() ?? "SANS FAMILLE",
                        //    date_achat = reader["DATE_FIN_MOIS"]?.ToString(), // التاريخ المجمع (آخر الشهر)
                        //    montant = reader["MONTANT_TOTAL"] != DBNull.Value ? Convert.ToDouble(reader["MONTANT_TOTAL"]) : 0
                        //});
                    }
                }
            }
            return new JsonResult(list);
        }
    }
}