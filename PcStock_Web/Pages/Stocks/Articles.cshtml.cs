using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PcStock_Web.Pages.Stocks
{
    public class ArticlesModel : PageModel
    {
        private readonly SqliteDbService _sqliteService;
        private readonly ConfigService _configService;

        public ArticlesModel(SqliteDbService sqliteService, ConfigService configService)
        {
            _sqliteService = sqliteService;
            _configService = configService;
        }

        public void OnGet()
        {
            // تفتح الصفحة فارغة ويتم جلب البيانات عبر Ajax (LoadData)
        }

        // 1. المحرك الرئيسي لجلب البيانات مع الفلترة والتقسيم (Pagination)
        //public IActionResult OnPostLoadData()
        //{
        //    try
        //    {
        //        // استقبال بارامترات التحكم من DataTables
        //        var draw = Request.Form["draw"].FirstOrDefault();
        //        var start = Request.Form["start"].FirstOrDefault();
        //        var length = Request.Form["length"].FirstOrDefault();

        //        // قراءة قيم الفلترة من رأس الأعمدة (حسب الترتيب في الجدول)
        //        string fRef = Request.Form["columns[0][search][value]"].FirstOrDefault();
        //        string fInt1 = Request.Form["columns[1][search][value]"].FirstOrDefault();
        //        string fInt2 = Request.Form["columns[2][search][value]"].FirstOrDefault();
        //        string fInt3 = Request.Form["columns[3][search][value]"].FirstOrDefault();
        //        string fFam = Request.Form["columns[4][search][value]"].FirstOrDefault();
        //        string fQte = Request.Form["columns[5][search][value]"].FirstOrDefault();
        //        string fPamp = Request.Form["columns[6][search][value]"].FirstOrDefault();
        //        string fCasier = Request.Form["columns[9][search][value]"].FirstOrDefault();
        //        string fDate = Request.Form["columns[10][search][value]"].FirstOrDefault();

        //        var list = new List<object>();
        //        int totalRecords = 0;
        //        int filteredRecords = 0;

        //        using (var conn = new SqliteConnection(_sqliteService.GetSqliteConnectionString()))
        //        {
        //            conn.Open();

        //            // أ. جلب العدد الإجمالي بدون فلاتر
        //            totalRecords = Convert.ToInt32(new SqliteCommand("SELECT COUNT(*) FROM ST_STOCK", conn).ExecuteScalar());

        //            // ب. بناء استعلام الفلترة الديناميكي
        //            string filterSql = " WHERE 1=1 ";
        //            var filterCmd = new SqliteCommand("", conn);

        //            if (!string.IsNullOrEmpty(fRef)) { filterSql += " AND REF LIKE @ref "; filterCmd.Parameters.AddWithValue("@ref", $"%{fRef.ToUpper()}%"); }
        //            if (!string.IsNullOrEmpty(fInt1)) { filterSql += " AND INTITULE LIKE @int1 "; filterCmd.Parameters.AddWithValue("@int1", $"%{fInt1.ToUpper()}%"); }
        //            if (!string.IsNullOrEmpty(fInt2)) { filterSql += " AND INTITULE2 LIKE @int2 "; filterCmd.Parameters.AddWithValue("@int2", $"%{fInt2.ToUpper()}%"); }
        //            if (!string.IsNullOrEmpty(fInt3)) { filterSql += " AND INTITULE3 LIKE @int3 "; filterCmd.Parameters.AddWithValue("@int3", $"%{fInt3.ToUpper()}%"); }
        //            if (!string.IsNullOrEmpty(fFam)) { filterSql += " AND FAMILLE LIKE @fam "; filterCmd.Parameters.AddWithValue("@fam", $"%{fFam}%"); }
        //            if (!string.IsNullOrEmpty(fCasier)) { filterSql += " AND CASIER LIKE @cas "; filterCmd.Parameters.AddWithValue("@cas", $"%{fCasier.ToUpper()}%"); }
        //            if (!string.IsNullOrEmpty(fDate)) { filterSql += " AND DATE_MAJ LIKE @date "; filterCmd.Parameters.AddWithValue("@date", $"%{fDate}%"); }

        //            // البحث في الأرقام (الكمية والسعر)
        //            if (!string.IsNullOrEmpty(fQte)) { filterSql += " AND CAST(QTE AS TEXT) LIKE @qte "; filterCmd.Parameters.AddWithValue("@qte", $"%{fQte}%"); }
        //            if (!string.IsNullOrEmpty(fPamp)) { filterSql += " AND CAST(PAMP AS TEXT) LIKE @pamp "; filterCmd.Parameters.AddWithValue("@pamp", $"%{fPamp}%"); }

        //            // ج. حساب عدد السجلات بعد تطبيق الفلترة
        //            filterCmd.CommandText = "SELECT COUNT(*) FROM ST_STOCK " + filterSql;
        //            filteredRecords = Convert.ToInt32(filterCmd.ExecuteScalar());

        //            // د. جلب البيانات النهائية (الصفحة الحالية فقط)
        //            string dataSql = $"SELECT * FROM ST_STOCK {filterSql} LIMIT @limit OFFSET @offset";
        //            var dataCmd = new SqliteCommand(dataSql, conn);
        //            dataCmd.Parameters.AddRange(filterCmd.Parameters.Cast<SqliteParameter>().Select(p => p.Clone()).ToArray());
        //            dataCmd.Parameters.AddWithValue("@limit", int.Parse(length ?? "25"));
        //            dataCmd.Parameters.AddWithValue("@offset", int.Parse(start ?? "0"));

        //            using (var reader = dataCmd.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    list.Add(new
        //                    {
        //                        // الأسماء بحروف صغيرة لتتطابق مع الـ JavaScript
        //                        REF = reader["REF"]?.ToString(),
        //                        intitule = reader["INTITULE"]?.ToString(),
        //                        intitule2 = reader["INTITULE2"]?.ToString(),
        //                        intitule3 = reader["INTITULE3"]?.ToString(),
        //                        famille = reader["FAMILLE"]?.ToString(),
        //                        qte = reader["QTE"] != DBNull.Value ? Convert.ToDouble(reader["QTE"]) : 0,
        //                        pamp = reader["PAMP"] != DBNull.Value ? Convert.ToDouble(reader["PAMP"]) : 0,
        //                        stock_ini = reader["STOCK_INI"] != DBNull.Value ? Convert.ToDouble(reader["STOCK_INI"]) : 0,
        //                        casier = reader["CASIER"]?.ToString(),
        //                        date_maj = reader["DATE_MAJ"]?.ToString(),

        //                        // حقول إضافية لنافذة التعديل (Modal)
        //                        lieu_sto = reader["LIEU_STO"]?.ToString(),
        //                        priX_ACH = reader["PRIX_ACH"] != DBNull.Value ? Convert.ToDouble(reader["PRIX_ACH"]) : 0,
        //                        stocK_MAX = reader["STOCK_MAX"] != DBNull.Value ? Convert.ToDouble(reader["STOCK_MAX"]) : 0,
        //                        stocK_SEC = reader["STOCK_SEC"] != DBNull.Value ? Convert.ToDouble(reader["STOCK_SEC"]) : 0,
        //                        valeuR_INI = reader["VALEUR_INI"] != DBNull.Value ? Convert.ToDouble(reader["VALEUR_INI"]) : 0,
        //                        entrees = reader["ENTREES"] != DBNull.Value ? Convert.ToDouble(reader["ENTREES"]) : 0,
        //                        sorties = reader["SORTIES"] != DBNull.Value ? Convert.ToDouble(reader["SORTIES"]) : 0,
        //                        qtE_ENT = reader["QTE_ENT"] != DBNull.Value ? Convert.ToDouble(reader["QTE_ENT"]) : 0,
        //                        qtE_SOR = reader["QTE_SOR"] != DBNull.Value ? Convert.ToDouble(reader["QTE_SOR"]) : 0,
        //                        tempval = reader["TEMPVAL"] != DBNull.Value ? Convert.ToDouble(reader["TEMPVAL"]) : 0,
        //                        achaT_HT = reader["ACHAT_HT"] != DBNull.Value ? Convert.ToDouble(reader["ACHAT_HT"]) : 0,
        //                        consO_HT = reader["CONSO_HT"] != DBNull.Value ? Convert.ToDouble(reader["CONSO_HT"]) : 0,
        //                        cesS_HT = reader["CESS_HT"] != DBNull.Value ? Convert.ToDouble(reader["CESS_HT"]) : 0,
        //                        cessR_HT = reader["CESSR_HT"] != DBNull.Value ? Convert.ToDouble(reader["CESSR_HT"]) : 0
        //                    });
        //                }
        //            }
        //        }

        //        return new JsonResult(new { draw = draw, recordsTotal = totalRecords, recordsFiltered = filteredRecords, data = list });
        //    }
        //    catch (Exception ex)
        //    {
        //        return new JsonResult(new { error = ex.Message });
        //    }
        //}

        public IActionResult OnPostLoadData()
        {
            try
            {
                // 1. استقبال بارامترات التحكم من DataTables
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();

                // قراءة قيم الفلترة من رأس الأعمدة
                string fRef = Request.Form["columns[0][search][value]"].FirstOrDefault();
                string fInt1 = Request.Form["columns[1][search][value]"].FirstOrDefault();
                string fInt2 = Request.Form["columns[2][search][value]"].FirstOrDefault();
                string fInt3 = Request.Form["columns[3][search][value]"].FirstOrDefault();
                string fFam = Request.Form["columns[4][search][value]"].FirstOrDefault();
                string fQte = Request.Form["columns[5][search][value]"].FirstOrDefault();
                string fPamp = Request.Form["columns[6][search][value]"].FirstOrDefault();
                string fCasier = Request.Form["columns[9][search][value]"].FirstOrDefault();
                string fDate = Request.Form["columns[10][search][value]"].FirstOrDefault();

                var list = new List<object>();
                int totalRecords = 0;
                int filteredRecords = 0;

                using (var conn = new SqliteConnection(_sqliteService.GetSqliteConnectionString()))
                {
                    conn.Open();

                    // أ. جلب العدد الإجمالي بدون فلاتر
                    totalRecords = Convert.ToInt32(new SqliteCommand("SELECT COUNT(*) FROM ST_STOCK", conn).ExecuteScalar());

                    // ب. بناء استعلام الفلترة وشروط الـ WHERE
                    string filterSql = " WHERE 1=1 ";
                    // نستخدم قائمة لتخزين البارامترات لكي نتمكن من إضافتها للأوامر لاحقاً
                    var sqlParams = new List<SqliteParameter>();

                    if (!string.IsNullOrEmpty(fRef)) { filterSql += " AND REF LIKE @ref "; sqlParams.Add(new SqliteParameter("@ref", $"%{fRef.ToUpper()}%")); }
                    if (!string.IsNullOrEmpty(fInt1)) { filterSql += " AND INTITULE LIKE @int1 "; sqlParams.Add(new SqliteParameter("@int1", $"%{fInt1.ToUpper()}%")); }
                    if (!string.IsNullOrEmpty(fInt2)) { filterSql += " AND INTITULE2 LIKE @int2 "; sqlParams.Add(new SqliteParameter("@int2", $"%{fInt2.ToUpper()}%")); }
                    if (!string.IsNullOrEmpty(fInt3)) { filterSql += " AND INTITULE3 LIKE @int3 "; sqlParams.Add(new SqliteParameter("@int3", $"%{fInt3.ToUpper()}%")); }
                    if (!string.IsNullOrEmpty(fFam)) { filterSql += " AND FAMILLE LIKE @fam "; sqlParams.Add(new SqliteParameter("@fam", $"%{fFam}%")); }
                    if (!string.IsNullOrEmpty(fCasier)) { filterSql += " AND CASIER LIKE @cas "; sqlParams.Add(new SqliteParameter("@cas", $"%{fCasier.ToUpper()}%")); }
                    if (!string.IsNullOrEmpty(fDate)) { filterSql += " AND DATE_MAJ LIKE @date "; sqlParams.Add(new SqliteParameter("@date", $"%{fDate}%")); }

                    if (!string.IsNullOrEmpty(fQte)) { filterSql += " AND CAST(QTE AS TEXT) LIKE @qte "; sqlParams.Add(new SqliteParameter("@qte", $"%{fQte}%")); }
                    if (!string.IsNullOrEmpty(fPamp)) { filterSql += " AND CAST(PAMP AS TEXT) LIKE @pamp "; sqlParams.Add(new SqliteParameter("@pamp", $"%{fPamp}%")); }

                    // ج. حساب عدد السجلات المفلترة
                    var countCmd = new SqliteCommand("SELECT COUNT(*) FROM ST_STOCK" + filterSql, conn);
                    foreach (var p in sqlParams) countCmd.Parameters.Add(new SqliteParameter(p.ParameterName, p.Value));
                    filteredRecords = Convert.ToInt32(countCmd.ExecuteScalar());

                    // د. جلب البيانات النهائية (Pagination)
                    string dataSql = $"SELECT * FROM ST_STOCK {filterSql} LIMIT @limit OFFSET @offset";
                    var dataCmd = new SqliteCommand(dataSql, conn);

                    // إضافة بارامترات الفلترة مرة أخرى للأمر الجديد
                    foreach (var p in sqlParams) dataCmd.Parameters.Add(new SqliteParameter(p.ParameterName, p.Value));

                    dataCmd.Parameters.AddWithValue("@limit", int.Parse(length ?? "25"));
                    dataCmd.Parameters.AddWithValue("@offset", int.Parse(start ?? "0"));

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

                                // حقول الـ Modal
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
                }

                return new JsonResult(new { draw = draw, recordsTotal = totalRecords, recordsFiltered = filteredRecords, data = list });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { error = ex.Message });
            }
        }

        // 2. تحديث المنتج في ملف DBF وقاعدة SQLite
        public IActionResult OnPostUpdateArticle([FromForm] string articleRef, [FromForm] string intitule, [FromForm] string intitule2, [FromForm] string intitule3, [FromForm] string famille, [FromForm] string lieu_sto, [FromForm] string casier)
        {
            if (string.IsNullOrEmpty(articleRef))
                return new JsonResult(new { success = false, message = "Erreur: La référence est manquante." });

            string dbfPath = _configService.GetDbPath();
            string sqliteConnString = _sqliteService.GetSqliteConnectionString();

            // تنظيف البيانات (Uppercase)
            intitule = intitule?.ToUpper() ?? "";
            intitule2 = intitule2?.ToUpper() ?? "";
            intitule3 = intitule3?.ToUpper() ?? "";
            famille = famille?.ToUpper() ?? "";
            lieu_sto = lieu_sto?.ToUpper() ?? "";
            casier = casier?.ToUpper() ?? "";

            try
            {
                // أ. التعديل في ملف DBF (الأصلي)
                string dbfConnStr = $@"Driver={{Microsoft Access dBASE Driver (*.dbf, *.ndx, *.mdx)}};Dbq={dbfPath};ReadOnly=0;";
                using (System.Data.Odbc.OdbcConnection dbfConn = new System.Data.Odbc.OdbcConnection(dbfConnStr))
                {
                    dbfConn.Open();
                    string sqlDbf = @"UPDATE ST_STOCK.DBF SET INTITULE=?, INTITULE2=?, INTITULE3=?, FAMILLE=?, LIEU_STO=?, CASIER=? WHERE TRIM(REF)=?";
                    using (var cmd = new System.Data.Odbc.OdbcCommand(sqlDbf, dbfConn))
                    {
                        cmd.Parameters.AddWithValue("p1", intitule);
                        cmd.Parameters.AddWithValue("p2", intitule2);
                        cmd.Parameters.AddWithValue("p3", intitule3);
                        cmd.Parameters.AddWithValue("p4", famille);
                        cmd.Parameters.AddWithValue("p5", lieu_sto);
                        cmd.Parameters.AddWithValue("p6", casier);
                        cmd.Parameters.AddWithValue("p7", articleRef.Trim());
                        cmd.ExecuteNonQuery();
                    }
                }

                // ب. التعديل في SQLite (العرض الفوري في الويب)
                using (var sqliteConn = new SqliteConnection(sqliteConnString))
                {
                    sqliteConn.Open();
                    string sqlSqlite = @"UPDATE ST_STOCK SET INTITULE=@i, INTITULE2=@i2, INTITULE3=@i3, FAMILLE=@f, LIEU_STO=@l, CASIER=@c WHERE REF=@r";
                    using (var cmd = new SqliteCommand(sqlSqlite, sqliteConn))
                    {
                        cmd.Parameters.AddWithValue("@i", intitule);
                        cmd.Parameters.AddWithValue("@i2", intitule2);
                        cmd.Parameters.AddWithValue("@i3", intitule3);
                        cmd.Parameters.AddWithValue("@f", famille);
                        cmd.Parameters.AddWithValue("@l", lieu_sto);
                        cmd.Parameters.AddWithValue("@c", casier);
                        cmd.Parameters.AddWithValue("@r", articleRef);
                        cmd.ExecuteNonQuery();
                    }
                }

                return new JsonResult(new { success = true, message = "L'article a été mis à jour avec succès (DBF & SQLite) !" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Erreur : " + ex.Message });
            }
        }
    }
}
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using Microsoft.Data.Sqlite; // تأكد من استخدام مكتبة SQLite
//using System.Collections.Generic;

//namespace PcStock_Web.Pages.Stocks
//{
//    public class ArticlesModel : PageModel
//    {
//        private readonly SqliteDbService _sqliteService;
//        private readonly ConfigService _configService;

//        public ArticlesModel(SqliteDbService sqliteService, ConfigService configService)
//        {
//            _sqliteService = sqliteService;
//            _configService = configService;
//        }

//        public void OnGet()
//        {
//            // الصفحة تفتح فارغة والبيانات تُجلب عبر Ajax لسرعة العرض
//        }

//        public IActionResult OnPostLoadData()
//        {
//            // 1. استقبال بارامترات DataTables
//            var draw = Request.Form["draw"].FirstOrDefault();
//            var start = Request.Form["start"].FirstOrDefault();
//            var length = Request.Form["length"].FirstOrDefault();
//            var searchValue = Request.Form["search[value]"].FirstOrDefault()?.Trim().ToUpper();

//            int pageSize = length != null ? int.Parse(length) : 25;
//            int skip = start != null ? int.Parse(start) : 0;

//            var list = new List<object>();
//            int totalRecords = 0;
//            int filteredRecords = 0;

//            string connString = _sqliteService.GetSqliteConnectionString();

//            using (var conn = new SqliteConnection(connString))
//            {
//                conn.Open();

//                // أ. جلب العدد الإجمالي (سريع جداً في SQLite)
//                var countCmd = new SqliteCommand("SELECT COUNT(*) FROM ST_STOCK", conn);
//                totalRecords = Convert.ToInt32(countCmd.ExecuteScalar());

//                // ب. بناء استعلام البيانات مع البحث والتقسيم (Paging)
//                string filterSql = "";
//                if (!string.IsNullOrEmpty(searchValue))
//                {
//                    filterSql = " WHERE REF LIKE @search OR INTITULE LIKE @search";
//                }

//                // جلب عدد السجلات المفلترة
//                var filteredCountCmd = new SqliteCommand($"SELECT COUNT(*) FROM ST_STOCK {filterSql}", conn);
//                if (!string.IsNullOrEmpty(searchValue)) filteredCountCmd.Parameters.AddWithValue("@search", $"%{searchValue}%");
//                filteredRecords = Convert.ToInt32(filteredCountCmd.ExecuteScalar());

//                // ج. جلب الصفحة الحالية فقط (هذا سر السرعة)
//                string dataSql = $"SELECT * FROM ST_STOCK {filterSql} LIMIT @limit OFFSET @offset";
//                var cmd = new SqliteCommand(dataSql, conn);
//                cmd.Parameters.AddWithValue("@limit", pageSize);
//                cmd.Parameters.AddWithValue("@offset", skip);
//                if (!string.IsNullOrEmpty(searchValue)) cmd.Parameters.AddWithValue("@search", $"%{searchValue}%");

//                using (var reader = cmd.ExecuteReader())
//                {
//                    while (reader.Read())
//                    {
//                        list.Add(new
//                        {
//                            REF = reader["REF"]?.ToString(),
//                            intitule = reader["INTITULE"]?.ToString(),
//                            intitule2 = reader["INTITULE2"]?.ToString(),
//                            intitule3 = reader["INTITULE3"]?.ToString(),
//                            famille = reader["FAMILLE"]?.ToString(),
//                            qte = reader["QTE"] != DBNull.Value ? Convert.ToDouble(reader["QTE"]) : 0,
//                            pamp = reader["PAMP"] != DBNull.Value ? Convert.ToDouble(reader["PAMP"]) : 0,
//                            stock_ini = reader["STOCK_INI"] != DBNull.Value ? Convert.ToDouble(reader["STOCK_INI"]) : 0,
//                            casier = reader["CASIER"]?.ToString(),
//                            date_maj = reader["DATE_MAJ"]?.ToString(),
//                            // الحقول الإضافية للـ Modal
//                            lieu_sto = reader["LIEU_STO"]?.ToString(),
//                            priX_ACH = reader["PRIX_ACH"] != DBNull.Value ? Convert.ToDouble(reader["PRIX_ACH"]) : 0,
//                            stocK_MAX = reader["STOCK_MAX"] != DBNull.Value ? Convert.ToDouble(reader["STOCK_MAX"]) : 0,
//                            stocK_SEC = reader["STOCK_SEC"] != DBNull.Value ? Convert.ToDouble(reader["STOCK_SEC"]) : 0,
//                            valeuR_INI = reader["VALEUR_INI"] != DBNull.Value ? Convert.ToDouble(reader["VALEUR_INI"]) : 0,
//                            entrees = reader["ENTREES"] != DBNull.Value ? Convert.ToDouble(reader["ENTREES"]) : 0,
//                            sorties = reader["SORTIES"] != DBNull.Value ? Convert.ToDouble(reader["SORTIES"]) : 0,
//                            qtE_ENT = reader["QTE_ENT"] != DBNull.Value ? Convert.ToDouble(reader["QTE_ENT"]) : 0,
//                            qtE_SOR = reader["QTE_SOR"] != DBNull.Value ? Convert.ToDouble(reader["QTE_SOR"]) : 0,
//                            tempval = reader["TEMPVAL"] != DBNull.Value ? Convert.ToDouble(reader["TEMPVAL"]) : 0,
//                            achaT_HT = reader["ACHAT_HT"] != DBNull.Value ? Convert.ToDouble(reader["ACHAT_HT"]) : 0,
//                            consO_HT = reader["CONSO_HT"] != DBNull.Value ? Convert.ToDouble(reader["CONSO_HT"]) : 0,
//                            cesS_HT = reader["CESS_HT"] != DBNull.Value ? Convert.ToDouble(reader["CESS_HT"]) : 0,
//                            cessR_HT = reader["CESSR_HT"] != DBNull.Value ? Convert.ToDouble(reader["CESSR_HT"]) : 0
//                        });
//                    }
//                }
//            }

//            return new JsonResult(new
//            {
//                draw = draw,
//                recordsTotal = totalRecords,
//                recordsFiltered = filteredRecords,
//                data = list
//            });
//        }


//        // هذا الـ Handler هو الذي يتصل به كود الجافا سكريبت (?handler=AllData)
//        public IActionResult OnGetAllData()
//        {
//            var list = new List<object>();
//            string connString = _sqliteService.GetSqliteConnectionString();

//            using (var conn = new SqliteConnection(connString))
//            {
//                try
//                {
//                    conn.Open();
//                    // جلب كل الحقول من جدول ST_STOCK الموجود في SQLite
//                    // قمت بجلب كل الحقول لكي تعمل نافذة التعديل (Modal) بالكامل
//                    string sql = "SELECT * FROM ST_STOCK";
//                    var cmd = new SqliteCommand(sql, conn);

//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            // نستخدم أسماء الحقول كما هي في الـ DBF الأصلي لأن SQLite نسخها بنفس الأسماء
//                            list.Add(new
//                            {
//                                // الحروف الصغيرة هنا مهمة لتطابق كود الـ JavaScript الخاص بك
//                                REF = reader["REF"]?.ToString(),
//                                intitule = reader["INTITULE"]?.ToString(),
//                                intitule2 = reader["INTITULE2"]?.ToString(),
//                                intitule3 = reader["INTITULE3"]?.ToString(),
//                                famille = reader["FAMILLE"]?.ToString(),
//                                qte = reader["QTE"] != DBNull.Value ? Convert.ToDouble(reader["QTE"]) : 0,
//                                pamp = reader["PAMP"] != DBNull.Value ? Convert.ToDouble(reader["PAMP"]) : 0,
//                                stock_ini = reader["STOCK_INI"] != DBNull.Value ? Convert.ToDouble(reader["STOCK_INI"]) : 0,
//                                casier = reader["CASIER"]?.ToString(),
//                                date_maj = reader["DATE_MAJ"]?.ToString(),

//                                // حقول إضافية لنافذة التعديل والإحصائيات
//                                lieu_sto = reader["LIEU_STO"]?.ToString(),
//                                priX_ACH = reader["PRIX_ACH"] != DBNull.Value ? Convert.ToDouble(reader["PRIX_ACH"]) : 0,
//                                stocK_MAX = reader["STOCK_MAX"] != DBNull.Value ? Convert.ToDouble(reader["STOCK_MAX"]) : 0,
//                                stocK_SEC = reader["STOCK_SEC"] != DBNull.Value ? Convert.ToDouble(reader["STOCK_SEC"]) : 0,
//                                valeuR_INI = reader["VALEUR_INI"] != DBNull.Value ? Convert.ToDouble(reader["VALEUR_INI"]) : 0,

//                                // حقول الإحصائيات (Tab 3)
//                                entrees = reader["ENTREES"] != DBNull.Value ? Convert.ToDouble(reader["ENTREES"]) : 0,
//                                sorties = reader["SORTIES"] != DBNull.Value ? Convert.ToDouble(reader["SORTIES"]) : 0,
//                                qtE_ENT = reader["QTE_ENT"] != DBNull.Value ? Convert.ToDouble(reader["QTE_ENT"]) : 0,
//                                qtE_SOR = reader["QTE_SOR"] != DBNull.Value ? Convert.ToDouble(reader["QTE_SOR"]) : 0,
//                                tempval = reader["TEMPVAL"] != DBNull.Value ? Convert.ToDouble(reader["TEMPVAL"]) : 0,
//                                achaT_HT = reader["ACHAT_HT"] != DBNull.Value ? Convert.ToDouble(reader["ACHAT_HT"]) : 0,
//                                consO_HT = reader["CONSO_HT"] != DBNull.Value ? Convert.ToDouble(reader["CONSO_HT"]) : 0,
//                                cesS_HT = reader["CESS_HT"] != DBNull.Value ? Convert.ToDouble(reader["CESS_HT"]) : 0,
//                                cessR_HT = reader["CESSR_HT"] != DBNull.Value ? Convert.ToDouble(reader["CESSR_HT"]) : 0
//                            });
//                        }
//                    }
//                }
//                catch (Exception ex)
//                {
//                    return new JsonResult(new { error = ex.Message });
//                }
//            }

//            // إرجاع البيانات بصيغة JSON للجدول
//            return new JsonResult(list);
//        }

//        // دالة التحديث (Update) يجب أيضاً أن تحدث الطرفين: DBF و SQLite
//        // أضف [FromForm] قبل كل بارامتر لضمان وصول القيم
//        public IActionResult OnPostUpdateArticle([FromForm] string articleRef, [FromForm] string intitule, [FromForm] string intitule2, [FromForm] string intitule3, [FromForm] string famille, [FromForm] string lieu_sto, [FromForm] string casier)
//        {
//            // أضف هذا الفحص في البداية لتجنب الانهيار مستقبلاً
//            if (string.IsNullOrEmpty(articleRef))
//            {
//                return new JsonResult(new { success = false, message = "Erreur: La référence de l'article est manquante." });
//            }

//            // الآن يمكنك استخدام .Trim() بأمان
//            string cleanRef = articleRef.Trim();


//            // 1. جلب المسارات
//            string dbfPath = _configService.GetDbPath();
//            string sqliteConnString = _sqliteService.GetSqliteConnectionString();

//            // تنظيف البيانات (تحويل للأحرف الكبيرة كما يفعل PCSTOCK)
//            intitule = intitule?.ToUpper() ?? "";
//            intitule2 = intitule2?.ToUpper() ?? "";
//            intitule3 = intitule3?.ToUpper() ?? "";
//            famille = famille?.ToUpper() ?? "";
//            lieu_sto = lieu_sto?.ToUpper() ?? "";
//            casier = casier?.ToUpper() ?? "";

//            try
//            {
//                // أولاً: التعديل في ملف الـ DBF الأصلي
//                string dbfConnStr = $@"Driver={{Microsoft Access dBASE Driver (*.dbf, *.ndx, *.mdx)}};Dbq={dbfPath};ReadOnly=0;";
//                using (System.Data.Odbc.OdbcConnection dbfConn = new System.Data.Odbc.OdbcConnection(dbfConnStr))
//                {
//                    dbfConn.Open();
//                    string sqlDbf = @"UPDATE ST_STOCK.DBF SET INTITULE=?, INTITULE2=?, INTITULE3=?, FAMILLE=?, LIEU_STO=?, CASIER=? WHERE TRIM(REF)=?";
//                    using (var cmd = new System.Data.Odbc.OdbcCommand(sqlDbf, dbfConn))
//                    {
//                        cmd.Parameters.AddWithValue("p1", intitule);
//                        cmd.Parameters.AddWithValue("p2", intitule2);
//                        cmd.Parameters.AddWithValue("p3", intitule3);
//                        cmd.Parameters.AddWithValue("p4", famille);
//                        cmd.Parameters.AddWithValue("p5", lieu_sto);
//                        cmd.Parameters.AddWithValue("p6", casier);
//                        cmd.Parameters.AddWithValue("p7", (articleRef ?? "").Trim());
//                        cmd.ExecuteNonQuery();
//                    }
//                }

//                // ثانياً: التعديل في قاعدة SQLite (المرآة) لكي يظهر التغيير فوراً في الويب
//                using (var sqliteConn = new SqliteConnection(sqliteConnString))
//                {
//                    sqliteConn.Open();
//                    string sqlSqlite = @"UPDATE ST_STOCK SET INTITULE=@i, INTITULE2=@i2, INTITULE3=@i3, FAMILLE=@f, LIEU_STO=@l, CASIER=@c WHERE REF=@r";
//                    using (var cmd = new SqliteCommand(sqlSqlite, sqliteConn))
//                    {
//                        cmd.Parameters.AddWithValue("@i", intitule);
//                        cmd.Parameters.AddWithValue("@i2", intitule2);
//                        cmd.Parameters.AddWithValue("@i3", intitule3);
//                        cmd.Parameters.AddWithValue("@f", famille);
//                        cmd.Parameters.AddWithValue("@l", lieu_sto);
//                        cmd.Parameters.AddWithValue("@c", casier);
//                        cmd.Parameters.AddWithValue("@r", articleRef);
//                        cmd.ExecuteNonQuery();
//                    }
//                }

//                return new JsonResult(new { success = true, message = "L'article a été mis à jour dans le fichier DBF et SQLite !" });
//            }
//            catch (Exception ex)
//            {
//                return new JsonResult(new { success = false, message = "Erreur lors de la mise à jour : " + ex.Message });
//            }
//        } 
//    }
//}