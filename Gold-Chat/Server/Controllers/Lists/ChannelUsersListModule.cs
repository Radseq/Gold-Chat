using CommandClient;
using Server.Interfaces;
using Server.Modules.ResponseMessagesController;
using Server.Utilies;
using System.Collections.Generic;

namespace Server.Controllers.Lists
{
    class ChannelUsersListModule : ServerResponds, IClient, IBuildResponse
    {
        protected List<Channel> ChannelsList;
        protected List<Client> ListOfClientsOnline;

        public void Execute()
        {
            prepareResponse();

            Channel channel = ChannelGets.getChannelByName(ChannelsList, Received.strMessage2);
            if (channel != null)
            {
                foreach (string userName in channel.Users)
                    Send.strMessage3 += userName + "*";
            }
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
