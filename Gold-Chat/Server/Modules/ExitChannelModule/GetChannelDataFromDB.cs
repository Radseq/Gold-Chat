using Server.Interfaces;
using Server.Interfaces.ExitChannel;
using System;

namespace Server.Modules.ExitChannelModule
{
    class GetChannelDataFromDB : IGetChannelData
    {
        private readonly IDataBase DataBase;

        public GetChannelDataFromDB(IDataBase dataBase)
        {
            DataBase = dataBase;
        }

        public string[] Get(string ChannelName, Int64 ClientId)
        {
            DataBase.bind(new string[] { "channelName", ChannelName, "idUser", ClientId.ToString() });
            DataBase.manySelect("SELECT uc.id_channel, c.id_user_founder FROM channel c, user_channel uc WHERE uc.id_channel = c.id_channel AND c.channel_name = @channelName AND uc.id_user = @idUser");
            return DataBase.tableToRow();
        }
    }
}
