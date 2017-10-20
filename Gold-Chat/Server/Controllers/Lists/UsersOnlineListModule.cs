using CommandClient;
using Server.Interfaces;
using Server.Modules.ResponseMessagesController;
using System.Collections.Generic;

namespace Server.Controllers.Lists
{
    class UsersOnlineListModule : ServerResponds, IClient, IBuildResponse
    {
        protected List<Channel> ChannelsList;
        protected List<Client> ListOfClientsOnline;

        public void Execute()
        {
            prepareResponse();
            foreach (Client client in ListOfClientsOnline)
                Send.strMessage2 += client.strName + "*";
        }

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelList = null)
        {
            Received = receive;
            Client = client;
            ListOfClientsOnline = clientList;
            ChannelsList = channelList;
        }

        public override void Response()
        {
            if (Send.strMessage2 != null || Send.strMessage3 != null)
                base.Response();
        }
    }
}
