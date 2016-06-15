using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ryu_s.Database;
namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            if (System.IO.File.Exists("test"))
            {
                System.IO.File.Delete("test");
            }
            var conn = SQLiteHelper.CreateConnection("test");
            conn.Open();
            var n = SQLiteHelper.ExecuteNonQuery(conn, "CREATE TABLE ta(name Text, age int)");
            Assert.AreEqual(0, n);
            SQLiteHelper.ExecuteNonQuery(conn, "CREATE TABLE ku(name Text, age int)");
            n = SQLiteHelper.ExecuteNonQuery(conn, "INSERT INTO ta VALUES (?,?)", "ryu", 20);
            Assert.AreEqual(1, n);
            Assert.AreEqual(true, SQLiteHelper.TableExists(conn, "ta"));
            Assert.AreEqual(false, SQLiteHelper.TableExists(conn, "ff"));
            var list = SQLiteHelper.GetTableNameList(conn);
            Assert.AreEqual(true,list.Contains("ta"));
            Assert.AreEqual(2, list.Count);
        }
    }
}
