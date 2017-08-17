using Gold_Client.Validations;
using System.Security;
using Xunit;

namespace Gold_ClientTests
{
    public class PasswordValidatorTests
    {
        readonly PasswordValidator validator;
        SecureString SecurePassword = new SecureString();

        public PasswordValidatorTests()
        {
            validator = new PasswordValidator();
        }

        public bool execute()
        {
            return validator.validate(SecurePassword);
        }

        private void addStringToSecurePassword(string password)
        {
            foreach (char x in password)
                SecurePassword.AppendChar(x);
        }

        [Fact]
        public void validateGmailEmailAdress()
        {
            addStringToSecurePassword("somePass1*");
            bool result = execute();
            Assert.True(result);
        }
    }
}
