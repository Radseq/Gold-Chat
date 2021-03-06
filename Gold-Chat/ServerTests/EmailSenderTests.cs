﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Server.Interfaces;

namespace Server.Tests
{
    [TestClass()]
    public class EmailSenderTests
    {
        Mock<ISendEmail> mock = new Mock<ISendEmail>();

        [TestMethod()]
        [ExpectedException(typeof(AssertFailedException))]
        public void SendEmailReturnFalse()
        {
            SetPropertiesEmailTest("", "", "", "");
        }

        [TestMethod()]
        public void SendEmailReturnTrue()
        {
            Assert.IsTrue(SetPropertiesEmailTest("username", "email", "subject", "message"));
        }

        [TestMethod()]
        public void SendEmailTest(/*bool SetPropertiesEmailTestResult*/)
        {
            bool result = false;
            if (SetPropertiesEmailTest("username", "email", "subject", "message"))
                result = true;

            mock.Setup(x => x.SendEmail()).Returns(result);
            Assert.AreEqual(result, true);
        }

        [TestMethod()]
        public bool SetPropertiesEmailTest(string username, string email, string subject, string message)
        {
            if (username == "" || string.IsNullOrWhiteSpace(username))
                Assert.Fail();
            if (email == "" || string.IsNullOrWhiteSpace(email))
                Assert.Fail();
            if (subject == "" || string.IsNullOrWhiteSpace(subject))
                Assert.Fail();
            if (message == "" || string.IsNullOrWhiteSpace(message))
                Assert.Fail();

            mock.Setup(x => x.SetProperties(username, email, subject, message));
            Assert.IsNotNull(mock.Object);
            return true;
        }
    }
}