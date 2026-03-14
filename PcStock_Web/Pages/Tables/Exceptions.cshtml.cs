using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using System.Data.Odbc;

namespace PcStock_Web.Pages.Tables
{
    public class ExceptionsModel : PageModel
    {
        private readonly SqliteDbService _sqliteService;
        private readonly ConfigService _configService;

        public ExceptionsModel(SqliteDbService sqliteService, ConfigService configService)
        {
            _sqliteService = sqliteService;
            _configService = configService;
        }

        public void OnGet() { }

        // 1. جلب البيانات من SQLite (سرعة البرق)
        public IActionResult OnGetLoadData()
        {
            var list = new List<object>();
            using (var conn = new SqliteConnection(_sqliteService.GetSqliteConnectionString()))
            {
                conn.Open();
                var cmd = new SqliteCommand("SELECT * FROM ST_SORTI ORDER BY COD_SOC ASC", conn);
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new
                        {
                            cod_soc = r["COD_SOC"]?.ToString().Trim(),
                            nom = r["NOM"]?.ToString().Trim()
                        });
                    }
                }
            }
            return new JsonResult(list);
        }

        // 2. الحفظ المزدوج (DBF & SQLite)
        public IActionResult OnPostSave([FromForm] string cod_soc, [FromForm] string nom, [FromForm] bool isEdit)
        {
            if (string.IsNullOrEmpty(cod_soc)) return new JsonResult(new { success = false, message = "Code obligatoire." });

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
                        ? "UPDATE ST_SORTI.DBF SET NOM=? WHERE TRIM(COD_SOC)=?"
                        : "INSERT INTO ST_SORTI.DBF (COD_SOC, NOM) VALUES (?,?)";

                    using (var cmd = new OdbcCommand(sql, conn))
                    {
                        if (isEdit)
                        {
                            cmd.Parameters.AddWithValue("p1", nom?.ToUpper() ?? "");
                            cmd.Parameters.AddWithValue("p2", cod_soc.Trim());
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("p1", cod_soc.ToUpper());
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
                        ? "UPDATE ST_SORTI SET NOM=@n WHERE COD_SOC=@c"
                        : "INSERT INTO ST_SORTI (COD_SOC, NOM) VALUES (@c,@n)";

                    var cmd = new SqliteCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@c", cod_soc.ToUpper());
                    cmd.Parameters.AddWithValue("@n", nom?.ToUpper() ?? "");
                    cmd.ExecuteNonQuery();
                }

                return new JsonResult(new { success = true, message = "Exception enregistrée avec succès !" });
            }
            catch (Exception ex) { return new JsonResult(new { success = false, message = ex.Message }); }
        }

        // 3. الحذف المزدوج
        public IActionResult OnPostDelete(string id)
        {
            try
            {
                string dbfPath = _configService.GetDbPath();
                string dbfConnStr = $@"Driver={{Microsoft Access dBASE Driver (*.dbf, *.ndx, *.mdx)}};Dbq={dbfPath};ReadOnly=0;";

                using (var conn = new OdbcConnection(dbfConnStr))
                {
                    conn.Open();
                    using var cmd = new OdbcCommand("DELETE FROM ST_SORTI.DBF WHERE TRIM(COD_SOC) = ?", conn);
                    cmd.Parameters.AddWithValue("p1", id.Trim());
                    cmd.ExecuteNonQuery();
                }

                using (var conn = new SqliteConnection(_sqliteService.GetSqliteConnectionString()))
                {
                    conn.Open();
                    using var cmd = new SqliteCommand("DELETE FROM ST_SORTI WHERE COD_SOC = @id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
                return new JsonResult(new { success = true });
            }
            catch (Exception ex) { return new JsonResult(new { success = false, message = ex.Message }); }
        }
    }
}