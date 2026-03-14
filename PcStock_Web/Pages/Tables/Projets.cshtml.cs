using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using System.Data.Odbc;

namespace PcStock_Web.Pages.Tables
{
    public class ProjetsModel : PageModel
    {
        private readonly SqliteDbService _sqliteService;
        private readonly ConfigService _configService;

        public ProjetsModel(SqliteDbService sqliteService, ConfigService configService)
        {
            _sqliteService = sqliteService;
            _configService = configService;
        }

        public void OnGet() { }

        // 1. جلب البيانات من SQLite
        public IActionResult OnGetLoadData()
        {
            var list = new List<object>();
            using (var conn = new SqliteConnection(_sqliteService.GetSqliteConnectionString()))
            {
                conn.Open();
                var cmd = new SqliteCommand("SELECT * FROM ST_UNITE ORDER BY NOM ASC", conn);
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new
                        {
                            cod_soc = r["COD_SOC"]?.ToString().Trim(),
                            nom = r["NOM"]?.ToString().Trim(),
                            adresse = r["ADRESSE"]?.ToString().Trim(),
                            ville = r["VILLE"]?.ToString().Trim()
                        });
                    }
                }
            }
            return new JsonResult(list);
        }

        // 2. الحفظ المزدوج (إضافة / تعديل)
        public IActionResult OnPostSave([FromForm] string cod_soc, [FromForm] string nom, [FromForm] string adresse, [FromForm] string ville, [FromForm] bool isEdit)
        {
            string dbfPath = _configService.GetDbPath();
            string sqliteConn = _sqliteService.GetSqliteConnectionString();

            try
            {
                // أ. التعديل في ملف DBF
                string dbfConnStr = $@"Driver={{Microsoft Access dBASE Driver (*.dbf, *.ndx, *.mdx)}};Dbq={dbfPath};ReadOnly=0;";
                using (var conn = new OdbcConnection(dbfConnStr))
                {
                    conn.Open();
                    string sql = isEdit
                        ? "UPDATE ST_UNITE.DBF SET NOM=?, ADRESSE=?, VILLE=? WHERE TRIM(COD_SOC)=?"
                        : "INSERT INTO ST_UNITE.DBF (COD_SOC, NOM, ADRESSE, VILLE) VALUES (?,?,?,?)";

                    using (var cmd = new OdbcCommand(sql, conn))
                    {
                        if (isEdit)
                        {
                            cmd.Parameters.AddWithValue("p1", nom?.ToUpper() ?? "");
                            cmd.Parameters.AddWithValue("p2", adresse?.ToUpper() ?? "");
                            cmd.Parameters.AddWithValue("p3", ville?.ToUpper() ?? "");
                            cmd.Parameters.AddWithValue("p4", cod_soc.Trim());
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("p1", cod_soc.ToUpper());
                            cmd.Parameters.AddWithValue("p2", nom?.ToUpper() ?? "");
                            cmd.Parameters.AddWithValue("p3", adresse?.ToUpper() ?? "");
                            cmd.Parameters.AddWithValue("p4", ville?.ToUpper() ?? "");
                        }
                        cmd.ExecuteNonQuery();
                    }
                }

                // ب. التحديث في SQLite
                using (var conn = new SqliteConnection(sqliteConn))
                {
                    conn.Open();
                    string sql = isEdit
                        ? "UPDATE ST_UNITE SET NOM=@n, ADRESSE=@a, VILLE=@v WHERE COD_SOC=@c"
                        : "INSERT INTO ST_UNITE (COD_SOC, NOM, ADRESSE, VILLE) VALUES (@c,@n,@a,@v)";

                    var cmd = new SqliteCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@c", cod_soc.ToUpper());
                    cmd.Parameters.AddWithValue("@n", nom?.ToUpper() ?? "");
                    cmd.Parameters.AddWithValue("@a", adresse?.ToUpper() ?? "");
                    cmd.Parameters.AddWithValue("@v", ville?.ToUpper() ?? "");
                    cmd.ExecuteNonQuery();
                }

                return new JsonResult(new { success = true, message = "Données enregistrées avec succès !" });
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
                    using var cmd = new OdbcCommand("DELETE FROM ST_UNITE.DBF WHERE TRIM(COD_SOC) = ?", conn);
                    cmd.Parameters.AddWithValue("p1", id.Trim());
                    cmd.ExecuteNonQuery();
                }

                using (var conn = new SqliteConnection(_sqliteService.GetSqliteConnectionString()))
                {
                    conn.Open();
                    using var cmd = new SqliteCommand("DELETE FROM ST_UNITE WHERE COD_SOC = @id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }

                return new JsonResult(new { success = true });
            }
            catch (Exception ex) { return new JsonResult(new { success = false, message = ex.Message }); }
        }
    }
}