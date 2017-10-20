using Server.Interfaces;
using Server.Interfaces.ClientSendActiveCode;

namespace Server.Modules.ActiveCodeModule
{
    public class ActivCodeGetDB : IActiveCodeGet
    {
        private readonly IDataBase DataBase;
        private string[] getCodeAndEmailFromDb;

        public ActivCodeGetDB(IDataBase dataBase)
        {
            DataBase = dataBase;
        }

        public bool DeleteRegistrationCode(string userEmail)
        {
            DataBase.bind("email", userEmail);
            int updated = DataBase.executeNonQuery("DELETE FROM client_register_code WHERE id_user = (SELECT id_user from users WHERE email = @email)");

            if (updated > 0)
                return true;
            else
                return false;
        }

        public void GetRegisterCode(string UserName)
        {
            DataBase.bind(new string[] { "@userName", UserName });
            DataBase.manySelect("SELECT crc.code, u.email FROM users u LEFT JOIN client_register_code crc ON u.id_user = crc.id_user WHERE u.login = @userName");
            getCodeAndEmailFromDb = DataBase.tableToRow();
        }

        public string GetEmail()
        {
            return (getCodeAndEmailFromDb[1] != null) ? getCodeAndEmailFromDb[1] : null;
        }

        public bool ComprareCode(string Code)
        {
            if (getCodeAndEmailFromDb[0] != null || getCodeAndEmailFromDb[0] == Code) return true;
            else return false;
        }

        public string ReturnRegisterCode()
        {
            return (getCodeAndEmailFromDb[0] != null) ? getCodeAndEmailFromDb[0] : null;
        }
    }
}
