using Server.Interfaces;
using Server.Interfaces.LostPassword;

namespace Server.Modules.LostPasswordModule
{
    class GetUserIdAndEmailFromDB : IGetUserIdAndEmail
    {
        private readonly IDataBase DataBase;

        public GetUserIdAndEmailFromDB(IDataBase dataBase)
        {
            DataBase = dataBase;
        }

        public string[] Get(string emailAdr)
        {
            DataBase.bind("Email", emailAdr);
            DataBase.manySelect("SELECT id_user, email FROM users WHERE email = @Email");
            return DataBase.tableToRow();
        }
    }
}
