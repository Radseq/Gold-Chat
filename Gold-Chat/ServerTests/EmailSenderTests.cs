using Moq;
using Server.Interfaces;
using Xunit;

namespace ServerTests
{
    public class EmailSenderTests
    {
        Mock<ISendEmail> mock = new Mock<ISendEmail>();

        //[ExpectedException(typeof(AssertFailedException))]
        [Fact]
        public void SendEmailReturnFalse()
        {
            SetPropertiesEmailTest("", "", "", "");
        }

        [Fact]
        public void SendEmailReturnTrue()
        {
            Assert.True(SetPropertiesEmailTest("username", "email", "subject", "message"));
        }

        [Fact]
        public void SendEmailTest(/*bool SetPropertiesEmailTestResult*/)
        {
            bool result = false;
            if (SetPropertiesEmailTest("username", "email", "subject", "message"))
                result = true;

            mock.Setup(x => x.SendEmail()).Returns(result);
            Assert.True(result);
        }

        public bool SetPropertiesEmailTest(string username, string email, string subject, string message)
        {
            if (username == "" || string.IsNullOrWhiteSpace(username))
                Assert.Empty(username);
            if (email == "" || string.IsNullOrWhiteSpace(email))
                Assert.Empty(email);
            if (subject == "" || string.IsNullOrWhiteSpace(subject))
                Assert.Empty(subject);
            if (message == "" || string.IsNullOrWhiteSpace(message))
                Assert.Empty(message);

            mock.Setup(x => x.SetProperties(username, email, subject, message));
            Assert.NotNull(mock.Object);
            return true;
        }
    }
}