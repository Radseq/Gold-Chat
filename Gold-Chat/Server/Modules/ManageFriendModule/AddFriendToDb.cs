using Server.Interfaces;
using Server.Interfaces.ManageFriend;
using System;

namespace Server.Modules.ManageFriendModule
{
    class AddFriendToDb : IAddFriend
    {
        private readonly IDataBase DataBase;

        public AddFriendToDb(IDataBase dataBase)
        {
            DataBase = dataBase;
        }

        public bool Add(Int64 clientId, Int64 friendId)
        {
            DataBase.bind(new string[] { "idUser", clientId.ToString(), "idFriend", friendId.ToString() });

            if (DataBase.executeNonQuery("INSERT INTO user_friend (id_user, id_friend) " + "VALUES (@idUser, @idFriend)") > 0)
                return true;
            else
                return false;
        }
    }
}
