using Server.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace Server.ClientService
{
    //cache
    //get channels at server start
    public class GetListOfChannelsController : IGetListOfChannels
    {
        private readonly IDataBase DataBase;

        public GetListOfChannelsController(IDataBase dataBase)
        {
            DataBase = dataBase;
        }

        public List<Channel> Get()
        {
            List<Channel> channels = new List<Channel>();
            string query = "SELECT id_channel, channel_name, id_user_founder FROM channel";
            DataTable dt = DataBase.manySelect(query);

            foreach (DataRow row in dt.Rows)
            {
                Channel channel = new Channel(row.Field<Int64>("id_channel"), row.Field<string>("channel_name"), row.Field<Int64>("id_user_founder"));
                if (!channels.Contains(channel))
                    channels.Add(channel);
            }
            return channels;
        }
    }
}
