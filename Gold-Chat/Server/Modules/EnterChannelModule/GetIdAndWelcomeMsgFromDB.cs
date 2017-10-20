using Server.Interfaces;
using Server.Interfaces.EnterChannel;
using System;

namespace Server.Modules.EnterChannelModule
{
    class GetIdAndWelcomeMsgFromDB : IGetIdAndWelcomeMsg
    {
        private readonly IDataBase DataBase;

        public GetIdAndWelcomeMsgFromDB(IDataBase dataBase)
        {
            DataBase = dataBase;
        }

        public string[] Get(string ChannelName, Int64 ClientId)
        {
            DataBase.bind(new string[] { "channelName", ChannelName, "idUser", ClientId.ToString() });
            DataBase.manySelect("SELECT uc.id_channel, c.welcome_message FROM channel c, user_channel uc WHERE uc.id_channel = c.id_channel AND c.channel_name = @channelName AND uc.id_user = @idUser");

            return DataBase.tableToRow();
        }
    }
}
