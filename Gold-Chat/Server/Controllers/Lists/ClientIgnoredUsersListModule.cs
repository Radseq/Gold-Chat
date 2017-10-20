using CommandClient;
using Server.Interfaces;
using Server.Interfaces.ClientLists;
using Server.Modules.ResponseMessagesController;
using System.Collections.Generic;

namespace Server.Controllers.Lists
{
    class ClientIgnoredUsersListModule : ServerResponds, IClient, IBuildResponse
    {
        private readonly IClientLists clientLists;

        protected List<Channel> ChannelsList;
        protected List<Client> ListOfClientsOnline;

        public ClientIgnoredUsersListModule(IClientLists ClientLists)
        {
            clientLists = ClientLists;
        }

        public void Execute()
        {
            prepareResponse();

            foreach (string ignoredUser in clientLists.GetList(Client.id))
            {
                Send.strMessage2 += ignoredUser + "*";
                Client.ignoredUsers.Add(ignoredUser);
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
