using Server.Interfaces;
using Server.Interfaces.CreateChannel;

namespace Server.Modules.CreateChannelModule
{

    /// <summary>
    /// TO DELETE, server always have full list of channels, maybe when i will make multi servers and these servers will not comunicate witch each order
    /// </summary>

    class SearchForExistingChannelDB : ISearchForExistingChannel
    {
        private readonly IDataBase DataBase;

        public SearchForExistingChannelDB(IDataBase dataBase)
        {
            DataBase = dataBase;
        }

        //read class note ....
        public string[] Search(Client client, string channelName)
        {
            //DataBase.bind(new string[] { "id_user_f", client.id.ToString(), "channel_n", channelName });
            //DataBase.manySelect("SELECT id_user_founder, channel_name FROM channel WHERE id_user_founder = @id_user_f AND channel_name = @channel_n");
            DataBase.bind("channel_n", channelName);
            DataBase.manySelect("SELECT id_user_founder, channel_name FROM channel WHERE channel_name = @channel_n");
            return DataBase.tableToRow();
        }
    }
}
