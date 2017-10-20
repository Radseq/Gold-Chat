using Server.Interfaces;
using Server.Interfaces.DeleteChannel;
using System;

namespace Server.Modules.DeleteChannelModule
{
    class GetAdminPassFromDB : IGetAdminPass
    {
        private readonly IDataBase DataBase;

        public GetAdminPassFromDB(IDataBase dataBase)
        {
            DataBase = dataBase;
        }

        public string GetPassword(string ChannelName, Int64 ClientID)
        {
            DataBase.bind(new string[] { "channelName", ChannelName, "idUserFounder", ClientID.ToString() });
            return DataBase.singleSelect("SELECT admin_password FROM channel WHERE channel_name = @channelName AND id_user_founder = @idUserFounder");
        }
    }
}
