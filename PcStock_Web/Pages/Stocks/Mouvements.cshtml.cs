using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PcStock_Web.Pages.Stocks
{
    public class MouvementsModel : PageModel
    {
        private readonly SqliteDbService _sqliteService;

        public MouvementsModel(SqliteDbService sqliteService)
        {
            _sqliteService = sqliteService;
        }

        public void OnGet() { }

        // 1. محرك البحث التلقائي (Autocomplete)
        public IActionResult OnGetArticleAutocomplete(string term)
        {
            var results = new List<object>();
            using (var conn = new SqliteConnection(_sqliteService.GetSqliteConnectionString()))
            {
                conn.Open();
                string sql = "SELECT REF, INTITULE, FAMILLE, CASIER, STOCK_INI FROM ST_STOCK WHERE REF LIKE @t OR INTITULE LIKE @t LIMIT 15";
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
                            famille = r["FAMILLE"]?.ToString().Trim() ?? "---",
                            casier = r["CASIER"]?.ToString().Trim() ?? "---"
                        });
                    }
                }
            }
            return new JsonResult(new { results = results });
        }

        // 2. جلب حركات المنتج (Fiche Mouvement)
        //public async Task<IActionResult> OnGetLoadMouvements(string refArt)
        //{
        //    if (string.IsNullOrEmpty(refArt)) return new JsonResult(new List<object>());

        //    return await Task.Run(() =>
        //    {
        //        var list = new List<object>();
        //        string connString = _sqliteService.GetSqliteConnectionString();
        //        int orderCounter = 1;

        //        using (var conn = new SqliteConnection(connString))
        //        {
        //            conn.Open();

        //            // أ. جلب الـ Stock Initial من جدول ST_STOCK أولاً
        //            string sqlStock = "SELECT STOCK_INI, DATE_COM FROM ST_STOCK WHERE REF = @r";
        //            var cmdStock = new SqliteCommand(sqlStock, conn);
        //            cmdStock.Parameters.AddWithValue("@r", refArt);

        //            using (var reader = cmdStock.ExecuteReader())
        //            {
        //                if (reader.Read())
        //                {
        //                    double stockIni = reader["STOCK_INI"] != DBNull.Value ? Convert.ToDouble(reader["STOCK_INI"]) : 0;
        //                    if (stockIni > 0)
        //                    {
        //                        list.Add(new
        //                        {
        //                            ord = orderCounter++,
        //                            date = reader["DATE_COM"]?.ToString(), // تاريخ بدء المنتج/السنة
        //                            qte_entre = stockIni,
        //                            qte_sortie = 0.0,
        //                            en_stock = stockIni,
        //                            pamp = 0.0,
        //                            entity = "STOCK INITIAL",
        //                            type = "INITIAL"
        //                        });
        //                    }
        //                }
        //            }

        //            // ب. جلب القواميس لربط الأسماء (الموردين والوحدات)
        //            var entities = GetEntitiesDict(conn);

        //            // ج. استعلام UNION ALL لجمع كل الحركات وترتيبها (دخول ثم خروج في نفس اليوم)
        //            string sqlMouv = @"
        //                SELECT * FROM (
        //                    SELECT DATE, QTE, 0 AS Q_SOR, ENSTOCK, PAMP, COD_SOC, 1 AS Priority, 'ACHAT' AS MovType FROM ST_ACHAT WHERE REF = @r
        //                    UNION ALL
        //                    SELECT DATE, QTE, 0 AS Q_SOR, ENSTOCK, PAMP, COD_SOC, 1 AS Priority, 'REINTEG' AS MovType FROM ST_CESSR WHERE REF = @r
        //                    UNION ALL
        //                    SELECT DATE, 0 AS Q_ENT, QTE, ENSTOCK, PAMP, COD_SOC, 2 AS Priority, 'CESSION' AS MovType FROM ST_CESS WHERE REF = @r
        //                    UNION ALL
        //                    SELECT DATE, 0 AS Q_ENT, QTE, ENSTOCK, PAMP, COD_SOC, 2 AS Priority, 'CONSO' AS MovType FROM ST_CONSO WHERE REF = @r
        //                ) 
        //                ORDER BY DATE ASC, Priority ASC";

        //            var cmdMouv = new SqliteCommand(sqlMouv, conn);
        //            cmdMouv.Parameters.AddWithValue("@r", refArt);

        //            using (var reader = cmdMouv.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    double q_ent = reader["Priority"].ToString() == "1" ? Convert.ToDouble(reader["QTE"]) : 0;
        //                    double q_sor = reader["Priority"].ToString() == "2" ? Convert.ToDouble(reader["QTE"]) : 0;
        //                    string codSoc = reader["COD_SOC"]?.ToString().Trim();

        //                    list.Add(new
        //                    {
        //                        ord = orderCounter++,
        //                        date = reader["DATE"]?.ToString(),
        //                        qte_entre = q_ent,
        //                        qte_sortie = q_sor,
        //                        en_stock = Convert.ToDouble(reader["ENSTOCK"]), // الرصيد اللحظي من الملف
        //                        pamp = Convert.ToDouble(reader["PAMP"]),
        //                        entity = entities.ContainsKey(codSoc) ? entities[codSoc] : codSoc,
        //                        type = reader["MovType"].ToString()
        //                    });
        //                }
        //            }
        //        }
        //        return (IActionResult)new JsonResult(list);
        //    });
        //}
        public async Task<IActionResult> OnGetLoadMouvements(string refArt)
        {
            if (string.IsNullOrEmpty(refArt)) return new JsonResult(new List<object>());

            return await Task.Run(() =>
            {
                var list = new List<object>();
                string connString = _sqliteService.GetSqliteConnectionString();
                int orderCounter = 1;

                using (var conn = new SqliteConnection(connString))
                {
                    conn.Open();

                    // أ. جلب الرصيد الافتتاحي
                    string sqlStock = "SELECT STOCK_INI, DATE_COM FROM ST_STOCK WHERE REF = @r";
                    var cmdStock = new SqliteCommand(sqlStock, conn);
                    cmdStock.Parameters.AddWithValue("@r", refArt);
                    using (var reader = cmdStock.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            double sIni = reader["STOCK_INI"] != DBNull.Value ? Convert.ToDouble(reader["STOCK_INI"]) : 0;
                            if (sIni > 0)
                            {
                                list.Add(new { ord = orderCounter++, date = reader["DATE_COM"]?.ToString(), qte_entre = sIni, qte_sortie = 0.0, en_stock = sIni, pamp = 0.0, entity = "STOCK INITIAL", type = "INITIAL" });
                            }
                        }
                    }

                    var entities = GetEntitiesDict(conn);

                    // ب. تصحيح استعلام الـ UNION (توحيد أسماء الحقول VAL_E و VAL_S)
                    string sqlMouv = @"
                SELECT * FROM (
                    SELECT DATE, QTE AS VAL_E, 0 AS VAL_S, ENSTOCK, PAMP, COD_SOC, 1 AS Priority, 'ACHAT' AS MovType FROM ST_ACHAT WHERE REF = @r
                    UNION ALL
                    SELECT DATE, QTE AS VAL_E, 0 AS VAL_S, ENSTOCK, PAMP, COD_SOC, 1 AS Priority, 'REINTEG' AS MovType FROM ST_CESSR WHERE REF = @r
                    UNION ALL
                    SELECT DATE, 0 AS VAL_E, QTE AS VAL_S, ENSTOCK, PAMP, COD_SOC, 2 AS Priority, 'CESSION' AS MovType FROM ST_CESS WHERE REF = @r
                    UNION ALL
                    SELECT DATE, 0 AS VAL_E, QTE AS VAL_S, ENSTOCK, PAMP, COD_SOC, 2 AS Priority, 'CONSO' AS MovType FROM ST_CONSO WHERE REF = @r
                ) 
                ORDER BY DATE ASC, Priority ASC";

                    var cmdMouv = new SqliteCommand(sqlMouv, conn);
                    cmdMouv.Parameters.AddWithValue("@r", refArt);

                    using (var reader = cmdMouv.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new
                            {
                                ord = orderCounter++,
                                date = reader["DATE"]?.ToString(),
                                qte_entre = Convert.ToDouble(reader["VAL_E"]), // قراءة من الاسم الجديد
                                qte_sortie = Convert.ToDouble(reader["VAL_S"]), // قراءة من الاسم الجديد
                                en_stock = Convert.ToDouble(reader["ENSTOCK"]),
                                pamp = Convert.ToDouble(reader["PAMP"]),
                                entity = entities.ContainsKey(reader["COD_SOC"].ToString().Trim()) ? entities[reader["COD_SOC"].ToString().Trim()] : reader["COD_SOC"].ToString(),
                                type = reader["MovType"].ToString()
                            });
                        }
                    }
                }
                return (IActionResult)new JsonResult(list);
            });
        }
        private Dictionary<string, string> GetEntitiesDict(SqliteConnection conn)
        {
            var dict = new Dictionary<string, string>();
            // دمج أسماء الموردين (ST_FOURN) وأسماء الورشات (ST_UNITE) في قائمة واحدة
            string sql = "SELECT COD_SOC, NOM FROM ST_FOURN UNION SELECT COD_SOC, NOM FROM ST_UNITE";
            using (var cmd = new SqliteCommand(sql, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string key = reader["COD_SOC"].ToString().Trim();
                    if (!dict.ContainsKey(key))
                        dict.Add(key, reader["NOM"].ToString().Trim());
                }
            }
            return dict;
        }
    }
} 