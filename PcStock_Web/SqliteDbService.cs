using Microsoft.Data.Sqlite;
using System.Data;
using System.Data.Odbc;

namespace PcStock_Web
{
    public class SqliteDbService
    {
        private readonly string _sqliteConnString;
        private readonly ConfigService _configService;

        public SqliteDbService(ConfigService configService)
        {
            _configService = configService;
            // إنشاء قاعدة البيانات في مجلد bin الخاص بالتطبيق
            string dbPath = Path.Combine(AppContext.BaseDirectory, "PcStockLocal.db");
            _sqliteConnString = $"Data Source={dbPath}";
        }

        public string GetSqliteConnectionString() => _sqliteConnString;

        public async Task<(bool success, string message)> SyncTables(List<string> tableNames)
        {
            try
            {
                string dbfDirectory = _configService.GetDbPath();
                string dbfConnStr = $@"Driver={{Microsoft Access dBASE Driver (*.dbf, *.ndx, *.mdx)}};Dbq={dbfDirectory};ReadOnly=1;";

                using var dbfConn = new OdbcConnection(dbfConnStr);
                using var sqliteConn = new SqliteConnection(_sqliteConnString);

                await dbfConn.OpenAsync();
                await sqliteConn.OpenAsync();

                foreach (var tableName in tableNames)
                {
                    await SyncSingleTable(tableName, dbfConn, sqliteConn);
                }

                return (true, "Synchronisation terminée avec succès !");
            }
            catch (Exception ex)
            {
                return (false, "Erreur de synchronisation: " + ex.Message);
            }
        }

        private async Task SyncSingleTable(string tableName, OdbcConnection dbfConn, SqliteConnection sqliteConn)
        {
            // 1. جلب هيكلة ملف DBF
            var cmd = new OdbcCommand($"SELECT * FROM {tableName}.DBF", dbfConn);
            using var reader = cmd.ExecuteReader(CommandBehavior.SchemaOnly);
            var schemaTable = reader.GetSchemaTable();

            // 2. بناء جملة CREATE TABLE ديناميكياً
            string createTableSql = $"DROP TABLE IF EXISTS {tableName}; CREATE TABLE {tableName} (";
            List<string> colNames = new List<string>();
            List<string> colDefinitions = new List<string>();

            foreach (DataRow row in schemaTable.Rows)
            {
                string colName = row["ColumnName"].ToString();
                string dataType = row["DataType"].ToString();

                string sqliteType = "TEXT";
                if (dataType.Contains("Int")) sqliteType = "INTEGER";
                else if (dataType.Contains("Decimal") || dataType.Contains("Double") || dataType.Contains("Single")) sqliteType = "REAL";

                colDefinitions.Add($"[{colName}] {sqliteType}");
                colNames.Add(colName);
            }
            createTableSql += string.Join(", ", colDefinitions) + ");";

            using (var createCmd = new SqliteCommand(createTableSql, sqliteConn))
            {
                createCmd.ExecuteNonQuery();
            }

            // 3. نقل البيانات باستخدام Transaction للسرعة القصوى
            using var dataReader = new OdbcCommand($"SELECT * FROM {tableName}.DBF", dbfConn).ExecuteReader();
            using var transaction = sqliteConn.BeginTransaction();

            try
            {
                string insertSql = $"INSERT INTO {tableName} ([{string.Join("], [", colNames)}]) VALUES ({string.Join(", ", colNames.Select(c => "@" + c))})";
                var insertCmd = new SqliteCommand(insertSql, sqliteConn, transaction);

                while (dataReader.Read())
                {
                    insertCmd.Parameters.Clear();
                    foreach (var col in colNames)
                    {
                        insertCmd.Parameters.AddWithValue("@" + col, dataReader[col] ?? DBNull.Value);
                    }
                    insertCmd.ExecuteNonQuery();
                }
                transaction.Commit();
            }
            catch { transaction.Rollback(); throw; }
        }
    }
}