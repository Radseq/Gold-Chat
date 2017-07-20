using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Server.Tests
{
    [TestClass()]
    public class DataBaseManagerTests
    {
        [TestMethod()]
        public void tableToRowTest()
        {
            DataBaseManager dbManager = DataBaseManager.Instance;
            dbManager.singleSelect("SELECT id_user FROM users");
            string[] actual_result = dbManager.tableToRow();
            Assert.IsNotNull(actual_result);
        }
    }
}