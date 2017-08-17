using Server.Interfaces;
using Server.Interfaces.ClientLogin;

namespace Server.ClientLoginModule
{
    class GetClientPropertiesFromDB : IGetClientProperties
    {
        private readonly IDataBase DataBase;

        public GetClientPropertiesFromDB(IDataBase dataBase)
        {
            DataBase = dataBase;
        }

        public string[] GetUserProperties(string userName, string userPassword)
        {
            DataBase.bind(new string[] { "@userName", userName, "@password", userPassword });
            DataBase.manySelect("SELECT register_id, email, id_user, login, permission FROM users WHERE login = @userName AND password = @password");
            return DataBase.tableToRow();
        }
    }
}
