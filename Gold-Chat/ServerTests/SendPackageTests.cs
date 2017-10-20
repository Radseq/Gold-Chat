using Moq;
using Server.Interfaces;

namespace ServerTests
{
    public class SendPackageTests
    {
        public void TestStrMessage()
        {
            var mock = new Mock<IServerSend>();
            mock.Setup(x => x.Send).Returns(new CommandClient.Data());
        }
    }
}
