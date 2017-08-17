using Gold_Client.Validations;
using Xunit;

namespace Gold_ClientTests
{
    public class LoginValidatorTests
    {
        readonly LoginValidator validator;
        string login;

        public LoginValidatorTests()
        {
            validator = new LoginValidator();
        }

        public bool execute()
        {
            return validator.validate(login);
        }

        [Fact]
        public void validateGmailEmailAdress()
        {
            login = "Login";
            bool result = execute();
            Assert.True(result);
        }
    }
}
