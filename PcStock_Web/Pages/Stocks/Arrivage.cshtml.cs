using DocumentFormat.OpenXml.ExtendedProperties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace PcStock_Web.Pages.Stocks
{
    public class ArrivageModel : PageModel
    {
        public AppSettings CompanySettings { get; set; }
        private readonly SqliteDbService _sqliteService;
        private readonly ConfigService _configService;
        public ArrivageModel(SqliteDbService sqliteService, ConfigService configService) 
        { 
            _sqliteService = sqliteService;
            _configService = configService; 
        }

        public void OnGet() 
        { 
            _sqliteService.EnsurePersistentTables(); 
            CompanySettings = _configService.GetAllSettings();
        }

        // 1. جلب البيانات للجدول
        public IActionResult OnGetLoadData()
        {
            var list = new List<object>();
            using (var conn = new SqliteConnection(_sqliteService.GetSqliteConnectionString()))
            {
                conn.Open();
                var cmd = new SqliteCommand("SELECT * FROM Arrivage_Journalier ORDER BY ID DESC", conn);
                using var r = cmd.ExecuteReader();
                int ord = 1;
                while (r.Read())
                {
                    list.Add(new
                    {
                        ord = ord++,
                        id = r["ID"],
                        dates = r["DATES"],
                        ref_art = r["REF"],
                        designation = r["DESIGNATION"],
                        machine = r["MACHINE"],
                        qte = r["QTE"],
                        prix = r["PRIX"],
                        fournisseur = r["FOURNISSEUR"],
                        fact_n = r["FACT_N"],
                        bc_n = r["BC_N"],
                        casier = r["CASIER"],
                        acheteur = r["ACHETEUR"]
                    });
                }
            }
            return new JsonResult(list);
        }

        // 2. البحث التلقائي عن السلع (Autocomplete)
        public IActionResult OnGetArticleAutocomplete(string term)
        {
            var results = new List<object>();
            using (var conn = new SqliteConnection(_sqliteService.GetSqliteConnectionString()))
            {
                conn.Open();
                var cmd = new SqliteCommand("SELECT REF, INTITULE, CASIER FROM ST_STOCK WHERE REF LIKE @t OR INTITULE LIKE @t LIMIT 15", conn);
                cmd.Parameters.AddWithValue("@t", "%" + term + "%");
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    results.Add(new
                    {
                        id = r["REF"].ToString().Trim(),
                        text = r["REF"].ToString().Trim() + " | " + r["INTITULE"].ToString().Trim(),
                        intitule = r["INTITULE"].ToString().Trim(),
                        casier = r["CASIER"]?.ToString().Trim() ?? ""
                    });
                }
            }
            return new JsonResult(new { results = results });
        }

        // 3. حفظ سطر جديد (POST)
        public IActionResult OnPostSave([FromForm] ArrivageEntry f)
        {
            try
            {
                if (string.IsNullOrEmpty(f.Ref)) return new JsonResult(new { success = false, message = "Référence obligatoire" });

                using var conn = new SqliteConnection(_sqliteService.GetSqliteConnectionString());
                conn.Open();
                string sql = @"INSERT INTO Arrivage_Journalier 
                    (DATES, REF, DESIGNATION, MACHINE, QTE, PRIX, FOURNISSEUR, FACT_N, BC_N, CASIER, ACHETEUR) 
                    VALUES (@d, @r, @des, @m, @q, @p, @four, @fact, @bc, @cas, @ach)";

                var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@d", f.Dates ?? "");
                cmd.Parameters.AddWithValue("@r", f.Ref.ToUpper());
                cmd.Parameters.AddWithValue("@des", f.Designation?.ToUpper() ?? "");
                cmd.Parameters.AddWithValue("@m", f.Machine?.ToUpper() ?? "");
                cmd.Parameters.AddWithValue("@q", f.Qte);
                cmd.Parameters.AddWithValue("@p", f.Prix);
                cmd.Parameters.AddWithValue("@four", f.Fournisseur?.ToUpper() ?? "");
                cmd.Parameters.AddWithValue("@fact", f.Fact_N?.ToUpper() ?? "");
                cmd.Parameters.AddWithValue("@bc", f.Bc_N?.ToUpper() ?? "");
                cmd.Parameters.AddWithValue("@cas", f.Casier?.ToUpper() ?? "");
                cmd.Parameters.AddWithValue("@ach", f.Acheteur?.ToUpper() ?? "");
                cmd.ExecuteNonQuery();
                return new JsonResult(new { success = true });
            }
            catch (Exception ex) { return new JsonResult(new { success = false, message = ex.Message }); }
        }

        // 4. تحديث سطر (POST)
        public IActionResult OnPostUpdate([FromForm] ArrivageEntry f, [FromForm] int Id)
        {
            try
            {
                using var conn = new SqliteConnection(_sqliteService.GetSqliteConnectionString());
                conn.Open();
                string sql = @"UPDATE Arrivage_Journalier SET 
                    DATES=@d, DESIGNATION=@des, MACHINE=@m, QTE=@q, PRIX=@p, FOURNISSEUR=@four, FACT_N=@fact, BC_N=@bc, ACHETEUR=@ach 
                    WHERE ID=@id";
                var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@d", f.Dates ?? "");
                cmd.Parameters.AddWithValue("@des", f.Designation?.ToUpper() ?? "");
                cmd.Parameters.AddWithValue("@m", f.Machine?.ToUpper() ?? "");
                cmd.Parameters.AddWithValue("@q", f.Qte);
                cmd.Parameters.AddWithValue("@p", f.Prix);
                cmd.Parameters.AddWithValue("@four", f.Fournisseur?.ToUpper() ?? "");
                cmd.Parameters.AddWithValue("@fact", f.Fact_N?.ToUpper() ?? "");
                cmd.Parameters.AddWithValue("@bc", f.Bc_N?.ToUpper() ?? "");
                cmd.Parameters.AddWithValue("@ach", f.Acheteur?.ToUpper() ?? "");
                cmd.Parameters.AddWithValue("@id", Id);
                cmd.ExecuteNonQuery();
                return new JsonResult(new { success = true });
            }
            catch (Exception ex) { return new JsonResult(new { success = false, message = ex.Message }); }
        }

        // 5. حذف سطر
        public IActionResult OnPostDelete(int id)
        {
            using var conn = new SqliteConnection(_sqliteService.GetSqliteConnectionString());
            conn.Open();
            new SqliteCommand($"DELETE FROM Arrivage_Journalier WHERE ID={id}", conn).ExecuteNonQuery();
            return new JsonResult(new { success = true });
        }
    }

    public class ArrivageEntry
    {
        public string? Dates { get; set; }
        public string? Ref { get; set; }
        public string? Designation { get; set; }
        public string? Machine { get; set; }
        public double Qte { get; set; }
        public double Prix { get; set; }
        public string? Fournisseur { get; set; }
        public string? Fact_N { get; set; }
        public string? Bc_N { get; set; }
        public string? Casier { get; set; }
        public string? Acheteur { get; set; }
    }
}