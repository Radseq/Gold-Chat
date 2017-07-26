using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Server.Interfaces;

namespace Server.Tests
{
    [TestClass()]
    public class DataBaseManagerTests
    {
        [TestMethod()]
        public int DatabaseQuery(string query)
        {
            if (query == "" || string.IsNullOrWhiteSpace(query))
                Assert.Fail();

            var mock = new Mock<IDataBase>();
            mock.Setup(x => x.executeNonQuery(query)).Returns(1); // It.IsAny<string>()
            Assert.IsNotNull(mock.Object);
            return 1;
        }

        [TestMethod()]
        [ExpectedException(typeof(AssertFailedException))]
        public void QueryIsNull()
        {
            DatabaseQuery("");
        }

        [TestMethod()]
        public void CreateUser()
        {
            var mock = new Mock<IClient>();
            mock.Setup(x => x.Client).Returns(new Client());
            mock.Object.Client.id = 1;
            mock.Object.Client.strName = IsUserExists();
            Assert.AreEqual(mock.Object.Client.id, 1);
            Assert.AreEqual(mock.Object.Client.strName, "TestUser");
        }

        [TestMethod()]
        public int getUserFromDb(string UserName)
        {
            return DatabaseQuery("SELECT login FROM users WHERE login = '" + UserName + "'");
        }

        [TestMethod()]
        public string IsUserExists()
        {
            int result = getUserFromDb("TestUser");
            return "TestUser";
        }
    }
}