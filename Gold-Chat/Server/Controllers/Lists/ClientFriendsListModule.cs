using CommandClient;
using Server.Interfaces;
using Server.Interfaces.ClientLists;
using Server.Interfaces.Utilities;
using Server.Modules.ResponseMessagesController;
using System.Collections.Generic;

namespace Server.Controllers.Lists
{
    class ClientFriendsListModule : ServerResponds, IClient, IBuildResponse
    {
        private readonly IClientLists clientLists;
        private readonly ISeparateListOfStringToString separateList;

        protected List<Channel> ChannelsList;
        protected List<Client> ListOfClientsOnline;

        public ClientFriendsListModule(IClientLists ClientLists, ISeparateListOfStringToString SeparateList)
        {
            clientLists = ClientLists;
            separateList = SeparateList;
        }

        public void Execute()
        {
            prepareResponse();
            Send.strMessage2 = separateList.separate(clientLists.GetList(Client.id));
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
