using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Server.Interfaces;

namespace ServerTests
{
    [TestClass()]
    class SendPackageTests
    {
        [TestMethod()]
        public void TestStrMessage()
        {
            var mock = new Mock<IServerSend>();
            mock.Setup(x => x.Send).Returns(new CommandClient.Data());
        }
    }
}
