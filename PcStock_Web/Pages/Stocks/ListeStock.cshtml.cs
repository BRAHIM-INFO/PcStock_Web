//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using Microsoft.Data.Sqlite;
//using System.Data.Odbc;

//namespace PcStock_Web.Pages.Stocks
//{
//    public class ListeStockModel : PageModel
//    {
//        public void OnGet()
//        {
//        }
//    }
//}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using System.Data.Odbc;

namespace PcStock_Web.Pages.Stocks
{
    public class ListeStockModel : PageModel
    {
        private readonly SqliteDbService _sqliteService;
        private readonly ConfigService _configService;

        public ListeStockModel(SqliteDbService sqliteService, ConfigService configService)
        {
            _sqliteService = sqliteService;
            _configService = configService;
        }

        public void OnGet() { }

        // 1. محرك جلب البيانات (Server-side) مع فلترة متطورة
        //public IActionResult OnPostLoadData()
        //{
        //    try
        //    {
        //        var globalSearch = Request.Form["search[value]"].FirstOrDefault()?.ToUpper();
        //        var draw = Request.Form["draw"].FirstOrDefault();
        //        var start = int.Parse(Request.Form["start"].FirstOrDefault() ?? "0");
        //        var length = int.Parse(Request.Form["length"].FirstOrDefault() ?? "25");

        //        using var conn = new SqliteConnection(_sqliteService.GetSqliteConnectionString());
        //        conn.Open();

        //        // بناء جملة الفلترة الديناميكية
        //        string filterSql = " WHERE 1=1 ";
        //        var sqlParams = new List<SqliteParameter>();

        //        for (int i = 0; i < 11; i++) // نمر على الأعمدة التي بها فلتر
        //        {
        //            string val = Request.Form[$"columns[{i}][search][value]"].FirstOrDefault();
        //            if (string.IsNullOrEmpty(val)) continue;

        //            string colName = GetColumnName(i);
        //            // إذا كان اختياراً متعدداً من فلتر الإكسل
        //            if (val.StartsWith("^(") && val.EndsWith(")$"))
        //            {
        //                string cleanVal = val.Substring(2, val.Length - 4);
        //                string[] options = cleanVal.Split('|');
        //                var pNames = new List<string>();
        //                for (int j = 0; j < options.Length; j++)
        //                {
        //                    string pName = $"@c{i}_{j}";
        //                    sqlParams.Add(new SqliteParameter(pName, System.Text.RegularExpressions.Regex.Unescape(options[j])));
        //                    pNames.Add(pName);
        //                }
        //                filterSql += $" AND [{colName}] IN ({string.Join(",", pNames)}) ";
        //            }
        //            else // بحث عادي (LIKE)
        //            {
        //                sqlParams.Add(new SqliteParameter($"@p{i}", $"%{val.ToUpper()}%"));
        //                filterSql += $" AND [{colName}] LIKE @p{i} ";
        //            }
        //        }

        //        // حساب الأعداد والمجاميع
        //        long totalRecords = (long)new SqliteCommand("SELECT COUNT(*) FROM ST_STOCK", conn).ExecuteScalar();

        //        var filterCmd = new SqliteCommand("SELECT COUNT(*) FROM ST_STOCK" + filterSql, conn);
        //        filterCmd.Parameters.AddRange(sqlParams.Select(p => new SqliteParameter(p.ParameterName, p.Value)).ToArray());
        //        long filteredRecords = (long)filterCmd.ExecuteScalar();

        //        // حساب إجمالي المبلغ للنتائج المفلترة فقط
        //        var sumCmd = new SqliteCommand("SELECT SUM(QTE * PAMP) FROM ST_STOCK" + filterSql, conn);
        //        sumCmd.Parameters.AddRange(sqlParams.Select(p => new SqliteParameter(p.ParameterName, p.Value)).ToArray());
        //        var sumRes = sumCmd.ExecuteScalar();
        //        double filteredTotal = sumRes != DBNull.Value ? Convert.ToDouble(sumRes) : 0;

        //        // جلب البيانات النهائية
        //        var list = new List<object>();
        //        string dataSql = $"SELECT * FROM ST_STOCK {filterSql} LIMIT @lim OFFSET @off";
        //        var dataCmd = new SqliteCommand(dataSql, conn);
        //        dataCmd.Parameters.AddRange(sqlParams.Select(p => new SqliteParameter(p.ParameterName, p.Value)).ToArray());
        //        dataCmd.Parameters.AddWithValue("@lim", length);
        //        dataCmd.Parameters.AddWithValue("@off", start);

        //        using (var reader = dataCmd.ExecuteReader())
        //        {
        //            while (reader.Read())
        //            {
        //                list.Add(new
        //                {
        //                    @ref = reader["REF"]?.ToString(),
        //                    intitule = reader["INTITULE"]?.ToString(),
        //                    intitule2 = reader["INTITULE2"]?.ToString(),
        //                    intitule3 = reader["INTITULE3"]?.ToString(),
        //                    famille = reader["FAMILLE"]?.ToString(),
        //                    qte = Convert.ToDouble(reader["QTE"] == DBNull.Value ? 0 : reader["QTE"]),
        //                    pamp = Convert.ToDouble(reader["PAMP"] == DBNull.Value ? 0 : reader["PAMP"]),
        //                    stock_ini = Convert.ToDouble(reader["STOCK_INI"] == DBNull.Value ? 0 : reader["STOCK_INI"]),
        //                    casier = reader["CASIER"]?.ToString(),
        //                    date_maj = reader["DATE_MAJ"]?.ToString(),
        //                    // حقول الـ Modal
        //                    lieu_sto = reader["LIEU_STO"]?.ToString(),
        //                    priX_ACH = Convert.ToDouble(reader["PRIX_ACH"] == DBNull.Value ? 0 : reader["PRIX_ACH"]),
        //                    stocK_MAX = Convert.ToDouble(reader["STOCK_MAX"] == DBNull.Value ? 0 : reader["STOCK_MAX"]),
        //                    stocK_SEC = Convert.ToDouble(reader["STOCK_SEC"] == DBNull.Value ? 0 : reader["STOCK_SEC"]),
        //                    valeuR_INI = Convert.ToDouble(reader["VALEUR_INI"] == DBNull.Value ? 0 : reader["VALEUR_INI"]),
        //                    entrees = Convert.ToDouble(reader["ENTREES"] == DBNull.Value ? 0 : reader["ENTREES"]),
        //                    sorties = Convert.ToDouble(reader["SORTIES"] == DBNull.Value ? 0 : reader["SORTIES"]),
        //                    qtE_ENT = Convert.ToDouble(reader["QTE_ENT"] == DBNull.Value ? 0 : reader["QTE_ENT"]),
        //                    qtE_SOR = Convert.ToDouble(reader["QTE_SOR"] == DBNull.Value ? 0 : reader["QTE_SOR"]),
        //                    tempval = Convert.ToDouble(reader["TEMPVAL"] == DBNull.Value ? 0 : reader["TEMPVAL"]),
        //                    achaT_HT = Convert.ToDouble(reader["ACHAT_HT"] == DBNull.Value ? 0 : reader["ACHAT_HT"]),
        //                    consO_HT = Convert.ToDouble(reader["CONSO_HT"] == DBNull.Value ? 0 : reader["CONSO_HT"]),
        //                    cesS_HT = Convert.ToDouble(reader["CESS_HT"] == DBNull.Value ? 0 : reader["CESS_HT"]),
        //                    cessR_HT = Convert.ToDouble(reader["CESSR_HT"] == DBNull.Value ? 0 : reader["CESSR_HT"])
        //                });
        //            }
        //        }

        //        return new JsonResult(new { draw, recordsTotal = totalRecords, recordsFiltered = filteredRecords, data = list, filterSum = filteredTotal });
        //    }
        //    catch (Exception ex) { return new JsonResult(new { error = ex.Message }); }
        //}

        public IActionResult OnPostLoadData()
        {
            try
            {
                // 1. استقبال بارامترات التحكم الأساسية من DataTables
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = int.Parse(Request.Form["start"].FirstOrDefault() ?? "0");
                var length = int.Parse(Request.Form["length"].FirstOrDefault() ?? "25");

                // جلب قيمة البحث العام (الحقل الجديد)
                var globalSearch = Request.Form["search[value]"].FirstOrDefault()?.Trim().ToUpper();

                using var conn = new SqliteConnection(_sqliteService.GetSqliteConnectionString());
                conn.Open();

                // 2. بناء جملة الفلترة الديناميكية
                string filterSql = " WHERE 1=1 ";
                var sqlParams = new List<SqliteParameter>();

                // --- أ. منطق البحث العام (البحث في عدة حقول بـ OR) ---
                if (!string.IsNullOrEmpty(globalSearch))
                {
                    filterSql += @" AND (
                [REF] LIKE @gs OR 
                [INTITULE] LIKE @gs OR 
                [INTITULE2] LIKE @gs OR 
                [FAMILLE] LIKE @gs OR 
                [CASIER] LIKE @gs
            ) ";
                    sqlParams.Add(new SqliteParameter("@gs", $"%{globalSearch}%"));
                }

                // --- ب. منطق فلاتر الأعمدة (Excel Style) ---
                for (int i = 0; i < 11; i++)
                {
                    string val = Request.Form[$"columns[{i}][search][value]"].FirstOrDefault();
                    if (string.IsNullOrEmpty(val)) continue;

                    string colName = GetColumnName(i);

                    // حالة الاختيار المتعدد (من القائمة المنبثقة)
                    if (val.StartsWith("^(") && val.EndsWith(")$"))
                    {
                        string cleanVal = val.Substring(2, val.Length - 4);
                        string[] options = cleanVal.Split('|');
                        var pNames = new List<string>();

                        for (int j = 0; j < options.Length; j++)
                        {
                            string pName = $"@c{i}_{j}";
                            string finalVal = System.Text.RegularExpressions.Regex.Unescape(options[j]);
                            sqlParams.Add(new SqliteParameter(pName, finalVal));
                            pNames.Add(pName);
                        }
                        filterSql += $" AND [{colName}] IN ({string.Join(",", pNames)}) ";
                    }
                    // حالة البحث العادي داخل هيدر العمود
                    else
                    {
                        string pName = $"@p{i}";
                        sqlParams.Add(new SqliteParameter(pName, $"%{val.ToUpper()}%"));
                        filterSql += $" AND [{colName}] LIKE {pName} ";
                    }
                }

                // 3. حساب الأعداد والمجاميع النهائية
                // أ. العدد الإجمالي للجدول
                long totalRecords = (long)new SqliteCommand("SELECT COUNT(*) FROM ST_STOCK", conn).ExecuteScalar();

                // ب. عدد السجلات المفلترة (تكرار البارامترات لأننا لا نستخدم Clone)
                var filterCmd = new SqliteCommand("SELECT COUNT(*) FROM ST_STOCK" + filterSql, conn);
                foreach (var p in sqlParams) filterCmd.Parameters.Add(new SqliteParameter(p.ParameterName, p.Value));
                long filteredRecords = (long)filterCmd.ExecuteScalar();

                // ج. مجموع المبالغ المفلترة (لشاشة الـ LCD)
                var sumCmd = new SqliteCommand("SELECT SUM(QTE * PAMP) FROM ST_STOCK" + filterSql, conn);
                foreach (var p in sqlParams) sumCmd.Parameters.Add(new SqliteParameter(p.ParameterName, p.Value));
                var sumRes = sumCmd.ExecuteScalar();
                double filteredTotal = sumRes != DBNull.Value ? Convert.ToDouble(sumRes) : 0;

                // 4. جلب البيانات النهائية للصفحة الحالية فقط
                var list = new List<object>();
                string dataSql = $"SELECT * FROM ST_STOCK {filterSql} LIMIT @lim OFFSET @off";
                var dataCmd = new SqliteCommand(dataSql, conn);
                foreach (var p in sqlParams) dataCmd.Parameters.Add(new SqliteParameter(p.ParameterName, p.Value));
                dataCmd.Parameters.AddWithValue("@lim", length);
                dataCmd.Parameters.AddWithValue("@off", start);

                using (var reader = dataCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new
                        {
                            REF = reader["REF"]?.ToString(),
                            intitule = reader["INTITULE"]?.ToString(),
                            intitule2 = reader["INTITULE2"]?.ToString(),
                            intitule3 = reader["INTITULE3"]?.ToString(),
                            famille = reader["FAMILLE"]?.ToString(),
                            qte = reader["QTE"] != DBNull.Value ? Convert.ToDouble(reader["QTE"]) : 0,
                            pamp = reader["PAMP"] != DBNull.Value ? Convert.ToDouble(reader["PAMP"]) : 0,
                            stock_ini = reader["STOCK_INI"] != DBNull.Value ? Convert.ToDouble(reader["STOCK_INI"]) : 0,
                            casier = reader["CASIER"]?.ToString(),
                            date_maj = reader["DATE_MAJ"]?.ToString(),

                            // حقول إضافية للـ Modal
                            lieu_sto = reader["LIEU_STO"]?.ToString(),
                            priX_ACH = reader["PRIX_ACH"] != DBNull.Value ? Convert.ToDouble(reader["PRIX_ACH"]) : 0,
                            stocK_MAX = reader["STOCK_MAX"] != DBNull.Value ? Convert.ToDouble(reader["STOCK_MAX"]) : 0,
                            stocK_SEC = reader["STOCK_SEC"] != DBNull.Value ? Convert.ToDouble(reader["STOCK_SEC"]) : 0,
                            valeuR_INI = reader["VALEUR_INI"] != DBNull.Value ? Convert.ToDouble(reader["VALEUR_INI"]) : 0,
                            entrees = reader["ENTREES"] != DBNull.Value ? Convert.ToDouble(reader["ENTREES"]) : 0,
                            sorties = reader["SORTIES"] != DBNull.Value ? Convert.ToDouble(reader["SORTIES"]) : 0,
                            qtE_ENT = reader["QTE_ENT"] != DBNull.Value ? Convert.ToDouble(reader["QTE_ENT"]) : 0,
                            qtE_SOR = reader["QTE_SOR"] != DBNull.Value ? Convert.ToDouble(reader["QTE_SOR"]) : 0,
                            tempval = reader["TEMPVAL"] != DBNull.Value ? Convert.ToDouble(reader["TEMPVAL"]) : 0,
                            achaT_HT = reader["ACHAT_HT"] != DBNull.Value ? Convert.ToDouble(reader["ACHAT_HT"]) : 0,
                            consO_HT = reader["CONSO_HT"] != DBNull.Value ? Convert.ToDouble(reader["CONSO_HT"]) : 0,
                            cesS_HT = reader["CESS_HT"] != DBNull.Value ? Convert.ToDouble(reader["CESS_HT"]) : 0,
                            cessR_HT = reader["CESSR_HT"] != DBNull.Value ? Convert.ToDouble(reader["CESSR_HT"]) : 0
                        });
                    }
                }

                return new JsonResult(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = filteredRecords,
                    data = list,
                    filterSum = filteredTotal
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { error = ex.Message });
            }
        }
        // جلب القيم الفريدة لفلتر الإكسل
        public IActionResult OnGetUniqueValues(string columnName)
        {
            var values = new List<string>();
            using (var conn = new SqliteConnection(_sqliteService.GetSqliteConnectionString()))
            {
                conn.Open();
                var cmd = new SqliteCommand($"SELECT DISTINCT [{columnName}] FROM ST_STOCK WHERE [{columnName}] != '' ORDER BY [{columnName}]", conn);
                using var r = cmd.ExecuteReader();
                while (r.Read()) values.Add(r[0].ToString().Trim());
            }
            return new JsonResult(values);
        }

        // تحديث المنتج (DBF + SQLite)
        public IActionResult OnPostUpdateArticle([FromForm] string articleRef, [FromForm] string intitule, [FromForm] string intitule2, [FromForm] string intitule3, [FromForm] string famille, [FromForm] string lieu_sto, [FromForm] string casier)
        {
            try
            {
                string dbfPath = _configService.GetDbPath();
                string dbfConn = $@"Driver={{Microsoft Access dBASE Driver (*.dbf, *.ndx, *.mdx)}};Dbq={dbfPath};ReadOnly=0;";

                // 1. DBF Update
                using (var conn = new OdbcConnection(dbfConn))
                {
                    conn.Open();
                    string sql = "UPDATE ST_STOCK.DBF SET INTITULE=?, INTITULE2=?, INTITULE3=?, FAMILLE=?, LIEU_STO=?, CASIER=? WHERE TRIM(REF)=?";
                    using var cmd = new OdbcCommand(sql, conn);
                    cmd.Parameters.AddWithValue("p1", intitule?.ToUpper() ?? "");
                    cmd.Parameters.AddWithValue("p2", intitule2?.ToUpper() ?? "");
                    cmd.Parameters.AddWithValue("p3", intitule3?.ToUpper() ?? "");
                    cmd.Parameters.AddWithValue("p4", famille?.ToUpper() ?? "");
                    cmd.Parameters.AddWithValue("p5", lieu_sto?.ToUpper() ?? "");
                    cmd.Parameters.AddWithValue("p6", casier?.ToUpper() ?? "");
                    cmd.Parameters.AddWithValue("p7", articleRef.Trim());
                    cmd.ExecuteNonQuery();
                }

                // 2. SQLite Update
                using (var conn = new SqliteConnection(_sqliteService.GetSqliteConnectionString()))
                {
                    conn.Open();
                    string sql = "UPDATE ST_STOCK SET INTITULE=@i, INTITULE2=@i2, INTITULE3=@i3, FAMILLE=@f, LIEU_STO=@l, CASIER=@c WHERE REF=@r";
                    using var cmd = new SqliteCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@i", intitule?.ToUpper() ?? "");
                    cmd.Parameters.AddWithValue("@i2", intitule2?.ToUpper() ?? "");
                    cmd.Parameters.AddWithValue("@i3", intitule3?.ToUpper() ?? "");
                    cmd.Parameters.AddWithValue("@f", famille?.ToUpper() ?? "");
                    cmd.Parameters.AddWithValue("@l", lieu_sto?.ToUpper() ?? "");
                    cmd.Parameters.AddWithValue("@c", casier?.ToUpper() ?? "");
                    cmd.Parameters.AddWithValue("@r", articleRef);
                    cmd.ExecuteNonQuery();
                }
                return new JsonResult(new { success = true, message = "Mis à jour avec succès !" });
            }
            catch (Exception ex) { return new JsonResult(new { success = false, message = ex.Message }); }
        }

        private string GetColumnName(int index) => index switch { 0 => "REF", 1 => "INTITULE", 2 => "INTITULE2", 3 => "INTITULE3", 4 => "FAMILLE", 9 => "CASIER", 10 => "DATE_MAJ", _ => "REF" };
    }
}
