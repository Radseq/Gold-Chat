using Server.Interfaces;
using Server.Interfaces.IgnoreUser;
using System;

namespace Server.Modules.IgnoreUserModule
{
    class DeleteIgnoredUserFromDb : IDeleteIgnoredUser
    {
        private readonly IDataBase DataBase;

        public DeleteIgnoredUserFromDb(IDataBase dataBase)
        {
            DataBase = dataBase;
        }

        public bool Delete(Int64 ClientId, string nickofIgnoredUser)
        {
            DataBase.bind(new string[] { "idUser", ClientId.ToString(), "NameOfIgnored", nickofIgnoredUser });

            if (DataBase.executeNonQuery("DELETE FROM user_ignored WHERE id_user = @idUser AND id_user_ignored = (SELECT id_user FROM users WHERE login = @NameOfIgnored))") > 0)
                return true;
            else
                return false;
        }
    }
}
