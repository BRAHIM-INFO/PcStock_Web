using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace PcStock_Web.Pages.Repports
{
    public class RepportMensuelModel : PageModel
    {
        private readonly SqliteDbService _sqliteService;
        public RepportMensuelModel(SqliteDbService sqliteService) { _sqliteService = sqliteService; }

        // خصائص لتخزين البيانات وعرضها في الصفحة
        public double TotalAchat { get; set; }
        public double TotalCessionFournie { get; set; }
        public double TotalCessionRecue { get; set; }
        public int TotalArticles { get; set; }

        public List<ReportItem> TopFournisseurs { get; set; } = new();
        public List<ReportItem> TopProjets { get; set; } = new();

        public void OnGet()
        {
            string connString = _sqliteService.GetSqliteConnectionString();

            using (var conn = new SqliteConnection(connString))
            {
                conn.Open();

                // 1. حساب المبالغ الإجمالية
                TotalAchat = GetScalar(conn, "SELECT SUM(QTE * PAMP) FROM ST_ACHAT");
                TotalCessionFournie = GetScalar(conn, "SELECT SUM(QTE * PAMP) FROM ST_CESS");
                TotalCessionRecue = GetScalar(conn, "SELECT SUM(QTE * PAMP) FROM ST_CESSR");
                TotalArticles = (int)GetScalar(conn, "SELECT COUNT(*) FROM ST_STOCK");

                // 2. أفضل 10 موردين (ST_ACHAT + ST_FOURN)
                TopFournisseurs = GetTopItems(conn, @"
                    SELECT F.NOM as Name, SUM(A.QTE * A.PAMP) as Value 
                    FROM ST_ACHAT A JOIN ST_FOURN F ON A.COD_SOC = F.COD_SOC 
                    GROUP BY F.NOM ORDER BY Value DESC LIMIT 10");

                // 3. أفضل 10 مشاريع/ورشات (ST_CESS + ST_UNITE)
                TopProjets = GetTopItems(conn, @"
                    SELECT U.NOM as Name, SUM(C.QTE * C.PAMP) as Value 
                    FROM ST_CESS C JOIN ST_UNITE U ON C.COD_SOC = U.COD_SOC 
                    GROUP BY U.NOM ORDER BY Value DESC LIMIT 10");
            }
        }

        private double GetScalar(SqliteConnection conn, string sql)
        {
            var cmd = new SqliteCommand(sql, conn);
            var res = cmd.ExecuteScalar();
            return res != DBNull.Value ? Convert.ToDouble(res) : 0;
        }

        private List<ReportItem> GetTopItems(SqliteConnection conn, string sql)
        {
            var list = new List<ReportItem>();
            using var cmd = new SqliteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new ReportItem
                {
                    Name = reader["Name"].ToString(),
                    Value = Convert.ToDouble(reader["Value"])
                });
            }
            return list;
        }
    }

    public class ReportItem { public string Name { get; set; } public double Value { get; set; } }
}
