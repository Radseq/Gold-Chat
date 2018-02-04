using Moq;
using Server.Interfaces.Server;

namespace ServerTests
{
    public class SendPackageTests
    {
        public void TestStrMessage()
        {
            var mock = new Mock<IDataSend>();
            mock.Setup(x => x.Send).Returns(new CommandClient.Data());
        }
    }
}
