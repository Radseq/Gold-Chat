using Server.Interfaces;
using Server.Interfaces.ClientLists;
using System;
using System.Collections.Generic;

namespace Server.Modules.ListsToUserModule
{
    class GetIgnoredUsersList : IClientLists
    {
        private readonly IDataBase DataBase;

        public GetIgnoredUsersList(IDataBase dataBase)
        {
            DataBase = dataBase;
        }

        public List<string> GetList(Int64 id_user)
        {
            DataBase.bind("idUser", id_user.ToString());
            DataBase.manySelect("SELECT u.login FROM users u, user_ignored ui WHERE ui.id_user_ignored = u.id_user AND ui.id_user = @idUser");
            return DataBase.tableToColumn();
        }
    }
}
