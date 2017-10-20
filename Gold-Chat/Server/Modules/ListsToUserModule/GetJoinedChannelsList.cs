using Server.Interfaces;
using Server.Interfaces.ClientLists;
using System;
using System.Collections.Generic;

namespace Server.Modules.ListsToUserModule
{
    class GetJoinedChannelsList : IClientLists
    {
        private readonly IDataBase DataBase;

        public GetJoinedChannelsList(IDataBase dataBase)
        {
            DataBase = dataBase;
        }

        public List<string> GetList(Int64 id_user)
        {
            DataBase.bind("idUser", id_user.ToString());
            DataBase.manySelect("SELECT c.channel_name FROM channel c, user_channel uc WHERE uc.id_channel = c.id_channel AND uc.id_user = @idUser");
            return DataBase.tableToColumn();
        }
    }
}
