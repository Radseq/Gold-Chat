using Server.Interfaces;
using Server.Interfaces.ClientLogin;

namespace Server.Modules.ClientLoginModule
{
    public class GetClientPropertiesFromDB : IGetClientProperties
    {
        private readonly IDataBase DataBase;

        public GetClientPropertiesFromDB(IDataBase dataBase)
        {
            DataBase = dataBase;
        }

        public string[] GetUserProperties(string userName, string userPassword)
        {
            DataBase.bind(new string[] { "@userName", userName, "@password", userPassword });
            DataBase.manySelect("SELECT crc.code, u.email, u.id_user, u.login, u.permission FROM users u LEFT JOIN client_register_code crc ON u.id_user = crc.id_user WHERE u.login = @userName AND u.password = @password");
            return DataBase.tableToRow();
        }
    }
}
