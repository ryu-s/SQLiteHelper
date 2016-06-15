using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;
using System.Data.Common;
namespace ryu_s.Database
{
    public static class SQLiteHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbPath"></param>
        /// <returns></returns>
        public static SQLiteConnection CreateConnection(string dbPath)
        {
            return new SQLiteConnection($"Data Source={dbPath}");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="query"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static DbDataReader ExecuteReader(SQLiteConnection conn, string query, params object[] arg)
        {
            //ExecuteReader("select * from stream where provider_type = ?", "channel");
            if (conn == null)
                throw new ArgumentNullException("conn");

            using (var cmd = new SQLiteCommand(query, conn))
            {
                setParameter(cmd, query, arg);
                return cmd.ExecuteReader();
            }
        }
        /// <summary>
        /// 
        /// Usage:
        /// ExecuteReaderAsync(conn, "select * from stream where col1 = ?", "ryu");
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="query"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static Task<DbDataReader> ExecuteReaderAsync(SQLiteConnection conn, string query, params object[] arg)
        {
            if (conn == null)
                throw new ArgumentNullException("conn");

            using (var cmd = new SQLiteCommand(query, conn))
            {
                setParameter(cmd, query, arg);
                return cmd.ExecuteReaderAsync();
            }
        }
        public static int ExecuteNonQuery(SQLiteConnection conn, string query, params object[] arg)
        {
            int affectedLineCount = 0;
            using (var cmd = new SQLiteCommand(query, conn))
            {
                setParameter(cmd, query, arg);
                affectedLineCount = cmd.ExecuteNonQuery();
            }
            return affectedLineCount;
        }
        public static async Task<int> ExecuteNonQueryAsync(SQLiteConnection conn, string query, params object[] arg)
        {
            int affectedLineCount = 0;
            using (var cmd = new SQLiteCommand(query, conn))
            {
                setParameter(cmd, query, arg);
                affectedLineCount = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            return affectedLineCount;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static DataTable GetDataTableFrom(DbDataReader reader)
        {
            var dt = new DataTable();
            dt.Load(reader);
            reader.Close();
            return dt;
        }
        /// <summary>
        /// テーブルが存在するか
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static bool TableExists(SQLiteConnection conn, string tableName)
        {
            if (conn == null)
                throw new ArgumentNullException("conn");
            if (string.IsNullOrWhiteSpace(tableName))
                return false;

            string sql = "SELECT name FROM sqlite_master WHERE type='table' AND name= ?";

            using (var reader = ExecuteReader(conn, sql, tableName))
            {
                return reader.HasRows;
            }
        }

        /// <summary>
        /// このデータベースファイルに存在するテーブルの名前のリスト。
        /// </summary>
        /// <param name="dbFilename"></param>
        /// <returns></returns>
        public static List<string> GetTableNameList(SQLiteConnection conn)
        {
            const string colName = "name";
            const string sql = "SELECT " + colName + " FROM sqlite_master WHERE type='table'";
            var list = new List<string>();
            var reader = ExecuteReader(conn, sql);
            while (reader.Read())
            {
                list.Add(reader.GetString(0));
            }            
            return list;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static DbDataReader TableInfo(SQLiteConnection conn, string tableName)
        {
            var query = "PRAGMA table_info(" + tableName + ")";
            return ExecuteReader(conn, query);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tableName"></param>
        /// <param name="colName"></param>
        /// <returns></returns>
        public static bool ColumnExists(SQLiteConnection conn, string tableName, string colName)
        {
            var reader = TableInfo(conn, tableName);
            for (int i = 0; i < reader.FieldCount; i++)//VisibleFieldCountにするべき？
            {
                if (reader.GetName(i) == colName)
                    return true;
            }
            return false;
        }
        private class DbValue
        {
            public DbType Type { get; private set; }
            public object Value { get; private set; }
            public DbValue(DbType type, object value)
            {
                this.Type = type;
                this.Value = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static DbValue ConvertToDbType(object value)
        {
            if (value == null)
            {
                return new DbValue(DbType.String, null);
            }
            else if (value is string)
            {
                return new DbValue(DbType.String, value);
            }
            else if (value is Int32)
            {
                return new DbValue(DbType.Int32, value);
            }
            else if (value is float || value is double)
            {
                return new DbValue(DbType.Double, value);
            }
            else if (value is DateTime)
            {
                var dateTime = (DateTime)value;
                return new DbValue(DbType.String, dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            throw new ArgumentException("渡された引数の型が処理できない形式です。");
        }
        /// <summary>
        /// パラメータをセット (prepared statement)
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="query">実行するSQL</param>
        /// <param name="args">パラメータ</param>
        private static void setParameter(SQLiteCommand cmd, string query, object[] args)
        {
            cmd.CommandText = query;
            cmd.Parameters.Clear();
            if (args != null)
            {
                foreach (var arg in args)
                {
                    var parameter = cmd.CreateParameter();
                    var dbValue = ConvertToDbType(arg);
                    parameter.DbType = dbValue.Type;
                    parameter.Value = dbValue.Value;
                    cmd.Parameters.Add(parameter);
                }
            }
            cmd.Prepare();
        }
    }
}
