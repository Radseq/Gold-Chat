using System;
using System.Collections.Generic;
using CommandClient;
using Server.ResponseMessages;

namespace Server.ClientService
{
    class ClientBanFromChannel : ServerResponds, IPrepareRespond
    {

        public void Execute()
        {
            prepareResponse();
            throw new NotImplementedException();
        }

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelList = null)
        {
            throw new NotImplementedException();
        }
    }
}
