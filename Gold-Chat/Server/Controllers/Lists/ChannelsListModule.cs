using CommandClient;
using Server.Interfaces;
using Server.Interfaces.Utilities;
using Server.Modules.ResponseMessagesController;
using System.Collections.Generic;

namespace Server.Controllers.Lists
{
    class ChannelsListModule : ServerResponds, IClient, IBuildResponse
    {
        protected List<Channel> ChannelsList;
        protected List<Client> ListOfClientsOnline;

        private readonly ISeparateListOfStringToString SeparateList;

        public ChannelsListModule(ISeparateListOfStringToString separateList)
        {
            SeparateList = separateList;
        }

        public void Execute()
        {
            prepareResponse();
            Send.strMessage2 = SeparateList.separate(ChannelsList);
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
