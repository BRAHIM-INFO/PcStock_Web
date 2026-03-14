using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using System.Data.Odbc;

namespace PcStock_Web.Pages.Tables
{
    public class FournisseursModel : PageModel
    {
        private readonly SqliteDbService _sqliteService;
        private readonly ConfigService _configService;

        public FournisseursModel(SqliteDbService sqliteService, ConfigService configService)
        {
            _sqliteService = sqliteService;
            _configService = configService;
        }

        public void OnGet() { }

        // 1. جلب البيانات للجدول
        public IActionResult OnGetLoadData()
        {
            var list = new List<object>();
            using (var conn = new SqliteConnection(_sqliteService.GetSqliteConnectionString()))
            {
                conn.Open();
                var cmd = new SqliteCommand("SELECT * FROM ST_FOURN ORDER BY NOM ASC", conn);
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new
                        {
                            cod_soc = r["COD_SOC"]?.ToString().Trim(),
                            nom = r["NOM"]?.ToString().Trim(),
                            adresse = r["ADRESSE"]?.ToString().Trim(),
                            ville = r["VILLE"]?.ToString().Trim(),
                            telephone = r["TELEPHONE"]?.ToString().Trim(),
                            email = r["EMAIL"]?.ToString().Trim()
                        });
                    }
                }
            }
            return new JsonResult(list);
        }

        // 2. إضافة أو تعديل مورد (حفظ مزدوج)
        public IActionResult OnPostSave([FromForm] string cod_soc, [FromForm] string nom, [FromForm] string adresse, [FromForm] string ville, [FromForm] string telephone, [FromForm] string email, [FromForm] bool isEdit)
        {
            string dbfPath = _configService.GetDbPath();
            string sqliteConn = _sqliteService.GetSqliteConnectionString();

            // القيم الافتراضية التي طلبتها
            string faxValue = "(  )-  -  -";
            double zeroValue = 0;
            string familyValue = "1"; // القيمة المطلوبة لحقل FAMILLE

            try
            {
                // 1. التحديث في ملف DBF الأصلي
                string dbfConnStr = $@"Driver={{Microsoft Access dBASE Driver (*.dbf, *.ndx, *.mdx)}};Dbq={dbfPath};ReadOnly=0;";
                using (var conn = new OdbcConnection(dbfConnStr))
                {
                    conn.Open();
                    string sql;
                    if (isEdit)
                    {
                        // جملة UPDATE تشمل الحقول الإضافية لضمان سلامة البيانات
                        sql = @"UPDATE ST_FOURN.DBF SET NOM=?, ADRESSE=?, VILLE=?, TELEPHONE=?, EMAIL=?, 
                        TELEFAX=?, FAMILLE=?, OBJECTIF=?, REMISE=?, CA_HT=?, CA_TTC=?, DEBIT=?, CREDIT=?, 
                        DEB_INI=?, CRD_INI=?, AVANCES=? WHERE TRIM(COD_SOC)=?";
                    }
                    else
                    {
                        // جملة INSERT مع كل الحقول (16 حقل)
                        sql = @"INSERT INTO ST_FOURN.DBF (COD_SOC, NOM, ADRESSE, VILLE, TELEPHONE, EMAIL, 
                        TELEFAX, FAMILLE,OBJECTIF, REMISE, CA_HT, CA_TTC, DEBIT, CREDIT, DEB_INI, CRD_INI, AVANCES) 
                        VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)";
                    }

                    using (var cmd = new OdbcCommand(sql, conn))
                    {
                        if (isEdit)
                        {
                            cmd.Parameters.AddWithValue("p1", nom?.ToUpper() ?? "");
                            cmd.Parameters.AddWithValue("p2", adresse?.ToUpper() ?? "");
                            cmd.Parameters.AddWithValue("p3", ville?.ToUpper() ?? "");
                            cmd.Parameters.AddWithValue("p4", telephone ?? "");
                            cmd.Parameters.AddWithValue("p5", email?.ToLower() ?? "");
                            cmd.Parameters.AddWithValue("p6", faxValue);
                            cmd.Parameters.AddWithValue("p7", familyValue); // حقل العائلة = 1
                            cmd.Parameters.AddWithValue("p8", zeroValue); // OBJECTIF
                            cmd.Parameters.AddWithValue("p9", zeroValue); // REMISE
                            cmd.Parameters.AddWithValue("p10", zeroValue); // CA_HT
                            cmd.Parameters.AddWithValue("p11", zeroValue); // CA_TTC
                            cmd.Parameters.AddWithValue("p12", zeroValue); // DEBIT
                            cmd.Parameters.AddWithValue("p13", zeroValue); // CREDIT
                            cmd.Parameters.AddWithValue("p14", zeroValue); // DEB_INI
                            cmd.Parameters.AddWithValue("p15", zeroValue); // CRD_INI
                            cmd.Parameters.AddWithValue("p16", zeroValue); // AVANCES
                            cmd.Parameters.AddWithValue("p17", cod_soc.Trim()); // الـ ID في شرط WHERE
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("p1", cod_soc.ToUpper());
                            cmd.Parameters.AddWithValue("p2", nom?.ToUpper() ?? "");
                            cmd.Parameters.AddWithValue("p3", adresse?.ToUpper() ?? "");
                            cmd.Parameters.AddWithValue("p4", ville?.ToUpper() ?? "");
                            cmd.Parameters.AddWithValue("p5", telephone ?? "");
                            cmd.Parameters.AddWithValue("p6", email?.ToLower() ?? "");
                            cmd.Parameters.AddWithValue("p7", faxValue);
                            cmd.Parameters.AddWithValue("p8", familyValue); // حقل العائلة = 1
                            cmd.Parameters.AddWithValue("p9", zeroValue); // OBJECTIF
                            cmd.Parameters.AddWithValue("p10", zeroValue); // REMISE
                            cmd.Parameters.AddWithValue("p11", zeroValue); // CA_HT
                            cmd.Parameters.AddWithValue("p12", zeroValue); // CA_TTC
                            cmd.Parameters.AddWithValue("p13", zeroValue); // DEBIT
                            cmd.Parameters.AddWithValue("p14", zeroValue); // CREDIT
                            cmd.Parameters.AddWithValue("p15", zeroValue); // DEB_INI
                            cmd.Parameters.AddWithValue("p16", zeroValue); // CRD_INI
                            cmd.Parameters.AddWithValue("p17", zeroValue); // AVANCES
                        }
                        cmd.ExecuteNonQuery();
                    }
                }

                // 2. التحديث في قاعدة بيانات SQLite (المرآة)
                // 2. التحديث في قاعدة بيانات SQLite (المرآة)
                using (var conn = new SqliteConnection(sqliteConn))
                {
                    conn.Open();
                    string sql;
                    if (isEdit)
                    {
                        sql = "UPDATE ST_FOURN SET NOM=@n, ADRESSE=@a, VILLE=@v, TELEPHONE=@t, EMAIL=@e, TELEFAX=@f, FAMILLE=@g  WHERE COD_SOC=@c";
                    }
                    else
                    {
                        // التعديل هنا: تم تغيير ST_STOCK إلى ST_FOURN
                        sql = "INSERT INTO ST_FOURN (COD_SOC, NOM, ADRESSE, VILLE, TELEPHONE, EMAIL, TELEFAX, FAMILLE) VALUES (@c,@n,@a,@v,@t,@e,@f,@g)";
                    }

                    var cmd = new SqliteCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@c", cod_soc.ToUpper());
                    cmd.Parameters.AddWithValue("@n", nom?.ToUpper() ?? "");
                    cmd.Parameters.AddWithValue("@a", adresse?.ToUpper() ?? "");
                    cmd.Parameters.AddWithValue("@v", ville?.ToUpper() ?? "");
                    cmd.Parameters.AddWithValue("@t", telephone ?? "");
                    cmd.Parameters.AddWithValue("@e", email?.ToLower() ?? "");
                    cmd.Parameters.AddWithValue("@f", faxValue);
                    cmd.Parameters.AddWithValue("@g", familyValue);
                    cmd.ExecuteNonQuery();
                } 

                return new JsonResult(new { success = true, message = "Données enregistrées avec succès !" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Erreur DBF: " + ex.Message });
            }
        } 
        public IActionResult OnPostDelete(string id)
        {
            if (string.IsNullOrEmpty(id)) return new JsonResult(new { success = false, message = "ID invalide." });

            string dbfPath = _configService.GetDbPath();
            string sqliteConnString = _sqliteService.GetSqliteConnectionString();

            try
            {
                // 1. الحذف من ملف DBF (الأصلي)
                string dbfConnStr = $@"Driver={{Microsoft Access dBASE Driver (*.dbf, *.ndx, *.mdx)}};Dbq={dbfPath};ReadOnly=0;";
                using (var conn = new System.Data.Odbc.OdbcConnection(dbfConnStr))
                {
                    conn.Open();
                    // ملاحظة: في ملفات DBF يفضل استخدام TRIM لضمان مطابقة الـ ID
                    using (var cmd = new System.Data.Odbc.OdbcCommand("DELETE FROM ST_FOURN.DBF WHERE TRIM(COD_SOC) = ?", conn))
                    {
                        cmd.Parameters.AddWithValue("p1", id.Trim());
                        cmd.ExecuteNonQuery();
                    }
                }

                // 2. الحذف من قاعدة بيانات SQLite (المرآة)
                using (var conn = new SqliteConnection(sqliteConnString))
                {
                    conn.Open();
                    using (var cmd = new SqliteCommand("DELETE FROM ST_FOURN WHERE COD_SOC = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Erreur : " + ex.Message });
            }
        }
    }
}