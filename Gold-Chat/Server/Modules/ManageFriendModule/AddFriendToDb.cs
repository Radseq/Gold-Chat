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
            if (!IsFrindAllreadyAddedToDb(clientId, friendId))
            {
                DataBase.bind(new string[] { "IdUser", clientId.ToString(), "IdFriend", friendId.ToString() });
                if (DataBase.executeNonQuery("INSERT INTO user_friend (id_user, id_friend) VALUES (@IdUser, @IdFriend)") > 0)
                    return true;
                else
                    return false;
            }
            return false;
        }

        private bool IsFrindAllreadyAddedToDb(Int64 clientId, Int64 friendId)
        {
            DataBase.bind(new string[] { "idUser", clientId.ToString(), "idFriend", friendId.ToString() });

            string getId = DataBase.singleSelect("SELECT id_user_friend FROM user_friend WHERE id_user = @idUser AND id_friend = @idFriend)");
            if (getId != "")
                return true;
            else
                return false;
        }
    }
}
