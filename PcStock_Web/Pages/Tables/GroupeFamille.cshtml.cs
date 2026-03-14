using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using System.Data.Odbc;

namespace PcStock_Web.Pages.Tables
{
    public class GroupeFamilleModel : PageModel
    {
        private readonly SqliteDbService _sqliteService;
        private readonly ConfigService _configService;

        public GroupeFamilleModel(SqliteDbService sqliteService, ConfigService configService)
        {
            _sqliteService = sqliteService;
            _configService = configService;
        }

        public void OnGet() { }

        // 1. جلب البيانات لجدول العرض
        public IActionResult OnGetLoadData()
        {
            var list = new List<object>();
            using (var conn = new SqliteConnection(_sqliteService.GetSqliteConnectionString()))
            {
                conn.Open();
                var cmd = new SqliteCommand("SELECT * FROM ST_FAGRP ORDER BY NOM ASC", conn);
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new
                        {
                            cod_grp = r["COD_GRP"]?.ToString().Trim(),
                            nom = r["NOM"]?.ToString().Trim()
                        });
                    }
                }
            }
            return new JsonResult(list);
        }

        // 2. دالة الحفظ المزدوج (إضافة / تعديل)
        public IActionResult OnPostSave([FromForm] string cod_grp, [FromForm] string nom, [FromForm] bool isEdit)
        {
            string dbfPath = _configService.GetDbPath();
            string sqliteConn = _sqliteService.GetSqliteConnectionString();

            try
            {
                // أ. التحديث في ملف DBF الأصلي
                string dbfConnStr = $@"Driver={{Microsoft Access dBASE Driver (*.dbf, *.ndx, *.mdx)}};Dbq={dbfPath};ReadOnly=0;";
                using (var conn = new OdbcConnection(dbfConnStr))
                {
                    conn.Open();
                    string sql = isEdit
                        ? "UPDATE ST_FAGRP.DBF SET NOM=? WHERE TRIM(COD_GRP)=?"
                        : "INSERT INTO ST_FAGRP.DBF (COD_GRP, NOM) VALUES (?,?)";

                    using (var cmd = new OdbcCommand(sql, conn))
                    {
                        if (isEdit)
                        {
                            cmd.Parameters.AddWithValue("p1", nom?.ToUpper() ?? "");
                            cmd.Parameters.AddWithValue("p2", cod_grp.Trim());
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("p1", cod_grp.ToUpper());
                            cmd.Parameters.AddWithValue("p2", nom?.ToUpper() ?? "");
                        }
                        cmd.ExecuteNonQuery();
                    }
                }

                // ب. التحديث في قاعدة بيانات SQLite
                using (var conn = new SqliteConnection(sqliteConn))
                {
                    conn.Open();
                    string sql = isEdit
                        ? "UPDATE ST_FAGRP SET NOM=@n WHERE COD_GRP=@c"
                        : "INSERT INTO ST_FAGRP (COD_GRP, NOM) VALUES (@c,@n)";

                    var cmd = new SqliteCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@c", cod_grp.ToUpper());
                    cmd.Parameters.AddWithValue("@n", nom?.ToUpper() ?? "");
                    cmd.ExecuteNonQuery();
                }

                return new JsonResult(new { success = true, message = "Groupe enregistré avec succès !" });
            }
            catch (Exception ex) { return new JsonResult(new { success = false, message = ex.Message }); }
        }

        // 3. دالة الحذف المتسلسل (حذف المجموعة وكل العائلات المرتبطة بها)
        public IActionResult OnPostDelete(string id)
        {
            if (string.IsNullOrEmpty(id)) return new JsonResult(new { success = false });

            string dbfPath = _configService.GetDbPath();
            string sqliteConn = _sqliteService.GetSqliteConnectionString();

            try
            {
                // أ. الحذف من ملفات DBF (يجب حذف الأبناء من ST_FAMI أولاً ثم الأب من ST_FAGRP)
                string dbfConnStr = $@"Driver={{Microsoft Access dBASE Driver (*.dbf, *.ndx, *.mdx)}};Dbq={dbfPath};ReadOnly=0;";
                using (var conn = new OdbcConnection(dbfConnStr))
                {
                    conn.Open();
                    // حذف العائلات التابعة
                    using (var cmd1 = new OdbcCommand("DELETE FROM ST_FAMI.DBF WHERE TRIM(COD_GRP) = ?", conn))
                    {
                        cmd1.Parameters.AddWithValue("p1", id.Trim());
                        cmd1.ExecuteNonQuery();
                    }
                    // حذف المجموعة نفسها
                    using (var cmd2 = new OdbcCommand("DELETE FROM ST_FAGRP.DBF WHERE TRIM(COD_GRP) = ?", conn))
                    {
                        cmd2.Parameters.AddWithValue("p1", id.Trim());
                        cmd2.ExecuteNonQuery();
                    }
                }

                // ب. الحذف من SQLite
                using (var conn = new SqliteConnection(sqliteConn))
                {
                    conn.Open();
                    // حذف العائلات التابعة
                    using (var cmd1 = new SqliteCommand("DELETE FROM ST_FAMI WHERE COD_GRP = @id", conn))
                    {
                        cmd1.Parameters.AddWithValue("@id", id);
                        cmd1.ExecuteNonQuery();
                    }
                    // حذف المجموعة
                    using (var cmd2 = new SqliteCommand("DELETE FROM ST_FAGRP WHERE COD_GRP = @id", conn))
                    {
                        cmd2.Parameters.AddWithValue("@id", id);
                        cmd2.ExecuteNonQuery();
                    }
                }

                return new JsonResult(new { success = true });
            }
            catch (Exception ex) { return new JsonResult(new { success = false, message = ex.Message }); }
        }
    }
}