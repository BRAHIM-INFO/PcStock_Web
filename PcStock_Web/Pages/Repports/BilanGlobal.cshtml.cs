using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace PcStock_Web.Pages.Repports // تأكد أن هذا الـ Namespace يطابق مجلدك
{
    public class BilanGlobalModel : PageModel
    {
        private readonly SqliteDbService _sqliteService;

        public BilanGlobalModel(SqliteDbService sqliteService)
        {
            _sqliteService = sqliteService;
        }

        // 1. خصائص البيانات الإحصائية
        public dynamic Stats { get; set; }
        public List<TopItem> TopChantiers { get; set; } = new();
        public List<TopItem> TopFournisseurs { get; set; } = new();

        // 2. خصائص الرسوم البيانية (التي كانت تسبب الخطأ)
        public string StockChartJson { get; set; } = "[]";
        public string AchatChartJson { get; set; } = "[]";

        // 3. فلاتر التاريخ
        [BindProperty(SupportsGet = true)]
        public DateTime? DateDeb { get; set; }
        [BindProperty(SupportsGet = true)]
        public DateTime? DateFin { get; set; }

        public void OnGet()
        {
            // تعيين تواريخ افتراضية إذا كانت فارغة (السنة الحالية)
            DateDeb ??= new DateTime(DateTime.Now.Year, 1, 1);
            DateFin ??= DateTime.Now;

            string connString = _sqliteService.GetSqliteConnectionString();
            string d1 = DateDeb?.ToString("yyyy-MM-dd");
            string d2 = DateFin?.ToString("yyyy-MM-dd");

            using (var conn = new SqliteConnection(connString))
            {
                conn.Open();

                // أ. حساب المبالغ الإجمالية للبطاقات الملونة
                Stats = new System.Dynamic.ExpandoObject();
                Stats.StockActuel = GetScalar(conn, "SELECT SUM(QTE * PAMP) FROM ST_STOCK");
                Stats.Achats = GetScalar(conn, $"SELECT SUM(QTE * PAMP) FROM ST_ACHAT WHERE DATE BETWEEN '{d1}' AND '{d2}'");
                Stats.CessionF = GetScalar(conn, $"SELECT SUM(QTE * PAMP) FROM ST_CESS WHERE DATE BETWEEN '{d1}' AND '{d2}'");
                Stats.Consos = GetScalar(conn, $"SELECT SUM(QTE * PAMP) FROM ST_CONSO WHERE DATE BETWEEN '{d1}' AND '{d2}'");
                Stats.Reinteg = GetScalar(conn, $"SELECT SUM(QTE * PAMP) FROM ST_CESSR WHERE DATE BETWEEN '{d1}' AND '{d2}'");

                // ب. حساب الأعداد (Count)
                Stats.NbArticles = (long)GetScalar(conn, "SELECT COUNT(*) FROM ST_STOCK WHERE QTE != 0");
                Stats.NbFact = (long)GetScalar(conn, $"SELECT COUNT(DISTINCT NO_FACA) FROM ST_ACHAT WHERE DATE BETWEEN '{d1}' AND '{d2}'");
                Stats.NbBSM = (long)GetScalar(conn, $"SELECT COUNT(DISTINCT NO_BC) FROM ST_CESS WHERE DATE BETWEEN '{d1}' AND '{d2}'") +
                              (long)GetScalar(conn, $"SELECT COUNT(DISTINCT NO_BS) FROM ST_CONSO WHERE DATE BETWEEN '{d1}' AND '{d2}'");

                // ج. جلب بيانات الجداول (Top 10)
                TopChantiers = GetTopItems(conn, $@"
                    SELECT U.NOM as Name, SUM(C.QTE * C.PAMP) as Val 
                    FROM ST_CESS C LEFT JOIN ST_UNITE U ON C.COD_SOC = U.COD_SOC 
                    WHERE C.DATE BETWEEN '{d1}' AND '{d2}'
                    GROUP BY U.NOM ORDER BY Val DESC LIMIT 10");

                TopFournisseurs = GetTopItems(conn, $@"
                    SELECT F.NOM as Name, SUM(A.QTE * A.PAMP) as Val 
                    FROM ST_ACHAT A LEFT JOIN ST_FOURN F ON A.COD_SOC = F.COD_SOC 
                    WHERE A.DATE BETWEEN '{d1}' AND '{d2}'
                    GROUP BY F.NOM ORDER BY Val DESC LIMIT 10");

                // د. جلب بيانات الرسوم البيانية وتجهيزها بصيغة JSON
                var stockByFam = GetTopItems(conn, @"
                    SELECT FAMILLE as Name, SUM(QTE * PAMP) as Val 
                    FROM ST_STOCK 
                    WHERE QTE > 0 
                    GROUP BY FAMILLE ORDER BY Val DESC LIMIT 10");
                StockChartJson = JsonSerializer.Serialize(stockByFam);

                var achatByFam = GetTopItems(conn, $@"
                    SELECT S.FAMILLE as Name, SUM(A.QTE * A.PAMP) as Val 
                    FROM ST_ACHAT A JOIN ST_STOCK S ON A.REF = S.REF 
                    WHERE A.DATE BETWEEN '{d1}' AND '{d2}' 
                    GROUP BY S.FAMILLE ORDER BY Val DESC LIMIT 10");
                AchatChartJson = JsonSerializer.Serialize(achatByFam);
            }
        }

        // دالة مساعدة لجلب قيمة واحدة من القاعدة
        private double GetScalar(SqliteConnection conn, string sql)
        {
            using var cmd = new SqliteCommand(sql, conn);
            var res = cmd.ExecuteScalar();
            return res != DBNull.Value ? Convert.ToDouble(res) : 0;
        }

        // دالة مساعدة لجلب قائمة بيانات (اسم وقيمة)
        private List<TopItem> GetTopItems(SqliteConnection conn, string sql)
        {
            var list = new List<TopItem>();
            using var cmd = new SqliteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new TopItem
                {
                    Name = reader["Name"]?.ToString() ?? "N/A",
                    Amount = Convert.ToDouble(reader["Val"])
                });
            }
            return list;
        }
    }

    public class TopItem
    {
        public string Name { get; set; }
        public double Amount { get; set; }
    }
}
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using Microsoft.Data.Sqlite;

//namespace PcStock_Web.Pages.Repports
//{
//    public class BilanGlobalModel : PageModel
//    {
//        private readonly SqliteDbService _sqliteService;
//        public BilanGlobalModel(SqliteDbService sqliteService) { _sqliteService = sqliteService; }

//        // خصائص البيانات
//        public dynamic Stats { get; set; }
//        public List<TopItem> TopChantiers { get; set; } = new();
//        public List<TopItem> TopFournisseurs { get; set; } = new();

//        [BindProperty(SupportsGet = true)]
//        public DateTime? DateDeb { get; set; }
//        [BindProperty(SupportsGet = true)]
//        public DateTime? DateFin { get; set; }

//        public void OnGet()
//        {
//            // إذا لم يتم اختيار تاريخ، نأخذ السنة الحالية افتراضياً
//            DateDeb ??= new DateTime(DateTime.Now.Year, 1, 1);
//            DateFin ??= DateTime.Now;

//            string connString = _sqliteService.GetSqliteConnectionString();
//            string d1 = DateDeb?.ToString("yyyy-MM-dd");
//            string d2 = DateFin?.ToString("yyyy-MM-dd");

//            using (var conn = new SqliteConnection(connString))
//            {
//                conn.Open();

//                // 1. حساب الإحصائيات الرئيسية (السرعة هنا في SQLite)
//                Stats = new System.Dynamic.ExpandoObject();
//                Stats.StockActuel = GetScalar(conn, "SELECT SUM(QTE * PAMP) FROM ST_STOCK");
//                Stats.Achats = GetScalar(conn, $"SELECT SUM(QTE * PAMP) FROM ST_ACHAT WHERE DATE BETWEEN '{d1}' AND '{d2}'");
//                Stats.CessionF = GetScalar(conn, $"SELECT SUM(QTE * PAMP) FROM ST_CESS WHERE DATE BETWEEN '{d1}' AND '{d2}'");
//                Stats.Consos = GetScalar(conn, $"SELECT SUM(QTE * PAMP) FROM ST_CONSO WHERE DATE BETWEEN '{d1}' AND '{d2}'");
//                Stats.Reinteg = GetScalar(conn, $"SELECT SUM(QTE * PAMP) FROM ST_CESSR WHERE DATE BETWEEN '{d1}' AND '{d2}'");

//                Stats.NbArticles = (long)GetScalar(conn, "SELECT COUNT(*) FROM ST_STOCK WHERE QTE != 0");
//                Stats.NbFact = (long)GetScalar(conn, $"SELECT COUNT(DISTINCT NO_FACA) FROM ST_ACHAT WHERE DATE BETWEEN '{d1}' AND '{d2}'");
//                Stats.NbBSM = (long)GetScalar(conn, $"SELECT COUNT(DISTINCT NO_BC) FROM ST_CESS WHERE DATE BETWEEN '{d1}' AND '{d2}'") +
//                              (long)GetScalar(conn, $"SELECT COUNT(DISTINCT NO_BS) FROM ST_CONSO WHERE DATE BETWEEN '{d1}' AND '{d2}'");

//                // 2. TOP 10 CHANTIERS
//                TopChantiers = GetTop(conn, $@"
//                    SELECT U.NOM as Name, SUM(C.QTE * C.PAMP) as Val 
//                    FROM ST_CESS C LEFT JOIN ST_UNITE U ON C.COD_SOC = U.COD_SOC 
//                    WHERE C.DATE BETWEEN '{d1}' AND '{d2}'
//                    GROUP BY U.NOM ORDER BY Val DESC LIMIT 10");

//                // 3. TOP 10 FOURNISSEURS
//                TopFournisseurs = GetTop(conn, $@"
//                    SELECT F.NOM as Name, SUM(A.QTE * A.PAMP) as Val 
//                    FROM ST_ACHAT A LEFT JOIN ST_FOURN F ON A.COD_SOC = F.COD_SOC 
//                    WHERE A.DATE BETWEEN '{d1}' AND '{d2}'
//                    GROUP BY F.NOM ORDER BY Val DESC LIMIT 10");
//            }
//        }

//        private double GetScalar(SqliteConnection conn, string sql)
//        {
//            var cmd = new SqliteCommand(sql, conn);
//            var res = cmd.ExecuteScalar();
//            return res != DBNull.Value ? Convert.ToDouble(res) : 0;
//        }

//        private List<TopItem> GetTop(SqliteConnection conn, string sql)
//        {
//            var l = new List<TopItem>();
//            using var cmd = new SqliteCommand(sql, conn);
//            using var r = cmd.ExecuteReader();
//            while (r.Read()) l.Add(new TopItem { Name = r["Name"].ToString(), Amount = Convert.ToDouble(r["Val"]) });
//            return l;
//        }
//    }

//    public class TopItem { public string Name { get; set; } public double Amount { get; set; } }
//}