using Server.Interfaces;
using Server.Interfaces.IgnoreUser;
using System;

namespace Server.Modules.IgnoreUserModule
{
    class AddIgnoredUserToDb : IAddIgnoredUser
    {
        private readonly IDataBase DataBase;

        public AddIgnoredUserToDb(IDataBase dataBase)
        {
            DataBase = dataBase;
        }

        public bool Add(Int64 ClientId, string nickofIgnoredUser)
        {
            DataBase.bind(new string[] { "idUser", ClientId.ToString(), "NameOfIgnored", nickofIgnoredUser });
            if (DataBase.executeNonQuery("INSERT INTO user_ignored (id_user, id_user_ignored) " + "VALUES (@idUser, (SELECT id_user FROM users WHERE login = @NameOfIgnored))") > 0)
                return true;
            else
                return false;
        }
    }
}
