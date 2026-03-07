using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace PcStock_Web.Pages.Cessions.Fournie
{
    public class BSMModel : PageModel
    {
        private readonly SqliteDbService _sqliteService;
        public BSMModel(SqliteDbService sqliteService) { _sqliteService = sqliteService; }

        public void OnGet() { }

        // جلب كل المشتريات دفعة واحدة مع ربط الجداول
        public IActionResult OnGetLoadAllBSM()
        {
            var list = new List<object>();
            string connString = _sqliteService.GetSqliteConnectionString();

            using (var conn = new SqliteConnection(connString))
            {
                conn.Open();
                // استعلام واحد يربط المشتريات بالسلع والموردين
                string sql = @"
                    SELECT A.NO_BC, A.REF, A.DATE, A.QTE, A.PAMP, A.COD_SOC,
                           S.INTITULE, S.FAMILLE 
                    FROM ST_CESS A
                    LEFT JOIN ST_STOCK S ON A.REF = S.REF";

                var cmd = new SqliteCommand(sql, conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        double qte = reader["QTE"] != DBNull.Value ? Convert.ToDouble(reader["QTE"]) : 0;
                        double pamp = reader["PAMP"] != DBNull.Value ? Convert.ToDouble(reader["PAMP"]) : 0;

                        list.Add(new
                        {
                            NO_BC = reader["NO_BC"]?.ToString(),
                            REF = reader["REF"]?.ToString(),
                            intitule = reader["INTITULE"]?.ToString(),
                            famille = reader["FAMILLE"]?.ToString(),
                            date = reader["DATE"]?.ToString(),
                            noM_CHANT = reader["COD_SOC"]?.ToString(),
                            qte = qte,
                            pamp = pamp
                        });
                    }
                }
            }
            return new JsonResult(list);
        }

        // دالة الـ Autocomplete تبقى كما هي لجلب أرقام الـ BSM
        public IActionResult OnGetBSMAutocomplete(string term)
        {
            var results = new List<object>();
            using (var conn = new SqliteConnection(_sqliteService.GetSqliteConnectionString()))
            {
                conn.Open();
                var cmd = new SqliteCommand("SELECT DISTINCT NO_BC FROM ST_CESS WHERE NO_BC LIKE @t LIMIT 15", conn);
                cmd.Parameters.AddWithValue("@t", term + "%");
                using (var r = cmd.ExecuteReader()) { while (r.Read()) results.Add(new { id = r["NO_BC"], text = r["NO_BC"] }); }
            }
            return new JsonResult(results);
        }
    }
    public class BSMData
    {
        public string REF { get; set; }
        public string INTITULE { get; set; }
        public string FAMILLE { get; set; }
        public DateTime? DATE { get; set; }
        public string NOM_CHANT { get; set; }
        public double QTE { get; set; }
        public double PAMP { get; set; }
        public string NO_BC { get; set; } 
    }
}