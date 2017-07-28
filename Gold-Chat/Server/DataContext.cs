using CommandClient;
using Server.Interfaces;
using System.Collections.Generic;

namespace Server
{
    public class DataContext
    {
        private IBuildResponse buildRespond = null;

        public void SetContext(IBuildResponse BuildRespond)
        {
            buildRespond = BuildRespond;
        }

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelList = null)
        {
            buildRespond.Load(client, receive, clientList, channelList);
        }

        public void Execute()
        {
            buildRespond.Execute();
        }

        public void Response()
        {
            buildRespond.Response();
        }
    }
}
