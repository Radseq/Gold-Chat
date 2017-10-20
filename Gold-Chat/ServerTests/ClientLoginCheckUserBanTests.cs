using Moq;
using Server.Interfaces.ClientLogin;
using System;
using Xunit;

namespace ServerTests
{
    public class ClientLoginCheckUserBanTests
    {
        Mock<ICheckUserBan> mock;

        string banTime;
        string datatimeNow;

        public ClientLoginCheckUserBanTests()
        {
            mock = new Mock<ICheckUserBan>();
        }

        bool execute()
        {
            if (banTime == "" || string.IsNullOrWhiteSpace(banTime))
                return false;

            mock.Setup(x => x.CheckUserBan(1)).Returns(banTime);
            return true;
        }

        bool chackBan()
        {
            DateTime dt1 = DateTime.Parse(datatimeNow);
            DateTime dt2 = DateTime.Parse(banTime);

            if (dt1.Date > dt2.Date)
                return true;
            else
                return false;
        }

        [Fact]
        public void UserHaveABan()
        {
            datatimeNow = "2017-09-01 20:00:18";
            banTime = "2017-12-01 20:00:00";

            bool ban = execute();
            bool result = false;

            if (ban)
                result = chackBan();

            Assert.True(result);
        }

        [Fact]
        public void UserDontHaveABan()
        {
            datatimeNow = "2017-12-01 20:00:01";
            banTime = "2017-12-01 20:00:00";

            bool ban = execute();
            bool result = true;

            if (ban)
                result = chackBan();

            Assert.False(result);
        }
    }
}
