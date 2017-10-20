using Server.Interfaces;
using Server.Interfaces.DeleteChannel;
using System;

namespace Server.Modules.DeleteChannelModule
{
    class DeleteChannelFromDB : IDeleteChannel
    {
        private readonly IDataBase DataBase;

        public DeleteChannelFromDB(IDataBase dataBase)
        {
            DataBase = dataBase;
        }

        public bool Delete(string ChannelName, Int64 ClientId)
        {
            DataBase.bind(new string[] { "channelName", ChannelName, "idUser", ClientId.ToString() });
            if (DataBase.executeNonQuery("DELETE FROM channel WHERE channel.channel_name = @channelName AND channel.id_user_founder = @idUser") == 1)
                return true;
            return false;
        }
    }
}
