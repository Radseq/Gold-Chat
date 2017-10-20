using Moq;
using Server.Interfaces.ClientLogin;
using Xunit;

namespace ServerTests
{
    public class GetClientPropertiesFromDBTests
    {
        Mock<IGetClientProperties> mock;

        public GetClientPropertiesFromDBTests()
        {
            mock = new Mock<IGetClientProperties>();
        }

        [Fact]
        public int GetPropertiesFromDB(string login, string password)
        {
            mock.Setup(x => x.GetUserProperties(login, password)).Returns(new string[] { "register_id", "email", "id_user", "login", "permission" });
            return 1;
        }

        [Fact]
        public int GetProperties(string login, string pass)
        {
            if (login == "" || string.IsNullOrWhiteSpace(login))
                Assert.False(true);
            if (pass == "" || string.IsNullOrWhiteSpace(pass))
                Assert.False(true);

            return GetPropertiesFromDB(login, pass);
        }

        [Fact]
        public void GetProperiesFromDB()
        {
            int result = GetProperties("user", "pass");
            Assert.Equal(result, 1);
        }
    }
}
