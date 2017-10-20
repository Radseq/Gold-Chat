using Server.Interfaces;
using Server.Interfaces.ManageFriend;
using System;

namespace Server.Modules.ManageFriendModule
{
    class GetFriendIdFromDB : IGetFriendId
    {
        private readonly IDataBase DataBase;

        public GetFriendIdFromDB(IDataBase dataBase)
        {
            DataBase = dataBase;
        }

        public long GetFriendId(string FriendName)
        {
            DataBase.bind("FriendName", FriendName);
            return Int64.Parse(DataBase.singleSelect("SELECT id_user FROM users WHERE login = @FriendName"));
        }
    }
}
