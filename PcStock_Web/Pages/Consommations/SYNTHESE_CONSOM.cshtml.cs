using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using System.Dynamic;

namespace PcStock_Web.Pages.Consommations
{
    public class SYNTHESE_CONSOMModel : PageModel
    {
        private readonly SqliteDbService _sqliteService;
        public SYNTHESE_CONSOMModel(SqliteDbService sqliteService) { _sqliteService = sqliteService; }

        public void OnGet() { }

        public IActionResult OnGetLoadData(string d1, string d2)
        {
            string connString = _sqliteService.GetSqliteConnectionString();
            var rows = new Dictionary<string, ExpandoObject>();
            var familiesFound = new SortedSet<string>(); // لتخزين العائلات الفريدة مرتبة

            using (var conn = new SqliteConnection(connString))
            {
                conn.Open();
                string sql = @"
                    SELECT A.NO_BS, A.DATE, S.FAMILLE, (A.QTE * A.PAMP) AS MONTANT
                    FROM ST_CONSO A
                    LEFT JOIN ST_STOCK S ON A.REF = S.REF
                    WHERE A.DATE BETWEEN @d1 AND @d2
                    ORDER BY A.DATE ASC, A.NO_BS ASC";

                var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@d1", d1 ?? "2000-01-01");
                cmd.Parameters.AddWithValue("@d2", d2 ?? "2099-12-31");

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string noBs = reader["NO_BS"].ToString();
                        string date = reader["DATE"].ToString();
                        string famille = reader["FAMILLE"]?.ToString()?.Trim() ?? "SANS_FAM";
                        double montant = Convert.ToDouble(reader["MONTANT"]);

                        familiesFound.Add(famille);

                        if (!rows.ContainsKey(noBs))
                        {
                            dynamic newRow = new ExpandoObject();
                            newRow.no_bsm = noBs;
                            newRow.date = date;
                            newRow.amounts = new Dictionary<string, double>();
                            newRow.total = 0.0;
                            rows[noBs] = newRow;
                        }

                        var rowDict = (IDictionary<string, object>)rows[noBs];
                        var amounts = (Dictionary<string, double>)rowDict["amounts"];

                        if (!amounts.ContainsKey(famille)) amounts[famille] = 0;
                        amounts[famille] += montant;
                        rowDict["total"] = (double)rowDict["total"] + montant;
                    }
                }
            }

            return new JsonResult(new
            {
                families = familiesFound.ToList(),
                data = rows.Values.ToList()
            });
        }
    }
}