using Gold_Client.Validations;
using Xunit;

namespace Gold_ClientTests
{
    public class EmailValidatorTests
    {
        readonly EmailValidator validator;
        string email;

        public EmailValidatorTests()
        {
            validator = new EmailValidator();
        }

        public bool execute()
        {
            return validator.validate(email);
        }

        [Fact]
        public void validateGmailEmailAdress()
        {
            email = "NameSurname@gmail.com";
            bool result = execute();
            Assert.True(result);
        }
    }
}
