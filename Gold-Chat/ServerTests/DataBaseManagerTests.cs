using Moq;
using Server;
using Server.Interfaces;
using Xunit;

namespace ServerTests
{
    public class DataBaseManagerTests
    {
        Mock<IDataBase> mock = new Mock<IDataBase>();

        public int DatabaseQuery(string query)
        {
            if (query == "" || string.IsNullOrWhiteSpace(query))
                return 0;

            mock.Setup(x => x.executeNonQuery(query)).Returns(1); // It.IsAny<string>()
            return 1;
        }

        //[ExpectedException(typeof(AssertFailedException))]
        [Fact]
        public void QueryIsNull()
        {
            DatabaseQuery("");
        }

        [Fact]
        public void CreateUser()
        {
            var mock = new Mock<IClient>();
            mock.Setup(x => x.Client).Returns(new Client());
            mock.Object.Client.id = 1;
            mock.Object.Client.strName = IsUserExists();
            Assert.Equal(1, mock.Object.Client.id);
            Assert.Equal("TestUser", mock.Object.Client.strName);
        }

        public int getUserFromDb(string UserName)
        {
            return DatabaseQuery($"SELECT login FROM users WHERE login = {UserName}");
        }

        [Fact]
        public string IsUserExists()
        {
            int result = getUserFromDb("TestUser");
            return "TestUser";
        }
    }
}