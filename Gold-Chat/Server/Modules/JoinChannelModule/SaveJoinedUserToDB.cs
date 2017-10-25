using Server.Interfaces;
using Server.Interfaces.JoinChannel;
using Server.Utilies;
using System;

namespace Server.Modules.JoinChannelModule
{
    class SaveJoinedUserToDB : ISaveJoinedUser
    {
        private readonly IDataBase DataBase;

        public SaveJoinedUserToDB(IDataBase dataBase)
        {
            DataBase = dataBase;
        }

        public int Save(Client client, Int64 idChannelDb)
        {
            DataBase.bind(new string[] { "idUser", client.id.ToString(), "idChannel", idChannelDb.ToString(), "joinDate", Utilities.getDataTimeNow() });
            return DataBase.executeNonQuery("INSERT INTO user_channel (id_user, id_channel, join_date) VALUES (@idUser, @idChannel, @joinDate)");
        }
    }
}
