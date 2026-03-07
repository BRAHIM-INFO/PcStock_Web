using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace PcStock_Web.Pages.Achats
{
    public class BEMModel : PageModel
    {
        private readonly SqliteDbService _sqliteService;
        public BEMModel(SqliteDbService sqliteService) { _sqliteService = sqliteService; }

        public void OnGet() { }

        // جلب كل المشتريات دفعة واحدة مع ربط الجداول
        public IActionResult OnGetLoadAllBEM()
        {
            var list = new List<object>();
            string connString = _sqliteService.GetSqliteConnectionString();

            using (var conn = new SqliteConnection(connString))
            {
                conn.Open();
                // استعلام واحد يربط المشتريات بالسلع والموردين
                string sql = @"
                    SELECT A.NO_BR, A.REF, A.DATE, A.QTE, A.PAMP, A.NO_FACA, A.NO_BC,
                           S.INTITULE, S.FAMILLE, F.NOM AS NOM_FOURN
                    FROM ST_ACHAT A
                    LEFT JOIN ST_STOCK S ON A.REF = S.REF
                    LEFT JOIN ST_FOURN F ON A.COD_SOC = F.COD_SOC";

                var cmd = new SqliteCommand(sql, conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        double qte = reader["QTE"] != DBNull.Value ? Convert.ToDouble(reader["QTE"]) : 0;
                        double pamp = reader["PAMP"] != DBNull.Value ? Convert.ToDouble(reader["PAMP"]) : 0;

                        list.Add(new
                        {
                            nO_BR = reader["NO_BR"]?.ToString(),
                            REF = reader["REF"]?.ToString(),
                            intitule = reader["INTITULE"]?.ToString(),
                            famille = reader["FAMILLE"]?.ToString(),
                            date = reader["DATE"]?.ToString(),
                            noM_FOURN = reader["NOM_FOURN"]?.ToString(),
                            qte = qte,
                            pamp = pamp,
                            nO_FACA = reader["NO_FACA"]?.ToString(),
                            nO_BC = reader["NO_BC"]?.ToString()
                        });
                    }
                }
            }
            return new JsonResult(list);
        }

        // دالة الـ Autocomplete تبقى كما هي لجلب أرقام الـ BEM
        public IActionResult OnGetBEMAutocomplete(string term)
        {
            var results = new List<object>();
            using (var conn = new SqliteConnection(_sqliteService.GetSqliteConnectionString()))
            {
                conn.Open();
                var cmd = new SqliteCommand("SELECT DISTINCT NO_BR FROM ST_ACHAT WHERE NO_BR LIKE @t LIMIT 15", conn);
                cmd.Parameters.AddWithValue("@t", term + "%");
                using (var r = cmd.ExecuteReader()) { while (r.Read()) results.Add(new { id = r["NO_BR"], text = r["NO_BR"] }); }
            }
            return new JsonResult(results);
        }
    } 
public class BEMData
    {
        public string REF { get; set; }
        public string INTITULE { get; set; }
        public string FAMILLE { get; set; }
        public DateTime? DATE { get; set; }
        public string NOM_FOURN { get; set; }
        public double QTE { get; set; }
        public double PAMP { get; set; }
        public string NO_BR { get; set; }
        public string NO_FACA { get; set; }
        public string NO_BC { get; set; }
    }
}