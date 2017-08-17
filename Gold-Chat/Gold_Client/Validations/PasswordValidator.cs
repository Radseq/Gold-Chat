using System.Security;
using System.Text.RegularExpressions;

namespace Gold_Client.Validations
{
    public class PasswordValidator
    {
        /// <summary>
        /// 8 to 15 chars 
        /// At Last One Upper CHar
        /// At Last One Special Char like *
        /// At Last One Numeric Value
        /// </summary>

        const string PASSWORD_REGEX = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$";

        public bool validate(SecureString password)
        {
            return Regex.IsMatch(new System.Net.NetworkCredential(string.Empty, password).Password, PASSWORD_REGEX);
        }
    }
}
