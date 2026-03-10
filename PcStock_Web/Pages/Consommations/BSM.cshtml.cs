using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace PcStock_Web.Pages.Consommations
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
                    SELECT A.NO_BS, A.REF, A.DATE, A.QTE, A.PAMP, A.COD_SOC,
                           S.INTITULE, S.FAMILLE 
                    FROM ST_CONSO A
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
                            NO_BS = reader["NO_BS"]?.ToString(),
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
                var cmd = new SqliteCommand("SELECT DISTINCT NO_BS FROM ST_CONSO WHERE NO_BS LIKE @t LIMIT 15", conn);
                cmd.Parameters.AddWithValue("@t", term + "%");
                using (var r = cmd.ExecuteReader()) { while (r.Read()) results.Add(new { id = r["NO_BS"], text = r["NO_BS"] }); }
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
        public string NO_BS { get; set; }
    }
}
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using Microsoft.Data.Sqlite;

//namespace PcStock_Web.Pages.Consommations
//{
//    public class BSMModel : PageModel
//    {
//        private readonly SqliteDbService _sqliteService;
//        public BSMModel(SqliteDbService sqliteService) { _sqliteService = sqliteService; }

//        public void OnGet() { }

//        // جلب كل المشتريات دفعة واحدة مع ربط الجداول
//        public IActionResult OnGetLoadAllBSM()
//        {
//            var list = new List<object>();
//            string connString = _sqliteService.GetSqliteConnectionString();

//            using (var conn = new SqliteConnection(connString))
//            {
//                conn.Open();
//                // تأكد من جلب COD_SOC لربطه بالورشة
//                string sql = @"
//            SELECT A.NO_BS, A.REF, A.DATE, A.QTE, A.PAMP, A.COD_SOC,
//                   S.INTITULE, S.FAMILLE 
//            FROM ST_CONSO A
//            LEFT JOIN ST_STOCK S ON A.REF = S.REF";

//                var cmd = new SqliteCommand(sql, conn);
//                using (var reader = cmd.ExecuteReader())
//                {
//                    while (reader.Read())
//                    {
//                        double qte = reader["QTE"] != DBNull.Value ? Convert.ToDouble(reader["QTE"]) : 0;
//                        double pamp = reader["PAMP"] != DBNull.Value ? Convert.ToDouble(reader["PAMP"]) : 0;

//                        list.Add(new
//                        {
//                            // استخدم حروف صغيرة تماماً هنا لتجنب المشاكل في JS
//                            no_bs = reader["NO_BS"]?.ToString().Trim(),
//                            @ref = reader["REF"]?.ToString().Trim(),
//                            intitule = reader["INTITULE"]?.ToString().Trim(),
//                            famille = reader["FAMILLE"]?.ToString().Trim(),
//                            date = reader["DATE"]?.ToString(),
//                            nom_chant = reader["COD_SOC"]?.ToString().Trim(),
//                            qte = qte,
//                            pamp = pamp
//                        });
//                    }
//                }
//            }
//            return new JsonResult(list);
//        }
//        //public IActionResult OnGetLoadAllBSM()
//        //{
//        //    var list = new List<object>();
//        //    string connString = _sqliteService.GetSqliteConnectionString();

//        //    using (var conn = new SqliteConnection(connString))
//        //    {
//        //        conn.Open();
//        //        // استعلام واحد يربط المشتريات بالسلع والموردين
//        //        string sql = @"
//        //            SELECT A.NO_BS, A.REF, A.DATE, A.QTE, A.PAMP, A.COD_SOC,
//        //                   S.INTITULE, S.FAMILLE 
//        //            FROM ST_CONSO A
//        //            LEFT JOIN ST_STOCK S ON A.REF = S.REF";

//        //        var cmd = new SqliteCommand(sql, conn);
//        //        using (var reader = cmd.ExecuteReader())
//        //        {
//        //            while (reader.Read())
//        //            {
//        //                double qte = reader["QTE"] != DBNull.Value ? Convert.ToDouble(reader["QTE"]) : 0;
//        //                double pamp = reader["PAMP"] != DBNull.Value ? Convert.ToDouble(reader["PAMP"]) : 0;

//        //                list.Add(new
//        //                {
//        //                    NO_BS = reader["NO_BS"]?.ToString(),
//        //                    REF = reader["REF"]?.ToString(),
//        //                    intitule = reader["INTITULE"]?.ToString(),
//        //                    famille = reader["FAMILLE"]?.ToString(),
//        //                    date = reader["DATE"]?.ToString(),
//        //                    noM_CHANT = reader["COD_SOC"]?.ToString(),
//        //                    qte = qte,
//        //                    pamp = pamp
//        //                });
//        //            }
//        //        }
//        //    }
//        //    return new JsonResult(list);
//        //}

//        // دالة الـ Autocomplete تبقى كما هي لجلب أرقام الـ BSM
//        public IActionResult OnGetBSMAutocomplete(string term)
//        {
//            var results = new List<object>();
//            using (var conn = new SqliteConnection(_sqliteService.GetSqliteConnectionString()))
//            {
//                conn.Open();
//                var cmd = new SqliteCommand("SELECT DISTINCT NO_BS FROM ST_CONSO WHERE NO_BS LIKE @t LIMIT 15", conn);
//                cmd.Parameters.AddWithValue("@t", term + "%");
//                using (var r = cmd.ExecuteReader()) { while (r.Read()) results.Add(new { id = r["NO_BS"], text = r["NO_BS"] }); }
//            }
//            return new JsonResult(results);
//        }
//    }
//    public class BSMData
//    {
//        public string REF { get; set; }
//        public string INTITULE { get; set; }
//        public string FAMILLE { get; set; }
//        public DateTime? DATE { get; set; }
//        public string NOM_CHANT { get; set; }
//        public double QTE { get; set; }
//        public double PAMP { get; set; }
//        public string NO_BS { get; set; }
//    }
//}