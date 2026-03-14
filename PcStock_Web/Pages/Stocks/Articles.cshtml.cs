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

        // دالة لجلب القيم الفريدة لملء قائمة الفلتر (Checkboxes)
        public IActionResult OnGetUniqueValues(string columnName)
        {
            var values = new List<string>();
            using (var conn = new SqliteConnection(_sqliteService.GetSqliteConnectionString()))
            {
                conn.Open();
                // جلب القيم غير المكررة للعمود المطلوب
                string sql = $"SELECT DISTINCT [{columnName}] FROM ST_STOCK WHERE [{columnName}] IS NOT NULL AND [{columnName}] != '' ORDER BY [{columnName}] ASC";
                var cmd = new SqliteCommand(sql, conn);
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read()) values.Add(r[0].ToString().Trim());
                }
            }
            return new JsonResult(values);
        }

        // تعديل بسيط في OnPostLoadData لدعم البحث المتعدد القادم من الفلتر
        // في استعلام الـ SQL، بدلاً من LIKE، استخدمنا REGEXP أو قمنا بمعالجة النص
        // ملاحظة: SQLite لا تدعم REGEXP افتراضياً، لذا سنحول البحث المتعدد (Val1|Val2) إلى شرط IN

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