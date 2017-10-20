using Server.Interfaces;
using Server.Interfaces.ChangePassword;

namespace Server.Modules.ChangePasswordModule
{
    class GetOldPasswordFromDB : IGetOldPassword
    {
        private readonly IDataBase DataBase;

        public GetOldPasswordFromDB(IDataBase dataBase)
        {
            DataBase = dataBase;
        }

        public string GetPassword(string clientName)
        {
            DataBase.bind("userName", clientName);
            return DataBase.singleSelect("SELECT password FROM users WHERE login = @userName");
        }
    }
}
