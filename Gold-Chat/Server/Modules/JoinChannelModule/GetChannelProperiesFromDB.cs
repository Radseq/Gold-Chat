using Server.Interfaces;
using Server.Interfaces.JoinChannel;

namespace Server.Modules.JoinChannelModule
{
    class GetChannelProperiesFromDB : IGetChannelProperties
    {
        private readonly IDataBase DataBase;

        public GetChannelProperiesFromDB(IDataBase dataBase)
        {
            DataBase = dataBase;
        }

        public string[] Get(string channelName)
        {
            DataBase.bind("ChannelName", channelName);
            DataBase.manySelect("SELECT id_channel, welcome_Message, enter_password FROM channel WHERE channel_name = @ChannelName");
            return DataBase.tableToRow();
        }
    }
}
