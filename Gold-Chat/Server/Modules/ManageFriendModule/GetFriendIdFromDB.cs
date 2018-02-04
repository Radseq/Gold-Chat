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

        public Int64 GetFriendId(string FriendName)
        {
            DataBase.bind("FriendName", FriendName);

            try
            {
                string result = DataBase.singleSelect("SELECT id_user FROM users WHERE login = @FriendName");
                return Int64.Parse(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 0;
            }
        }
    }
}
