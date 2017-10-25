using System.Text.RegularExpressions;

namespace Gold_Client.Validations
{
    public class LoginValidator
    {
        const string LOGIN_REGEX = @"^(?=[a-z])[-\w.]{0,23}([a-zA-Z\d])$";

        public bool validate(string login)
        {
            return Regex.IsMatch(login, LOGIN_REGEX);
        }
    }
}
