using Server.Interfaces;
using Server.Interfaces.ClientLists;
using System;
using System.Collections.Generic;

namespace Server.Modules.ListsToUserModule
{
    class GetFriendsList : IClientLists
    {
        private readonly IDataBase DataBase;

        public GetFriendsList(IDataBase dataBase)
        {
            DataBase = dataBase;
        }

        public List<string> GetList(Int64 id_user)
        {
            DataBase.bind("idUser", id_user.ToString());
            DataBase.manySelect("SELECT u.login FROM users u, user_friend uf WHERE uf.id_friend = u.id_user AND uf.id_user = @idUser");
            return DataBase.tableToColumn();
        }
    }
}
