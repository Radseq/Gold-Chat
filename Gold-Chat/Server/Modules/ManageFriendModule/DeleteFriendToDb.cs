using Server.Interfaces;
using Server.Interfaces.ManageFriend;
using System;

namespace Server.Modules.ManageFriendModule
{
    class DeleteFriendToDb : IDeleteFriend
    {
        private readonly IDataBase DataBase;

        public DeleteFriendToDb(IDataBase dataBase)
        {
            DataBase = dataBase;
        }

        public bool Delete(Int64 clientId, Int64 friendId)
        {
            DataBase.bind(new string[] { "idUser", clientId.ToString(), "idFriend", friendId.ToString() });

            if (DataBase.executeNonQuery("DELETE FROM user_friend WHERE id_user = @idUser AND id_friend = @idFriend") > 0)
                return true;
            else
                return false;
        }
    }
}
