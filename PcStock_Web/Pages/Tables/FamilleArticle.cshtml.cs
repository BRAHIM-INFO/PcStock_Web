using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace PcStock_Web.Pages.Tables
{
    public class FamilleArticleModel : PageModel
    {
        private readonly SqliteDbService _sqliteService;
        private readonly ConfigService _configService;
        public FamilleArticleModel(SqliteDbService s, ConfigService c) { _sqliteService = s; _configService = c; }

        public IActionResult OnGetLoadData()
        {
            var list = new List<object>();
            using var conn = new SqliteConnection(_sqliteService.GetSqliteConnectionString());
            conn.Open();
            // ربط العائلة بالمجموعة لجلب اسم المجموعة
            string sql = "SELECT F.*, G.NOM AS GRP_NOM FROM ST_FAMI F LEFT JOIN ST_FAGRP G ON F.COD_GRP = G.COD_GRP ORDER BY F.NOM ASC";
            using var r = new SqliteCommand(sql, conn).ExecuteReader();
            while (r.Read()) list.Add(new { cod_soc = r["COD_SOC"].ToString().Trim(), nom = r["NOM"].ToString().Trim(), cod_grp = r["COD_GRP"].ToString().Trim(), grp_nom = r["GRP_NOM"].ToString() });
            return new JsonResult(list);
        }

        public IActionResult OnGetGroups()
        { // لجلب القائمة المنسدلة
            var list = new List<object>();
            using var conn = new SqliteConnection(_sqliteService.GetSqliteConnectionString());
            conn.Open();
            using var r = new SqliteCommand("SELECT COD_GRP, NOM FROM ST_FAGRP", conn).ExecuteReader();
            while (r.Read()) list.Add(new { id = r["COD_GRP"], text = r["NOM"] });
            return new JsonResult(list);
        }

        public IActionResult OnPostSave([FromForm] string cod_soc, [FromForm] string nom, [FromForm] string cod_grp, [FromForm] bool isEdit)
        {
            // نفس منطق الحفظ المزدوج (DBF & SQLite) المطبق في الدروس السابقة
            // تأكد من استخدام [COD_SOC], [NOM], [COD_GRP] في SQL
            return new JsonResult(new { success = true });
        }
    }
}
