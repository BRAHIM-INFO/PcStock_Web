using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace PcStock_Web.Pages.Achats
{
    public class ResteModel : PageModel
    {
        private readonly SqliteDbService _sqliteService;
        public ResteModel(SqliteDbService sqliteService) { _sqliteService = sqliteService; }

        public void OnGet() { }

        public IActionResult OnGetLoadData()
        {
            var list = new List<object>();
            string connString = _sqliteService.GetSqliteConnectionString();

            using (var conn = new SqliteConnection(connString))
            {
                conn.Open();
                string sql = @"
            SELECT 
                S.REF, S.INTITULE, S.INTITULE2, S.FAMILLE, S.QTE AS QTE_ACTUELLE, S.PAMP, S.CASIER, -- أضفنا CASIER هنا
                A.TOTAL_ACHAT_QTE,
                A.MAX_DATE AS DATE_DERNIER_ACHAT,
                A.NO_BR,
                A.COD_SOC
            FROM ST_STOCK S
            INNER JOIN (
                SELECT REF, SUM(QTE) AS TOTAL_ACHAT_QTE, MAX(DATE) AS MAX_DATE, NO_BR, COD_SOC
                FROM ST_ACHAT
                GROUP BY REF
            ) A ON S.REF = A.REF
            WHERE S.QTE > 0 
            ORDER BY A.MAX_DATE DESC";

                var cmd = new SqliteCommand(sql, conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        double qteStock = reader["QTE_ACTUELLE"] != DBNull.Value ? Convert.ToDouble(reader["QTE_ACTUELLE"]) : 0;
                        double qteAchat = reader["TOTAL_ACHAT_QTE"] != DBNull.Value ? Convert.ToDouble(reader["TOTAL_ACHAT_QTE"]) : 0;
                        double pamp = reader["PAMP"] != DBNull.Value ? Convert.ToDouble(reader["PAMP"]) : 0;

                        list.Add(new
                        {
                            ref_art = reader["REF"]?.ToString().Trim(),
                            intitule = reader["INTITULE"]?.ToString().Trim(),
                            intitule2 = reader["INTITULE2"]?.ToString().Trim(),
                            famille = reader["FAMILLE"]?.ToString().Trim(),
                            qte_achat = qteAchat,
                            qte_stock = qteStock,
                            pamp = pamp,
                            montant = qteStock * pamp,
                            no_br = reader["NO_BR"]?.ToString().Trim(),
                            date_achat = reader["DATE_DERNIER_ACHAT"]?.ToString(),
                            cod_soc = reader["COD_SOC"]?.ToString().Trim(),
                            casier = reader["CASIER"]?.ToString().Trim() // الحقل المضاف في آخر السطر
                        });
                    }
                }
            }
            return new JsonResult(list);
        }

        //public IActionResult OnGetLoadData()
        //{
        //    var list = new List<object>();
        //    string connString = _sqliteService.GetSqliteConnectionString();

        //    using (var conn = new SqliteConnection(connString))
        //    {
        //        conn.Open();
        //        // استعلام متطور: 
        //        // 1. نأخذ السلع من ST_STOCK التي كميتها أكبر من 0 (أو 1 حسب طلبك)
        //        // 2. نربطها بآخر حركة شراء لكل REF من جدول ST_ACHAT
        //        string sql = @"
        //            SELECT 
        //                S.REF, S.INTITULE, S.INTITULE2, S.FAMILLE, S.QTE, S.PAMP,
        //                A.MAX_DATE AS DATE_DERNIER_ACHAT,
        //                A.NO_BR,
        //                A.COD_SOC
        //            FROM ST_STOCK S
        //            INNER JOIN (
        //                -- استعلام فرعي لجلب أحدث تاريخ شراء لكل REF بدون تكرار
        //                SELECT REF, MAX(DATE) AS MAX_DATE, NO_BR, COD_SOC
        //                FROM ST_ACHAT
        //                GROUP BY REF
        //            ) A ON S.REF = A.REF
        //            WHERE S.QTE > 0 -- يمكن تغييرها لـ 1 حسب الحاجة الدقيقة
        //            ORDER BY A.MAX_DATE DESC";

        //        var cmd = new SqliteCommand(sql, conn);
        //        using (var reader = cmd.ExecuteReader())
        //        {
        //            while (reader.Read())
        //            {
        //                double qteStock = reader["QTE"] != DBNull.Value ? Convert.ToDouble(reader["QTE"]) : 0;
        //                double pamp = reader["PAMP"] != DBNull.Value ? Convert.ToDouble(reader["PAMP"]) : 0;

        //                list.Add(new
        //                {
        //                    ref_art = reader["REF"]?.ToString().Trim(),
        //                    intitule = reader["INTITULE"]?.ToString().Trim(),
        //                    intitule2 = reader["INTITULE2"]?.ToString().Trim(),
        //                    famille = reader["FAMILLE"]?.ToString().Trim(),
        //                    qte = qteStock,
        //                    pamp = pamp,
        //                    montant = qteStock * pamp,
        //                    no_br = reader["NO_BR"]?.ToString().Trim(),
        //                    date_achat = reader["DATE_DERNIER_ACHAT"]?.ToString(),
        //                    cod_soc = reader["COD_SOC"]?.ToString().Trim()
        //                });
        //            }
        //        }
        //    }
        //    return new JsonResult(list);
        //}
    }
}