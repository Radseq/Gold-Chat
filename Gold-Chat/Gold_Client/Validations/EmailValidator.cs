using System.Text.RegularExpressions;

namespace Gold_Client.Validations
{
    public class EmailValidator
    {
        const string EMAIL_REGEX = @"^[a-zA-Z][\w\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$";

        public bool validate(string email)
        {
            return Regex.IsMatch(email, EMAIL_REGEX);
        }
    }
}
