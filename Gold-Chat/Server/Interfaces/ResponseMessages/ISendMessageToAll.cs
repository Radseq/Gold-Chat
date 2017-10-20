using CommandClient;
using System.Collections.Generic;

namespace Server.Interfaces.ResponseMessages
{
    interface ISendMessageToAll
    {
        void ResponseToAll(Client client, Data Send, List<Client> ListOfClientsOnline);
    }
}
