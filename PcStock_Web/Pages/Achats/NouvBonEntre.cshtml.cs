using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml.Office;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using System;
using System.ComponentModel;
using System.Data.Odbc;
using System.Globalization;
using System.Xml.Linq;

namespace PcStock_Web.Pages.Achats
{
    public class NouvBonEntreModel : PageModel
    {
        private readonly ConfigService _configService;
        private readonly SqliteDbService _sqliteService;

        public NouvBonEntreModel(ConfigService configService, SqliteDbService sqliteService)
        {
            _configService = configService;
            _sqliteService = sqliteService;
        }

        public void OnGet() { }

        // 1. بحث الموردين (ST_FOURN)
        public IActionResult OnGetFournisseurAutocomplete(string term)
        {
            var results = new List<object>();
            using (var conn = new SqliteConnection(_sqliteService.GetSqliteConnectionString()))
            {
                conn.Open();
                string sql = "SELECT COD_SOC, NOM FROM ST_FOURN WHERE NOM LIKE @t OR COD_SOC LIKE @t LIMIT 15";
                var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@t", "%" + term + "%");

                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        results.Add(new
                        {
                            id = r["COD_SOC"].ToString().Trim(),
                            text = r["COD_SOC"].ToString().Trim() + " | " + r["NOM"].ToString().Trim()
                        });
                    }
                }
            }
            return new JsonResult(results);
        }

        // 2. بحث المنتجات (ST_STOCK) مع جلب كل التفاصيل
        //public IActionResult OnGetArticleAutocomplete(string term)
        //{
        //    var results = new List<object>();
        //    using (var conn = new SqliteConnection(_sqliteService.GetSqliteConnectionString()))
        //    {
        //        conn.Open();
        //        string sql = @"SELECT REF, INTITULE, INTITULE2, FAMILLE, PAMP 
        //                       FROM ST_STOCK 
        //                       WHERE REF LIKE @t OR INTITULE LIKE @t 
        //                       LIMIT 15";
        //        var cmd = new SqliteCommand(sql, conn);
        //        cmd.Parameters.AddWithValue("@t", "%" + term + "%");

        //        using (var r = cmd.ExecuteReader())
        //        {
        //            while (r.Read())
        //            {
        //                results.Add(new
        //                {
        //                    id = r["REF"].ToString().Trim(),
        //                    text = r["REF"].ToString().Trim() + " | " + r["INTITULE"].ToString().Trim(),
        //                    intitule = r["INTITULE"].ToString().Trim(),
        //                    intitule2 = r["INTITULE2"]?.ToString().Trim() ?? "",
        //                    famille = r["FAMILLE"]?.ToString().Trim() ?? "",
        //                    pamp = r["PAMP"] != DBNull.Value ? Convert.ToDouble(r["PAMP"]) : 0
        //                });
        //            }
        //        }
        //    }
        //    return new JsonResult(new { results = results });
        //}

        // محرك البحث عن المنتجات في SQLite
        public IActionResult OnGetArticleAutocomplete(string term)
        {
            var results = new List<object>();
            using (var conn = new Microsoft.Data.Sqlite.SqliteConnection(_sqliteService.GetSqliteConnectionString()))
            {
                conn.Open();
                // البحث في المرجع أو التسمية
                string sql = @"SELECT REF, INTITULE, PAMP FROM ST_STOCK 
                       WHERE REF LIKE @t OR INTITULE LIKE @t LIMIT 20";
                var cmd = new Microsoft.Data.Sqlite.SqliteCommand(sql, conn);
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
                            pamp = r["PAMP"] != DBNull.Value ? Convert.ToDouble(r["PAMP"]) : 0,
                            exists = true // علامة تدل على أن المنتج موجود فعلاً
                        });
                    }
                }
            }
            return new JsonResult(results);
        }

        // المحرك الرئيسي لحفظ الـ BEM بالكامل
        public async Task<IActionResult> OnPostValiderBEM([FromBody] BemPostData data)
        { 
            if (data == null || data.Items.Count == 0)
                return new JsonResult(new { success = false, message = "Les données sont nulles (Binding Error)" });

            string dbfPath = _configService.GetDbPath();
            string connStr = $@"Driver={{Microsoft Access dBASE Driver (*.dbf, *.ndx, *.mdx)}};Dbq={dbfPath};ReadOnly=0;";

            try
            {
                OdbcConnection.ReleaseObjectPool();
                using var conn = new OdbcConnection(connStr);
                conn.Open();

                foreach (var item in data.Items)
                {
                    // 1. التحقق هل المنتج موجود في ST_STOCK
                    bool exists = CheckProductExists(conn, item.Ref);

                    if (!exists)
                    {
                        // 2. إذا كان جديداً: إدراجه في ST_STOCK مع الحقول الافتراضية الكثيرة
                        InsertNewProductInStock(conn, item);
                    }
                    else
                    {
                        // 3. إذا كان موجوداً: تحديث الكمية والسعر (PAMP) والمبالغ
                        UpdateExistingProductInStock(conn, item);
                    }

                    // 4. إدراج الحركة في ملف المشتريات ST_ACHAT
                    InsertAchatRecord(conn, item, data);
                }

                // 5. المزامنة مع SQLite لكي يظهر المنتج الجديد فوراً في الويب
                await _sqliteService.SyncTables(new List<string> { "ST_STOCK", "ST_ACHAT" });

                return new JsonResult(new { success = true, message = "Bon validé et stocks mis à jour !" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Erreur DBF: " + ex.Message });
            }
        }

        private bool CheckProductExists(OdbcConnection conn, string reference)
        {
            using var cmd = new OdbcCommand("SELECT COUNT(*) FROM ST_STOCK.DBF WHERE TRIM(REF) = ?", conn);
            cmd.Parameters.AddWithValue("p1", reference.Trim());
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        private void InsertNewProductInStock(OdbcConnection conn, BemItem item)
        {
            // نضع [ ] حول كل الحقول لتجنب أخطاء الكلمات المحجوزة مثل DATE و TYPE
            string sql = @"INSERT INTO ST_STOCK ([REF], [CODE_INT], [COMPOSE], [OBTENU], [INTITULE], [INTITULE2], [INTITULE3], [INTITULE4], [FAMILLE], [QTE], [UNITE], [PRIX_ACH], [PAMP], [DATE_MAJ], [DATE_COM], [CODE_TVA], [EXO_TAIC], [CODE_TAIC], [ENTREES], [ACHAT_HT], [ACHAT_TTC], [QTE_ENT], [TYPE], [TYPE_ART], [TEMPVAL], [POIDS], [UPDATED]) 
                   VALUES (?, ?, 'N', '', ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, '1', 'N', '1', ?, ?, ?, ?, 'P', 'S', ?, 1, 'FAUX')";

            using var cmd = new OdbcCommand(sql, conn);
            double montant = item.Qte * item.Prix;
            string nowStr = DateTime.Now.ToString("yyyy-MM-dd"); // تنسيق التاريخ لـ DBF

            cmd.Parameters.AddWithValue("p1", item.Ref.ToUpper());
            cmd.Parameters.AddWithValue("p2", item.Ref.ToUpper());
            cmd.Parameters.AddWithValue("p3", item.Intitule.ToUpper());
            cmd.Parameters.AddWithValue("p4", (item.Intitule2 ?? "").ToUpper());
            cmd.Parameters.AddWithValue("p5", "");
            cmd.Parameters.AddWithValue("p6", "");
            cmd.Parameters.AddWithValue("p7", (item.Famille ?? "00").ToUpper());
            cmd.Parameters.AddWithValue("p8", 0); // QTE تبدأ بـ 0 كما طلبت
            cmd.Parameters.AddWithValue("p9", "U");
            cmd.Parameters.AddWithValue("p10", item.Prix);
            cmd.Parameters.AddWithValue("p11", item.Prix);
            cmd.Parameters.AddWithValue("p12", nowStr);
            cmd.Parameters.AddWithValue("p13", nowStr);
            cmd.Parameters.AddWithValue("p14", montant);
            cmd.Parameters.AddWithValue("p15", montant);
            cmd.Parameters.AddWithValue("p16", montant);
            cmd.Parameters.AddWithValue("p17", item.Qte);
            cmd.Parameters.AddWithValue("p18", montant);

            cmd.ExecuteNonQuery();
        }

        private void UpdateExistingProductInStock(OdbcConnection conn, BemItem item)
        {
            // تحديث السجل الحالي (زيادة الكمية وحساب PAMP جديد)
            // نستخدم استعلامين: واحد لجلب القديم والثاني للتحديث
            double oldQte = 0, oldPamp = 0, oldEntrees = 0, oldAchatHt = 0;
            using (var cmdGet = new OdbcCommand("SELECT QTE, PAMP, ENTREES, ACHAT_HT FROM ST_STOCK.DBF WHERE TRIM(REF) = ?", conn))
            {
                cmdGet.Parameters.AddWithValue("p1", item.Ref.Trim());
                using var r = cmdGet.ExecuteReader();
                if (r.Read())
                {
                    oldQte = Convert.ToDouble(r["QTE"]);
                    oldPamp = Convert.ToDouble(r["PAMP"]);
                    oldEntrees = Convert.ToDouble(r["ENTREES"]);
                    oldAchatHt = Convert.ToDouble(r["ACHAT_HT"]);
                }
            }

            double newQte = oldQte + item.Qte;
            // حساب المتوسط المرجح PAMP
            double newPamp = ((oldQte * oldPamp) + (item.Qte * item.Prix)) / (newQte > 0 ? newQte : 1);
            double newMontant = item.Qte * item.Prix;

            string sql = @"UPDATE ST_STOCK.DBF 
                           SET QTE = ?, PAMP = ?, DATE_MAJ = ?, ENTREES = ?, ACHAT_HT = ?, QTE_ENT = QTE_ENT + ?, TEMPVAL = ? 
                           WHERE TRIM(REF) = ?";

            using var cmdUpd = new OdbcCommand(sql, conn);
            cmdUpd.Parameters.AddWithValue("p1", newQte);
            cmdUpd.Parameters.AddWithValue("p2", Math.Round(newPamp, 2));
            cmdUpd.Parameters.AddWithValue("p3", DateTime.Now.ToString("yyyy-MM-dd"));
            cmdUpd.Parameters.AddWithValue("p4", oldEntrees + newMontant);
            cmdUpd.Parameters.AddWithValue("p5", oldAchatHt + newMontant);
            cmdUpd.Parameters.AddWithValue("p6", item.Qte);
            cmdUpd.Parameters.AddWithValue("p7", newQte * newPamp);
            cmdUpd.Parameters.AddWithValue("p8", item.Ref.Trim());
            cmdUpd.ExecuteNonQuery();
        }

        private void InsertAchatRecord(OdbcConnection conn, BemItem item, BemPostData data)
        {
            // وضع [DATE] ضروري جداً هنا
            string sql = @"INSERT INTO ST_ACHAT ([REF], [DATE], [COD_SOC], [QTE], [PRIX], [MONTANT], [MONTANT_HT], [CODE_TVA], [PAMP], [NO_BR], [NO_FACA], [NO_BC], [NO_BL], [PASSE], [RETOUR], [ENSTOCK], [VALSTOCK]) 
                   VALUES (?, ?, ?, ?, ?, ?, ?, '1', ?, ?, ?, ?, ?, 'N', 'N', ?, ?)";

            using var cmd = new OdbcCommand(sql, conn);
            double montant = item.Qte * item.Prix;

            cmd.Parameters.AddWithValue("p1", item.Ref.ToUpper());
            cmd.Parameters.AddWithValue("p2", data.Date); // تأكد أن التنسيق YYYY-MM-DD
            cmd.Parameters.AddWithValue("p3", data.FournisseurId);
            cmd.Parameters.AddWithValue("p4", item.Qte);
            cmd.Parameters.AddWithValue("p5", item.Prix);
            cmd.Parameters.AddWithValue("p6", montant);
            cmd.Parameters.AddWithValue("p7", montant);
            cmd.Parameters.AddWithValue("p8", item.Prix);
            cmd.Parameters.AddWithValue("p9", data.NoBem);
            cmd.Parameters.AddWithValue("p10", data.FactureNo);
            cmd.Parameters.AddWithValue("p11", data.BcNo);
            cmd.Parameters.AddWithValue("p12", "");
            cmd.Parameters.AddWithValue("p13", item.Qte);
            cmd.Parameters.AddWithValue("p14", montant);

            cmd.ExecuteNonQuery();
        }
    }

    // كلاسات استقبال البيانات من JSON
    public class BemPostData
    {
        public string NoBem { get; set; }
        public string Date { get; set; }
        public string FournisseurId { get; set; }
        public string FactureNo { get; set; }
        public string BcNo { get; set; }
        public List<BemItem> Items { get; set; }
    }

    public class BemItem
    {
        public string Ref { get; set; }
        public string Intitule { get; set; }
        public string Intitule2 { get; set; }
        public string Famille { get; set; }
        public double Qte { get; set; }
        public double Prix { get; set; }
    }


}